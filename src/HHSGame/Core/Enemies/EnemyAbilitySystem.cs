namespace HHSGame.Core.Enemies
{
    public class EnemyAbilitySystem(Enemy enemy, Random random)
    {
        private static readonly Dictionary<EnemyType, (int Chance, Action<Player, Enemy> Effect)> attackEffects = new()
        {
            {
                EnemyType.Gangster,
                (20, (Player player, Enemy self) => player.ApplyEffect(new PoisonEffect(3)))
            },
            {
                EnemyType.Bandit,
                (10, (Player player, Enemy self) => player.ApplyEffect(new StunEffect(1)))
            },
            {
                EnemyType.BanditLeader,
                (30, (Player player, Enemy self) => self.Heal(10))
            }
        };

        public void TryApplySpecialEffect(Player player)
        {
            if (attackEffects.TryGetValue(enemy.Type, out var effectData) &&
                random.Next(100) < effectData.Chance)
            {
                effectData.Effect(player, enemy);
                string abilityMessage = enemy.Type switch
                {
                    EnemyType.Gangster => $"{enemy.Name} poisons the player!",
                    EnemyType.Bandit => $"{enemy.Name} stuns the player!",
                    EnemyType.BanditLeader => $"{enemy.Name} heals itself!",
                    _ => $"{enemy.Name} uses a special ability!"
                };
                EventSystem.RaiseGameMessage(abilityMessage);
            }
        }
    }
}
