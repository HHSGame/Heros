using RpgGame.UI;
using Attribute = Terminal.Gui.Attribute;

namespace RpgGame.Core
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

        private static readonly Dictionary<EnemyType, EnemyConfig> _enemyConfigs = new()
        {
            { 
                EnemyType.Goblin, 
                new EnemyConfig("Goblin", 30, 5, 2, 50, 'g', ColorPresets.Enemies.Goblin)
            },
            { 
                EnemyType.Orc, 
                new EnemyConfig("Orc", 60, 8, 5, 100, 'o', ColorPresets.Enemies.Orc) 
            },
            { 
                EnemyType.Troll, 
                new EnemyConfig("Troll", 100, 12, 8, 200, 'T', ColorPresets.Enemies.Troll) 
            }
        };

        public static EnemyConfig GetConfig(EnemyType type)
        {
            if (_enemyConfigs.TryGetValue(type, out var config))
            {
                return config;
            }
            throw new ArgumentException($"No config found for enemy type {type}");
        }
    }
}
