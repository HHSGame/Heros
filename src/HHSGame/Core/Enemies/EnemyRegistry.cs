using HHSGame.UI;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.Core.Enemies
{
    public class EnemyRegistry
    {
        public record EnemyConfig(
            string Name,
            int MaxHealth,
            int Strength,
            int Defense,
            int ExperienceValue,
            char Glyph,
            Attribute Attribute
        );

        private static readonly Dictionary<EnemyType, EnemyConfig> enemyConfigs = new()
        {
            {
                EnemyType.Gangster,
                new EnemyConfig("Gangster", 30, 5, 2, 50, 'g', ColorPresets.Enemies.Gangster)
            },
            {
                EnemyType.Bandit,
                new EnemyConfig("Bandit", 60, 8, 5, 100, 'b', ColorPresets.Enemies.Bandit)
            },
            {
                EnemyType.BanditLeader,
                new EnemyConfig("Bandit Leader", 100, 12, 8, 200, 'L', ColorPresets.Enemies.BanditLeader)
            },
            {
                EnemyType.Thug,
                new EnemyConfig("Thug", 30, 5, 2, 30, 't', ColorPresets.Enemies.Thug)
            },
            {
                EnemyType.Soldier,
                new EnemyConfig("Soldier", 60, 10, 5, 60, 's', ColorPresets.Enemies.Soldier)
            },
            {
                EnemyType.Sniper,
                new EnemyConfig("Sniper", 50, 20, 3, 150, 's', ColorPresets.Enemies.Sniper)
            }
        };

        public static EnemyConfig GetConfig(EnemyType type)
        {
            return enemyConfigs.TryGetValue(type, out EnemyConfig? config)
                ? config
                : throw new ArgumentException($"No config found for enemy type {type}");
        }
    }
}
