using HHSGame.Core.Combat;
using Terminal.Gui;

namespace HHSGame.Core
{
    public class Player: GameActor
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Strength { get; private set; }
        public int Defense { get; private set; }
        public int Experience { get; private set; }
        public int Level { get; private set; }

        public char Glyph => '☭';
        public string Name => "Hero";

        public Terminal.Gui.Attribute Attribute => Terminal.Gui.Attribute.Make(Color.BrightYellow, Color.Red);

        private List<ActiveEffect> _activeEffects;
        private readonly CollisionSystem _collisionSystem;
        private readonly InventoryManager _inventoryManager;
        private readonly ItemManager _itemManager;
        private readonly MapState _mapState;
        private Weapon? _equippedWeapon;
        private Armor? _equippedArmor;
        private TurnManager _turnManager;
        
        private const int FOVRadius = 7;
        private HashSet<(int x, int y)> _visibleTiles = new();

        public Player(int x, int y, GameContext context)
        {
            X = x;
            Y = y;
            MaxHealth = 100;
            Health = MaxHealth;
            Strength = 10;
            Defense = 5;
            Experience = 0;
            Level = 1;
            _activeEffects = new List<ActiveEffect>();
            _collisionSystem = context.CollisionSystem;
            _inventoryManager = context.InventoryManager;
            _itemManager = context.ItemManager;
            _mapState = context.MapState;
            _turnManager = context.TurnManager;
            UpdateFOV();
        }

        public void UpdateFOV()
        {
            _visibleTiles.Clear();
            CalculateFOV(X, Y, FOVRadius);
            _mapState.MarkVisibleTiles(_visibleTiles);
            EventSystem.RaiseSurroundingsChange((X, Y), FOVRadius, SurroundingsChangeType.Movement);
        }

        private void CalculateFOV(int x, int y, int radius)
        {
            _visibleTiles.Clear();
            
            // Check all tiles within radius
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Skip if outside radius
                    if (dx * dx + dy * dy > radius * radius) continue;
                    
                    int targetX = x + dx;
                    int targetY = y + dy;
                    
                    if (!_mapState.IsInBounds(targetX, targetY)) continue;
                    
                    // Check line of sight
                    if (HasLineOfSight(x, y, targetX, targetY))
                    {
                        _visibleTiles.Add((targetX, targetY));
                    }
                }
            }
            
            // Always see current position
            _visibleTiles.Add((x, y));
        }

        private bool HasLineOfSight(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Add current tile to visible tiles
                _visibleTiles.Add((x0, y0));
                
                // If we hit an opaque tile, stop here but include it
                if (!_mapState.IsTransparent(x0, y0))
                    return true;
                    
                // If we reached the target, we have line of sight
                if (x0 == x1 && y0 == y1)
                    return true;
                    
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
            
            foreach (var effect in _activeEffects.ToArray())
            {
                effect.Duration--;
                if (effect.Duration <= 0)
                {
                    _activeEffects.Remove(effect);
                }
                else
                {
                    effect.ApplyEffect(this);
                }
            }
        }

        public void Move(int dx, int dy)
        {
            if (!_turnManager.IsPlayerTurn())
            {
                return;
            }

            Update();

            int newX = X + dx;
            int newY = Y + dy;

            if (_collisionSystem.CanMoveTo(newX, newY, this))
            {
                var collision = _collisionSystem.GetCollisionAt(newX, newY);
                if (collision is Enemy enemy)
                {
                    Attack(enemy);
                }
                else
                {
                    X = newX;
                    Y = newY;
                    UpdateFOV();
                    EventSystem.RaiseEvent(_collisionSystem.GetCollisionMessage(newX, newY, this));
                }
            }
            else
            {
                EventSystem.RaiseEvent(_collisionSystem.GetCollisionMessage(newX, newY, this));
            }
            _turnManager.EndPlayerTurn();
        }

        public void Attack(Enemy enemy)
        {
            if (IsStunned()) return;

            EventSystem.RaiseEvent($"Player attacks {enemy.Name}!");

            int baseDamage = Strength;
            int weaponDamage = _equippedWeapon?.Damage ?? 0;
            int totalDamage = baseDamage + weaponDamage;
            enemy.TakeDamage(totalDamage);

            if (enemy.Health <= 0)
            {
                AddExperience(enemy.ExperienceValue);
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsStunned()) return;
            EventSystem.RaiseEvent($"Player takes {damage} damage!");

            int baseDefense = Defense;
            int armorDefense = _equippedArmor?.Defense ?? 0;
            int totalDefense = baseDefense + armorDefense;
            
            int actualDamage = damage - totalDefense;
            if (actualDamage < 0) actualDamage = 0;
            Health -= actualDamage;

            if (Health <= 0)
            {
                Health = 0;
                EventSystem.RaiseEvent("Player has been defeated!");
                Die();
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
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
            _inventoryManager.AddItem(item);
        }

        public void EquipWeapon(Weapon weapon)
        {
            if (_equippedWeapon != null)
            {
                _inventoryManager.AddItem(_equippedWeapon);
            }
            _equippedWeapon = weapon;
            EventSystem.RaiseEvent($"Equipped {weapon.Name}");
        }

        public void EquipArmor(Armor armor)
        {
            if (_equippedArmor != null)
            {
                _inventoryManager.AddItem(_equippedArmor);
            }
            _equippedArmor = armor;
            EventSystem.RaiseEvent($"Equipped {armor.Name}");
        }

        public void PickupItems()
        {
            var items = _itemManager.GetItemsAt(X, Y);
            if (items.Count == 0)
            {
                EventSystem.RaiseEvent("No items here to pick up.");
                return;
            }

            _itemManager.RemoveItemsAt(X, Y);
            foreach (var item in items)
            {
                AddItem(item);
                EventSystem.RaiseEvent($"Picked up {item.Name}");
            }
            EventSystem.RaiseSurroundingsChange((X, Y), FOVRadius, SurroundingsChangeType.PickUpLoot);
        }

        public void ApplyEffect(ActiveEffect effect)
        {
            _activeEffects.Add(effect);
        }

        public bool IsStunned()
        {
            return _activeEffects.Any(e => e is StunEffect);
        }

        private void Die()
        {
            // Handle player death
        }
    }
}
