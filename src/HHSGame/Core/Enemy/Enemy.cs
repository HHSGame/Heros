

namespace HHSGame.Core
{
    public enum EnemyType
    {
        Gangster,
        Bandit,
        BanditLeader
    }

    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Fleeing
    }

    public class Enemy : GameActor
    {
        private readonly CollisionSystem _collisionSystem;
        private readonly EnemyAbilitySystem _abilitySystem;
        private readonly EnemyLootSystem _lootSystem;
        private readonly Pathfinder _pathfinder;
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Strength { get; private set; }
        public int Defense { get; private set; }
        public int ExperienceValue { get; private set; }
        public string Name { get; }
        public EnemyType Type { get; private set; }
        public EnemyState State { get; private set; }
        public char Glyph { get; }
        public Terminal.Gui.Attribute Attribute { get; } 
        private int _attackCooldown;
        private int _specialAbilityCooldown;
        private readonly Random _random;

        public Enemy(EnemyType type, int x, int y, CollisionSystem collisionSystem, Pathfinder pathfinder)
        {
            _random = new Random();
            Type = type;
            X = x;
            Y = y;
            _collisionSystem = collisionSystem;
            _pathfinder = pathfinder;
            State = EnemyState.Chasing;

            var config = EnemyRegistry.GetConfig(type);
            Name = config.Name;
            MaxHealth = config.MaxHealth;
            Strength = config.Strength;
            Defense = config.Defense;
            ExperienceValue = config.ExperienceValue;
            Glyph = config.Glyph;
            Attribute = config.Attribute;
            
            Health = MaxHealth;
            _attackCooldown = 0;
            _specialAbilityCooldown = 0;

            _abilitySystem = new EnemyAbilitySystem(this, _random);
            _lootSystem = new EnemyLootSystem(type, _random);
        }

        public void Update(Player player, bool isEnemyTurn)
        {
            if (!isEnemyTurn) return;

            if (_attackCooldown > 0) _attackCooldown--;
            if (_specialAbilityCooldown > 0) _specialAbilityCooldown--;
            
            // Calculate squared distance to player (avoid expensive sqrt)
            int dx = player.X - X;
            int dy = player.Y - Y;
            int sqDistance = dx * dx + dy * dy;

            // Update state based on squared distance
            if (sqDistance <= 1) // 1^2 = 1
            {
                State = EnemyState.Attacking;
            }
            else if (sqDistance <= 64) // 8^2 = 64
            {
                State = EnemyState.Chasing;
            }
            else
            {
                State = EnemyState.Idle;
            }

            // Handle state-specific behavior
            switch (State)
            {
                case EnemyState.Attacking:
                    if (_attackCooldown <= 0)
                    {
                        Attack(player);
                        _attackCooldown = 2; // 2 turn cooldown
                    }
                    break;
                    
                case EnemyState.Chasing:
                    // Get path to player
                    var path = _pathfinder.FindPath((X, Y), (player.X, player.Y));
                    
                    if (path.Count > 1) // First element is current position
                    {
                        var nextStep = path[1];
                        int moveX = nextStep.x - X;
                        int moveY = nextStep.y - Y;
                        Move(moveX, moveY);
                    }
                    break;
            }
        }

        public void Attack(Player player)
        {
            int damage = Strength;
            EventSystem.RaiseEvent($"{Name} attacks the player for {damage} damage!");
            
            _abilitySystem.TryApplySpecialEffect(player);
            player.TakeDamage(damage);
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = damage - Defense;
            if (actualDamage < 0) actualDamage = 0;
            Health -= actualDamage;
            
            if (Health <= 0)
            {
                Health = 0;
                EventSystem.RaiseEvent($"{Name} was defeated!");
                Die();
            }
            else
            {
                EventSystem.RaiseEvent($"{Name} took {actualDamage} damage! HP remains {Health}.");
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public void Move(int dx, int dy)
        {
            int newX = X + dx;
            int newY = Y + dy;

            if (_collisionSystem.CanMoveTo(newX, newY, this))
            {
                X = newX;
                Y = newY;
                EventSystem.RaiseEvent(_collisionSystem.GetCollisionMessage(newX, newY, this));
            }
            else
            {
                EventSystem.RaiseEvent(_collisionSystem.GetCollisionMessage(newX, newY, this));
            }
        }

        private void Die()
        {
            // Drop loot
            var loot = GenerateLoot();
            // TODO: Add loot to game world
        }

        public List<Item> GenerateLoot() => _lootSystem.GenerateLoot();
    }
}
