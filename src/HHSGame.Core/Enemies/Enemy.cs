using HHSGame.Core.Combat;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Rendering;
using HHSGame.Core.Stats;
using HHSGame.Utils;

namespace HHSGame.Core.Enemies
{
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
        private readonly ItemCatalog itemCatalog;
        private readonly MapState mapState;

        private readonly Factions.FactionManager factionManager;        public string Id { get; }
        public EnemyState State { get; private set; }
        public int ExperienceValue { get; }
        public int SkipTurnsRemaining { get; private set; }
        public Factions.Faction Faction { get; }
        public CharacterStats Stats { get; }
        public ActionSequence ActionSequence { get; } = new();
        public int ArmorValue => equippedArmor?.ArmorValue ?? 0;
        public Weapon EquippedWeapon => equippedWeapon ?? itemCatalog.CreateWeapon(itemCatalog.UnknownWeaponId);
        public int EvasionBonus => Stats.EvasionBonus;
        public bool IsDead => Stats.CurrentHp <= 0;

        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; }
        public char Glyph { get; }
        public GameAttribute Attribute { get; }

        private Weapon? equippedWeapon;
        private Armor? equippedArmor;

        public Coordinate Position => new(X, Y);

        public Enemy(
            int x,
            int y,
            CollisionSystem collisionSystem,
            Pathfinder pathfinder,
            EnemyDefinition definition,
            ItemCatalog itemCatalog,
            MapState mapState,
            Factions.FactionManager factionManager,
            Random random)
        {
            this.factionManager = factionManager;
            this.random = random;
            Id = definition.Id;
            X = x;
            Y = y;
            this.collisionSystem = collisionSystem;
            this.pathfinder = pathfinder;
            this.itemCatalog = itemCatalog;
            this.mapState = mapState;
            State = EnemyState.Idle;

            Name = definition.Name;
            Faction = Factions.FactionManager.ParseFaction(definition.Faction);
            ExperienceValue = definition.ExperienceValue;
            Glyph = definition.Glyph;
            Attribute = definition.Attribute;
            equippedWeapon = itemCatalog.CreateWeapon(definition.WeaponId);
            equippedArmor = itemCatalog.CreateArmor(definition.ArmorId);

            Stats = new CharacterStats(definition.Attributes with { }, definition.Skills.Clone());

            abilitySystem = new EnemyAbilitySystem(this, definition.Abilities, random);
            lootSystem = new EnemyLootSystem(definition.Loot, itemCatalog, random);
        }

        public void ResetTurn(bool includeAp = true)
        {
            Stats.ResetTurn(includeAp);
        }

        public void EndTurn()
        {
            Stats.EndTurn();
        }

        public void PlanTurn(Player player, bool useAp)
        {
            if (IsDead)
            {
                return;
            }

            ActionSequence.Clear();

            if (SkipTurnsRemaining > 0)
            {
                SkipTurnsRemaining--;
                return;
            }

            if (!useAp)
            {
                QueueSingleAction(player);
                return;
            }

            int remainingAp = Stats.MaxAp;
            while (remainingAp > 0)
            {
                State = DetermineState(player);

                switch (State)
                {
                    case EnemyState.Attacking:
                        if (EquippedWeapon.ApCost > remainingAp)
                        {
                            return;
                        }
                        ActionSequence.Enqueue(new QueuedAction(
                            $"Attack {player.Name}",
                            EquippedWeapon.ApCost,
                            () => Attack(player)));
                        remainingAp -= EquippedWeapon.ApCost;
                        break;
                    case EnemyState.Chasing:
                        if (ActionCosts.Movement > remainingAp)
                        {
                            return;
                        }
                        ActionSequence.Enqueue(new QueuedAction(
                            "Move towards player",
                            ActionCosts.Movement,
                            () => MoveTowards(player)));
                        remainingAp -= ActionCosts.Movement;
                        break;
                    default:
                        return;
                }
            }
        }

        public void ExecutePlannedActions(bool useAp, Action<QueuedAction>? onExecuted = null)
        {
            ActionSequence.Execute(Stats, !useAp, onExecuted);
            ActionSequence.Clear();
        }

        private void QueueSingleAction(Player player)
        {
            State = DetermineState(player);

            switch (State)
            {
                case EnemyState.Attacking:
                    ActionSequence.Enqueue(new QueuedAction(
                        $"Attack {player.Name}",
                        EquippedWeapon.ApCost,
                        () => Attack(player)));
                    break;
                case EnemyState.Chasing:
                    ActionSequence.Enqueue(new QueuedAction(
                        "Move towards player",
                        ActionCosts.Movement,
                        () => MoveTowards(player)));
                    break;
            }
        }

        private static EnemyState DetermineState(Player player, int enemyX, int enemyY)
        {
            int dx = player.X - enemyX;
            int dy = player.Y - enemyY;
            int sqDistance = dx * dx + dy * dy;

            if (sqDistance <= GameConstants.EnemyAI.AttackRangeSquared)
            {
                return EnemyState.Attacking;
            }

            if (sqDistance <= GameConstants.EnemyAI.DetectionRangeSquared)
            {
                return EnemyState.Chasing;
            }

            return EnemyState.Idle;
        }

        private EnemyState DetermineState(Player player)
        {
            if (!CanDetectPlayer(player))
            {
                return EnemyState.Idle;
            }

            // Check faction — allied/neutral factions don't attack each other
            Factions.Faction playerFaction = factionManager.GetEffectivePlayerFaction();
            if (!factionManager.WillAttack(Faction, playerFaction))
            {
                return EnemyState.Idle;
            }

            if (CombatTargeting.CanAttack(mapState, Position, player.Position, EquippedWeapon))
            {
                return EnemyState.Attacking;
            }

            return DetermineState(player, X, Y);
        }

        public bool CanDetectPlayer(Player player)
        {
            if (!player.IsSneaking)
            {
                return true;
            }

            int awareness = Stats.Skills.GetValue(SkillType.Awareness, Stats.Attributes);
            int stealth = player.Stats.Skills.GetValue(SkillType.Stealth, player.Stats.Attributes);
            return awareness >= stealth;
        }

        public void SkipNextTurns(int turns)
        {
            if (turns <= 0)
            {
                return;
            }

            SkipTurnsRemaining = Math.Max(SkipTurnsRemaining, turns);
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
            if (!CombatTargeting.CanAttack(mapState, Position, player.Position, EquippedWeapon))
            {
                return;
            }

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
