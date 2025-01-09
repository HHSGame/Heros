using Terminal.Gui;

namespace RpgGame.Core
{
    public class Player: GameActor
    {
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Strength { get; private set; }
        public int Defense { get; private set; }
        public int Experience { get; private set; }
        public int Level { get; private set; }

        public override char Glyph => '@';
        public override string Name => "Player";

        public override Terminal.Gui.Attribute Attribute => Terminal.Gui.Attribute.Make(Color.BrightCyan, Color.Magenta);

        private List<Item> _inventory;
        private List<ActiveEffect> _activeEffects;
        private Random _random;
        private GameWorld _world;
        private readonly CollisionSystem _collisionSystem;
        
        private const int FOVRadius = 5;
        private HashSet<(int x, int y)> _visibleTiles = new();

        public Player(int x, int y, GameWorld world, CollisionSystem collisionSystem)
        {
            X = x;
            Y = y;
            MaxHealth = 100;
            Health = MaxHealth;
            Strength = 10;
            Defense = 5;
            Experience = 0;
            Level = 1;
            _inventory = new List<Item>();
            _activeEffects = new List<ActiveEffect>();
            _random = new Random();
            _world = world;
            _collisionSystem = collisionSystem;
            UpdateFOV();
        }

        public void UpdateFOV()
        {
            _visibleTiles.Clear();
            CalculateFOV(X, Y, FOVRadius);
            _world.MarkVisibleTiles(_visibleTiles);
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
                    
                    if (!_world.IsInBounds(targetX, targetY)) continue;
                    
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
                if (!_world.IsTransparent(x0, y0))
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
            // Update FOV every turn
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

        public bool IsInFOV(int x, int y)
        {
            return _visibleTiles.Contains((x, y));
        }

        public void Move(int dx, int dy)
        {
            if (_world.CurrentTurn != TurnState.PlayerTurn)
            {
                EventSystem.RaiseEvent("Not your turn!");
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
            EndPlayerTurn();
        }

        private void EndPlayerTurn()
        {
            if (_world.CurrentTurn == TurnState.PlayerTurn)
            {
                EventSystem.RaiseTurnChanged(TurnState.EnemyTurn);
            }
        }

        public void Attack(Enemy enemy)
        {
            if (IsStunned()) return;

            EventSystem.RaiseEvent($"Player attacks {enemy.Name}!");

            int damage = Strength;
            enemy.TakeDamage(damage);

            if (enemy.Health <= 0)
            {
                AddExperience(enemy.ExperienceValue);
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsStunned()) return;
            EventSystem.RaiseEvent($"Player takes {damage} damage!");

            int actualDamage = damage - Defense;
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
            _inventory.Add(item);
        }

        public void UseItem(int index)
        {
            if (index >= 0 && index < _inventory.Count)
            {
                _inventory[index].Use(this);
                _inventory.RemoveAt(index);
            }
        }

        public List<Item> GetInventory()
        {
            return _inventory;
        }

        public void PickupItems()
        {
            var items = _world.GetItemsAt(X, Y);
            if (items.Count == 0)
            {
                EventSystem.RaiseEvent("No items here to pick up.");
                return;
            }

            foreach (var item in items)
            {
                AddItem(item);
                EventSystem.RaiseEvent($"Picked up {item.Name}");
            }
            _world.RemoveItemsAt(X, Y);
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
