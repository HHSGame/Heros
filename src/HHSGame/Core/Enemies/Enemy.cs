using HHSGame.Core.Classes;
using HHSGame.Core.Combat;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Stats;
using HHSGame.Utils;

namespace HHSGame.Core.Enemies
{
    public enum EnemyType
    {
        Gangster,
        Bandit,
        BanditLeader,
        Thug,
        Soldier,
        Sniper
    }

    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Fleeing
    }

    public class Enemy : IGameActor, ICombatant
    {
        private readonly CollisionSystem collisionSystem;
        private readonly EnemyAbilitySystem abilitySystem;
        private readonly EnemyLootSystem lootSystem;
        private readonly Pathfinder pathfinder;
        private readonly Random random;

        public EnemyType Type { get; }
        public EnemyState State { get; private set; }
        public int ExperienceValue { get; }

        public CharacterStats Stats { get; }
        public int ArmorValue => equippedArmor?.ArmorValue ?? 0;
        public Weapon EquippedWeapon => equippedWeapon ?? Classes.Weapons.UnknownWeapon;
        public int EvasionBonus => Stats.EvasionBonus;
        public bool IsDead => Stats.CurrentHp <= 0;

        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; }
        public char Glyph { get; }
        public Terminal.Gui.Drawing.Attribute Attribute { get; }

        private Weapon? equippedWeapon;
        private Armor? equippedArmor;

        public Coordinate Position => new(X, Y);

        public Enemy(
            EnemyType type,
            int x,
            int y,
            CollisionSystem collisionSystem,
            Pathfinder pathfinder,
            EnemyRegistry.EnemyConfig config)
        {
            random = new Random();
            Type = type;
            X = x;
            Y = y;
            this.collisionSystem = collisionSystem;
            this.pathfinder = pathfinder;
            State = EnemyState.Chasing;

            Name = config.Name;
            ExperienceValue = config.ExperienceValue;
            Glyph = config.Glyph;
            Attribute = config.Attribute;
            equippedWeapon = config.Weapon;
            equippedArmor = config.Armor;

            Stats = new CharacterStats(config.Attributes with { }, config.Skills.Clone());

            abilitySystem = new EnemyAbilitySystem(this, random);
            lootSystem = new EnemyLootSystem(type, random);
        }

        public void ResetTurn(bool includeAp = true)
        {
            Stats.ResetTurn(includeAp);
        }

        public void EndTurn()
        {
            Stats.EndTurn();
        }

        public void TakeTurn(Player player, bool useAp)
        {
            if (IsDead)
            {
                return;
            }

            if (!useAp)
            {
                ExecuteSingleAction(player);
                return;
            }

            while (Stats.CurrentAp > 0)
            {
                State = DetermineState(player);

                switch (State)
                {
                    case EnemyState.Attacking:
                        if (!Stats.TrySpendAp(EquippedWeapon.ApCost))
                        {
                            return;
                        }
                        Attack(player);
                        break;
                    case EnemyState.Chasing:
                        if (!Stats.TrySpendAp(ActionCosts.Movement))
                        {
                            return;
                        }
                        MoveTowards(player);
                        break;
                    default:
                        return;
                }
            }
        }

        private void ExecuteSingleAction(Player player)
        {
            State = DetermineState(player);

            switch (State)
            {
                case EnemyState.Attacking:
                    Attack(player);
                    break;
                case EnemyState.Chasing:
                    MoveTowards(player);
                    break;
            }
        }

        private static EnemyState DetermineState(Player player, int enemyX, int enemyY)
        {
            int dx = player.X - enemyX;
            int dy = player.Y - enemyY;
            int sqDistance = dx * dx + dy * dy;

            if (sqDistance <= 1)
            {
                return EnemyState.Attacking;
            }

            if (sqDistance <= 64)
            {
                return EnemyState.Chasing;
            }

            return EnemyState.Idle;
        }

        private EnemyState DetermineState(Player player)
        {
            return DetermineState(player, X, Y);
        }

        private void MoveTowards(Player player)
        {
            List<Coordinate> path = pathfinder.FindPath(Position, player.Position);
            if (path.Count > 1)
            {
                Coordinate nextStep = path[1];
                int moveX = nextStep.X - X;
                int moveY = nextStep.Y - Y;
                Move(moveX, moveY);
            }
        }

        public void Attack(Player player)
        {
            CombatResolver.ResolveAttack(this, player, EquippedWeapon, random);
            abilitySystem.TryApplySpecialEffect(player);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            Stats.ApplyDamage(damage);

            if (IsDead)
            {
                Events.RaiseGameMessage(I18n.T("HHS.Core.Enemies.Enemy.Defeated", Name));
                Die();
            }
            else
            {
                Events.RaiseGameMessage(I18n.T("HHS.Core.Enemies.Enemy.TakeDamage", Name, damage, Stats.CurrentHp));
            }
        }

        public void Heal(int amount)
        {
            Stats.Heal(amount);
        }

        public void Move(int dx, int dy)
        {
            int newX = X + dx;
            int newY = Y + dy;

            if (collisionSystem.CanMoveTo(newX, newY, this))
            {
                X = newX;
                Y = newY;
                Events.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
            else
            {
                Events.RaiseGameMessage(collisionSystem.GetCollisionMessage(newX, newY, this));
            }
        }

        private void Die()
        {
            _ = GenerateLoot();
        }

        public List<Item> GenerateLoot()
        {
            return lootSystem.GenerateLoot();
        }

        public override string ToString()
        {
            return $"[0]{Name} - {Stats.CurrentHp}/{Stats.MaxHp}";
        }
    }
}
