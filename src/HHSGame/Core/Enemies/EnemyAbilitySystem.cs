using HHSGame.Core.Engine.Catalogs;

namespace HHSGame.Core.Enemies
{
    public class EnemyAbilitySystem(Enemy enemy, IReadOnlyList<EnemyAbilityDefinition> abilities, Random random)
    {
        public void TryApplySpecialEffect(Player player)
        {
            foreach (EnemyAbilityDefinition ability in abilities)
            {
                if (ability.Chance <= 0)
                {
                    continue;
                }

                if (random.Next(100) < ability.Chance)
                {
                    ApplyAbility(player, ability);
                }
            }
        }

        private void ApplyAbility(Player player, EnemyAbilityDefinition ability)
        {
            string message = ability.Message;
            switch (ability.Kind)
            {
                case EnemyAbilityKind.Poison:
                    int poisonDuration = ability.Duration > 0 ? ability.Duration : 1;
                    int poisonDamage = ability.Amount > 0 ? ability.Amount : 2;
                    player.ApplyEffect(new PoisonEffect(poisonDuration, poisonDamage));
                    message = string.IsNullOrWhiteSpace(message) ? $"{enemy.Name} poisons the player!" : message;
                    break;
                case EnemyAbilityKind.Stun:
                    int stunDuration = ability.Duration > 0 ? ability.Duration : 1;
                    player.ApplyEffect(new StunEffect(stunDuration));
                    message = string.IsNullOrWhiteSpace(message) ? $"{enemy.Name} stuns the player!" : message;
                    break;
                case EnemyAbilityKind.HealSelf:
                    int healAmount = ability.Amount > 0 ? ability.Amount : 0;
                    if (healAmount > 0)
                    {
                        enemy.Heal(healAmount);
                    }
                    message = string.IsNullOrWhiteSpace(message) ? $"{enemy.Name} heals itself!" : message;
                    break;
                default:
                    message = string.IsNullOrWhiteSpace(message) ? $"{enemy.Name} uses a special ability!" : message;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                Events.RaiseGameMessage(message);
            }
        }
    }
}
