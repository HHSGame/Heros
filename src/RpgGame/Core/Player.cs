
namespace RpgGame.Core
{
    public class Player: GameActor
    {
        public int PreviousX { get; private set; }
        public int PreviousY { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int Strength { get; private set; }
        public int Defense { get; private set; }
        public int Experience { get; private set; }
        public int Level { get; private set; }

        public override char Glyph => '@';

        private List<Item> _inventory;
        private List<ActiveEffect> _activeEffects;
        private Random _random;
        private GameWorld _world;
        public Player(int x, int y, GameWorld world)
        {
            X = x;
            Y = y;
            PreviousX = x;
            PreviousY = y;
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

        }

        public void Update()
        {
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
            if (_world.CurrentTurn != TurnState.PlayerTurn)
            {
                EventSystem.RaiseEvent("Not your turn!");
                return;
            }

            Update();

            int newX = X + dx;
            int newY = Y + dy;

            // Check world bounds and collision
            if (newX >= 0 && newX < GameWorld.MapWidth &&
                newY >= 0 && newY < GameWorld.MapHeight &&
                _world.IsWalkable(newX, newY))
            {
                var enemy = _world.GetEnemyAt(newX, newY);
                if (enemy != null)
                {
                    Attack(enemy);
                }
                else
                {
                    PreviousX = X;
                    PreviousY = Y;
                    X = newX;
                    Y = newY;
                    EventSystem.RaiseEvent($"Player moved to ({X}, {Y})");
                }
            }
            else
            {
                EventSystem.RaiseEvent($"Player tried to move to blocked position ({newX}, {newY})");
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

        public void ApplyEffect(ActiveEffect effect)
        {
            _activeEffects.Add(effect);
        }

        public bool IsStunned()
        {
            return _activeEffects.Any(e => e is StunEffect);
        }

        public bool IsPoisoned()
        {
            return _activeEffects.Any(e => e is PoisonEffect);
        }

        private void Die()
        {
            // Handle player death
        }
    }

    public abstract class ActiveEffect
    {
        public int Duration { get; set; }

        public ActiveEffect(int duration)
        {
            Duration = duration;
        }

        public abstract void ApplyEffect(Player player);
    }

    public class PoisonEffect : ActiveEffect
    {
        private int _damagePerTurn;

        public PoisonEffect(int duration, int damagePerTurn = 2) : base(duration)
        {
            _damagePerTurn = damagePerTurn;
        }

        public override void ApplyEffect(Player player)
        {
            player.TakeDamage(_damagePerTurn);
        }
    }

    public class StunEffect : ActiveEffect
    {
        public StunEffect(int duration) : base(duration)
        {
        }

        public override void ApplyEffect(Player player)
        {
            // Stun prevents actions but doesn't deal damage
        }
    }
}
