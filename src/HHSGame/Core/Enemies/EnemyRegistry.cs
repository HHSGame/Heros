using HHSGame.Core.Classes;
using HHSGame.Core.Items;
using HHSGame.Core.Stats;
using HHSGame.UI;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.Core.Enemies
{
    public class EnemyRegistry
    {
        public record EnemyConfig(
            string Name,
            Attributes Attributes,
            Skills Skills,
            Weapon Weapon,
            Armor Armor,
            int ExperienceValue,
            char Glyph,
            Attribute Attribute
        );

        private static readonly Dictionary<EnemyType, EnemyConfig> enemyConfigs = new()
        {
            {
                EnemyType.Gangster,
                new EnemyConfig(
                    "Gangster",
                    new Attributes { Strength = 4, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(),
                    Classes.Weapons.Dagger,
                    Classes.Armors.Cloak,
                    50,
                    'g',
                    ColorPresets.Enemies.Gangster)
            },
            {
                EnemyType.Bandit,
                new EnemyConfig(
                    "Bandit",
                    new Attributes { Strength = 6, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(),
                    Classes.Weapons.Axe,
                    Classes.Armors.LeatherArmor,
                    100,
                    'b',
                    ColorPresets.Enemies.Bandit)
            },
            {
                EnemyType.BanditLeader,
                new EnemyConfig(
                    "Bandit Leader",
                    new Attributes { Strength = 8, Perception = 6, Agility = 5, Charisma = 5, Intelligence = 5 },
                    new Skills(),
                    Classes.Weapons.Hammer,
                    Classes.Armors.Shield,
                    200,
                    'L',
                    ColorPresets.Enemies.BanditLeader)
            },
            {
                EnemyType.Thug,
                new EnemyConfig(
                    "Thug",
                    new Attributes { Strength = 4, Perception = 4, Agility = 4, Charisma = 4, Intelligence = 4 },
                    new Skills(),
                    Classes.Weapons.Dagger,
                    Classes.Armors.Cloak,
                    30,
                    't',
                    ColorPresets.Enemies.Thug)
            },
            {
                EnemyType.Soldier,
                new EnemyConfig(
                    "Soldier",
                    new Attributes { Strength = 6, Perception = 6, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(),
                    Classes.Weapons.Sword,
                    Classes.Armors.Shield,
                    60,
                    's',
                    ColorPresets.Enemies.Soldier)
            },
            {
                EnemyType.Sniper,
                new EnemyConfig(
                    "Sniper",
                    new Attributes { Strength = 4, Perception = 8, Agility = 6, Charisma = 4, Intelligence = 5 },
                    new Skills(),
                    Classes.Weapons.Bow,
                    Classes.Armors.Robe,
                    150,
                    's',
                    ColorPresets.Enemies.Sniper)
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
