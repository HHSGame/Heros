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
