namespace RpgGame.Core
{
    public class EnemyAbilitySystem
    {
        private readonly Random _random;
        private readonly Enemy _enemy;

        private static readonly Dictionary<EnemyType, (int Chance, Action<Player, Enemy> Effect)> _attackEffects = new()
        {
            { 
                EnemyType.Goblin, 
                (20, (Player player, Enemy self) => player.ApplyEffect(new PoisonEffect(3))) 
            },
            { 
                EnemyType.Orc, 
                (10, (Player player, Enemy self) => player.ApplyEffect(new StunEffect(1))) 
            },
            { 
                EnemyType.Troll, 
                (30, (Player player, Enemy self) => self.Heal(10)) 
            }
        };

        public EnemyAbilitySystem(Enemy enemy, Random random)
        {
            _enemy = enemy;
            _random = random;
        }

        public void TryApplySpecialEffect(Player player)
        {
            if (_attackEffects.TryGetValue(_enemy.Type, out var effectData) && 
                _random.Next(100) < effectData.Chance)
            {
                effectData.Effect(player, _enemy);
                string abilityMessage = _enemy.Type switch
                {
                    EnemyType.Goblin => $"{_enemy.Name} poisons the player!",
                    EnemyType.Orc => $"{_enemy.Name} stuns the player!",
                    EnemyType.Troll => $"{_enemy.Name} heals itself!",
                    _ => $"{_enemy.Name} uses a special ability!"
                };
                EventSystem.RaiseEvent(abilityMessage);
            }
        }
    }
}
