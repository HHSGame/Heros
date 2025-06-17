using HHSGame.Core.Map;

namespace HHSGame.Core.Enemies
{
    using HHSGame.Utils;
    using Items;

    public enum EnemyType
    {
        Gangster,
        Bandit,
        BanditLeader,
        Thug,
        Soldier,
        Sniper,
    }

    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Fleeing
    }

    public class Enemy : IGameActor
    {
        private readonly CollisionSystem collisionSystem;
        private readonly EnemyAbilitySystem abilitySystem;
        private readonly EnemyLootSystem lootSystem;
        private readonly Pathfinder pathfinder;
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
        public Terminal.Gui.Drawing.Attribute Attribute { get; }
        private int attackCooldown;
        private int specialAbilityCooldown;
        private readonly Random random;

        public Coordinate Position => new (X, Y);

        public Enemy(EnemyType type, int x, int y, CollisionSystem collisionSystem, Pathfinder pathfinder)
        {
            random = new Random();
            Type = type;
            X = x;
            Y = y;
            this.collisionSystem = collisionSystem;
            this.pathfinder = pathfinder;
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
            attackCooldown = 0;
            specialAbilityCooldown = 0;

            abilitySystem = new EnemyAbilitySystem(this, random);
            lootSystem = new EnemyLootSystem(type, random);
        }

        public void Update(Player player, bool isEnemyTurn)
        {
            if (!isEnemyTurn) return;

            if (attackCooldown > 0) attackCooldown--;
            if (specialAbilityCooldown > 0) specialAbilityCooldown--;

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
                    if (attackCooldown <= 0)
                    {
                        Attack(player);
                        attackCooldown = 2; // 2 turn cooldown
                    }
                    break;

                case EnemyState.Chasing:
                    // Get path to player
                    var path = pathfinder.FindPath(Position, player.Position);

                    if (path.Count > 1) // First element is current position
                    {
                        var nextStep = path[1];
                        int moveX = nextStep.X - X;
                        int moveY = nextStep.Y - Y;
                        Move(moveX, moveY);
                    }
                    break;
            }
        }

        public void Attack(Player player)
        {
            int damage = Strength;
            EventSystem.RaiseGameMessage(I18n.GetString("HHS.Core.Enemies.Enemy.Attack", Name, damage));

            abilitySystem.TryApplySpecialEffect(player);
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
                EventSystem.RaiseGameMessage(I18n.GetString("HHS.Core.Enemies.Enemy.Defeated", Name));
                Die();
            }
            else
            {
                EventSystem.RaiseGameMessage(I18n.GetString("HHS.Core.Enemies.Enemy.TakeDamage", Name, actualDamage, Health));
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

            if (collisionSystem.CanMoveTo(newX, newY, this))
            {
                X = newX;
                Y = newY;
                EventSystem.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
            else
            {
                EventSystem.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
        }

        private void Die()
        {
            // Drop loot
            var loot = GenerateLoot();
            // TODO: Add loot to game world
        }

        public List<Item> GenerateLoot() => lootSystem.GenerateLoot();

        public override string ToString()
        {
            return $"[0]{Name} - {Health}/{MaxHealth}";
        }
    }
}
