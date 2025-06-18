using Terminal.Gui.Drawing;
using HHSGame.Utils;
using HHSGame.Core.Combat;
using HHSGame.Core.Map;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;

namespace HHSGame.Core
{
    public class Player(int x, int y, GameContext context) : IGameActor
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int Health { get; internal set; } = 100;
        public int MaxHealth { get; internal set; } = 100;
        public int Strength { get; internal set; } = 10;
        public int Defense { get; internal set; } = 5;
        public int Experience { get; private set; }
        public int Level { get; private set; } = 1;

        public Coordinate Position => new (X, Y);

        public char Glyph => '☭';
        public string Name => I18n.GetString("HHS.Core.Player.Name");

        public Terminal.Gui.Drawing.Attribute Attribute => new(Color.BrightYellow, Color.Red);

        private readonly List<ActiveEffect> activeEffects = [];
        private readonly CollisionSystem collisionSystem = context.CollisionSystem;
        private readonly InventoryManager inventoryManager = context.InventoryManager;
        private readonly ItemManager itemManager = context.ItemManager;
        private readonly MapState mapState = context.MapState;
        private readonly TurnManager turnManager = context.TurnManager;
        private Weapon? equippedWeapon;
        private Armor? equippedArmor;

        private const int FOVRadius = 7;
        private readonly HashSet<Coordinate> visibleTiles = [];

        public void UpdateFOV()
        {
            visibleTiles.Clear();
            CalculateFOV(Position, FOVRadius);
            mapState.MarkVisibleTiles(visibleTiles);
            EventSystem.RaiseSurroundingsChange((X, Y), FOVRadius, SurroundingsChangeType.Movement);
        }

        private void CalculateFOV(Coordinate position, int radius)
        {
            visibleTiles.Clear();

            // Check all tiles within radius
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Skip if outside radius
                    if (dx * dx + dy * dy > radius * radius)
                    {
                        continue;
                    }

                    Coordinate target = Position.Target(dx, dy);

                    if (!mapState.IsInBounds(target))
                    {
                        continue;
                    }

                    // Check line of sight
                    if (HasLineOfSight(position, target))
                    {
                        visibleTiles.Add(target);
                    }
                }
            }

            // Always see current position
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
                // Add current tile to visible tiles
                visibleTiles.Add(new(x0, y0));

                // If we hit an opaque tile, stop here but include it
                if (!mapState.IsTransparent(x0, y0))
                {
                    return true;
                }

                // If we reached the target, we have line of sight
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

        public void Update()
        {
            UpdateFOV();

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
            if (!turnManager.IsPlayerTurn())
            {
                return;
            }

            Update();

            (int newX, int newY) = Position.Move(move.ToVec());

            if (collisionSystem.CanMoveTo(newX, newY, this))
            {
                IGameActor? collision = collisionSystem.GetCollisionAt(newX, newY);
                if (collision is Enemy enemy)
                {
                    Attack(enemy);
                }
                else
                {
                    X = newX;
                    Y = newY;
                    UpdateFOV();
                    EventSystem.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
                }
            }
            else
            {
                EventSystem.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
            turnManager.EndPlayerTurn();
        }

        public void Attack(Enemy enemy)
        {
            if (IsStunned())
            {
                return;
            }

            EventSystem.RaiseGameMessage(I18n.GetString("PlayerAttacks", enemy.Name));

            int baseDamage = Strength;
            int weaponDamage = equippedWeapon?.Damage ?? 0;
            int totalDamage = baseDamage + weaponDamage;
            enemy.TakeDamage(totalDamage);

            if (enemy.Health <= 0)
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

            EventSystem.RaiseGameMessage(I18n.GetString("PlayerTakesDamage", damage));

            int baseDefense = Defense;
            int armorDefense = equippedArmor?.Defense ?? 0;
            int totalDefense = baseDefense + armorDefense;

            int actualDamage = damage - totalDefense;
            if (actualDamage < 0)
            {
                actualDamage = 0;
            }

            Health -= actualDamage;

            if (Health <= 0)
            {
                Health = 0;
                EventSystem.RaiseGameMessage(I18n.GetString("PlayerDefeated"));
                Die();
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        public void AddExperience(int amount)
        {
            Experience += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int requiredExp = Level * 100;
            if (Experience >= requiredExp)
            {
                Level++;
                Experience -= requiredExp;
                MaxHealth += 20;
                Strength += 2;
                Defense += 1;
                Health = MaxHealth;
            }
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
            EventSystem.RaiseGameMessage(I18n.GetString("EquippedWeapon", weapon.Name));
        }

        public void EquipArmor(Armor armor)
        {
            if (equippedArmor != null)
            {
                inventoryManager.AddItem(equippedArmor);
            }
            equippedArmor = armor;
            EventSystem.RaiseGameMessage(I18n.GetString("EquippedArmor", armor.Name));
        }

        public void PickupItems()
        {
            List<Item> items = itemManager.GetItemsAt(X, Y);
            if (items.Count == 0)
            {
                EventSystem.RaiseGameMessage(I18n.GetString("NoItemsToPickUp"));
                return;
            }

            itemManager.RemoveItemsAt(X, Y);
            foreach (Item item in items)
            {
                AddItem(item);
            }
            EventSystem.RaiseSurroundingsChange((X, Y), FOVRadius, SurroundingsChangeType.PickUpLoot);
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
            Health = 0;
        }
    }
}
