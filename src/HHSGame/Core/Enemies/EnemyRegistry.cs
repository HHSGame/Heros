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
            }
        };

        public static EnemyConfig GetConfig(EnemyType type)
        {
            if (enemyConfigs.TryGetValue(type, out var config))
            {
                return config;
            }
            throw new ArgumentException($"No config found for enemy type {type}");
        }
    }
}
