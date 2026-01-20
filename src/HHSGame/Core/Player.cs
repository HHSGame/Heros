using HHSGame.Core.Classes;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Stats;
using HHSGame.Utils;
using Terminal.Gui.Drawing;

namespace HHSGame.Core
{
    public class Player : IGameActor, ICombatant, ITrader
    {
        private const int FOVRadius = 7;
        private readonly HashSet<Coordinate> visibleTiles = [];
        private readonly List<ActiveEffect> activeEffects = [];

        private readonly CollisionSystem collisionSystem;
        private readonly InventoryManager inventoryManager;
        private readonly ItemManager itemManager;
        private readonly MapState mapState;
        private readonly Random random;

        private Weapon? equippedWeapon;
        private Armor? equippedArmor;

        public Player(int x, int y, GameContext context, Attributes? attributes = null, Skills? skills = null)
        {
            X = x;
            Y = y;
            collisionSystem = context.CollisionSystem;
            inventoryManager = context.InventoryManager;
            itemManager = context.ItemManager;
            mapState = context.MapState;
            random = context.Random;

            Attributes baseAttributes = attributes ?? new Attributes
            {
                Strength = 5,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills baseSkills = skills ?? new Skills();
            Stats = new CharacterStats(baseAttributes, baseSkills);
        }

        public int X { get; set; }
        public int Y { get; set; }

        public CharacterStats Stats { get; private set; }
        public ActionSequence ActionSequence { get; } = new();
        public InventoryManager Inventory => inventoryManager;

        public int Health => Stats.CurrentHp;
        public int MaxHealth => Stats.MaxHp;
        public int Sanity => Stats.CurrentSp;
        public int MaxSanity => Stats.MaxSp;
        public int CurrentAp => Stats.CurrentAp;
        public int MaxAp => Stats.MaxAp;
        public int Experience => Stats.Progression.Experience;
        public int Level => Stats.Progression.Level;
        public int EvasionBonus => Stats.EvasionBonus;

        public int ArmorValue => equippedArmor?.ArmorValue ?? 0;
        public Weapon EquippedWeapon => equippedWeapon ?? Classes.Weapons.UnknownWeapon;

        public Coordinate Position => new(X, Y);
        public char Glyph => '☭';
        public string Name => I18n.T("HHS.Core.Player.Name");
        public Terminal.Gui.Drawing.Attribute Attribute => new(Color.BrightYellow, Color.Red);

        public void ApplyBaseStats(Attributes attributes, Skills skills)
        {
            Attributes clonedAttributes = attributes with { };
            Skills clonedSkills = skills.Clone();
            Stats = new CharacterStats(clonedAttributes, clonedSkills, Stats.Progression);
        }

        public void ResetTurn(bool includeAp = true, bool updateFov = true)
        {
            Stats.ResetTurn(includeAp);
            TickEffects();
            if (updateFov)
            {
                UpdateFOV();
            }
        }

        public void EndTurn()
        {
            Stats.EndTurn();
        }

        public void UpdateFOV()
        {
            visibleTiles.Clear();
            CalculateFOV(Position, FOVRadius);
            mapState.MarkVisibleTiles(visibleTiles);
            Events.RaiseSurroundingsChange((X, Y), FOVRadius, SurroundingsChangeType.Movement);
        }

        private void CalculateFOV(Coordinate position, int radius)
        {
            visibleTiles.Clear();

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy > radius * radius)
                    {
                        continue;
                    }

                    Coordinate target = Position.Target(dx, dy);

                    if (!mapState.IsInBounds(target))
                    {
                        continue;
                    }

                    if (HasLineOfSight(position, target))
                    {
                        visibleTiles.Add(target);
                    }
                }
            }

            visibleTiles.Add(position);
        }

        private bool HasLineOfSight(Coordinate position, Coordinate target)
        {
            (int x0, int y0) = position;
            (int x1, int y1) = target;
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                visibleTiles.Add(new(x0, y0));

                if (!mapState.IsTransparent(x0, y0))
                {
                    return true;
                }

                if (x0 == x1 && y0 == y1)
                {
                    return true;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void TickEffects()
        {
            foreach (ActiveEffect effect in activeEffects.ToArray())
            {
                effect.Duration--;
                if (effect.Duration <= 0)
                {
                    activeEffects.Remove(effect);
                }
                else
                {
                    effect.ApplyEffect(this);
                }
            }
        }

        public void Move(Move move)
        {
            (int newX, int newY) = Position.Move(move.ToVec());

            if (collisionSystem.CanMoveTo(newX, newY, this))
            {
                X = newX;
                Y = newY;
                UpdateFOV();
                Events.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
            else
            {
                Events.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
        }

        public void Attack(Enemy enemy)
        {
            if (IsStunned())
            {
                return;
            }

            CombatResolver.ResolveAttack(this, enemy, EquippedWeapon, random);

            if (enemy.IsDead)
            {
                AddExperience(enemy.ExperienceValue);
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsStunned())
            {
                return;
            }

            Events.RaiseGameMessage(I18n.T("PlayerTakesDamage", damage));
            Stats.ApplyDamage(damage);

            if (Stats.CurrentHp <= 0)
            {
                Events.RaiseGameMessage(I18n.T("PlayerDefeated"));
                Die();
            }
        }

        public void Heal(int amount)
        {
            Stats.Heal(amount);
        }

        public void AddExperience(int amount)
        {
            Stats.Progression.TryAddExperience(amount, Stats.Attributes);
        }

        public void AddItem(Item item)
        {
            inventoryManager.AddItem(item);
        }

        public void EquipWeapon(Weapon weapon)
        {
            if (equippedWeapon != null)
            {
                inventoryManager.AddItem(equippedWeapon);
            }
            equippedWeapon = weapon;
            Events.RaiseGameMessage(I18n.T("EquippedWeapon", weapon.Name));
        }

        public void EquipArmor(Armor armor)
        {
            if (equippedArmor != null)
            {
                inventoryManager.AddItem(equippedArmor);
            }
            equippedArmor = armor;
            Events.RaiseGameMessage(I18n.T("EquippedArmor", armor.Name));
        }

        public void PickupItems()
        {
            List<Item> items = itemManager.GetItemsAt(X, Y);
            if (items.Count == 0)
            {
                Events.RaiseGameMessage(I18n.T("NoItemsToPickUp"));
                return;
            }

            itemManager.RemoveItemsAt(X, Y);
            foreach (Item item in items)
            {
                AddItem(item);
            }
            Events.RaiseSurroundingsChange((X, Y), FOVRadius, SurroundingsChangeType.PickUpLoot);
        }

        public void ApplyEffect(ActiveEffect effect)
        {
            activeEffects.Add(effect);
        }

        public bool IsStunned()
        {
            return activeEffects.Any(e => e is StunEffect);
        }

        private void Die()
        {
            Stats.ApplyDamage(Stats.CurrentHp);
        }
    }
}
