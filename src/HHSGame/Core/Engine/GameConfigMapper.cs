using System.IO;
using HHSGame.Core;
using HHSGame.Core.Classes;
using HHSGame.Core.Enemies;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Map;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Engine
{
    public static class GameConfigMapper
    {
        public static GameParameters ToParameters(GameConfig config)
        {
            MapStyle style = MapStyle.Cave;
            if (!string.IsNullOrWhiteSpace(config.Map.Style)
                && Enum.TryParse(config.Map.Style, true, out MapStyle parsedStyle))
            {
                style = parsedStyle;
            }

            GameParameters parameters = new()
            {
                MapStyle = style,
                MapWidth = config.Map.Width,
                MapHeight = config.Map.Height,
                UseCustomMap = config.Map.UseCustomMap,
                CustomMapPath = config.Map.CustomMapPath,
                PlayerClass = ResolveClass(config.Player.Class),
                PlayerAttributes = config.Player.Attributes,
                PlayerSkills = BuildSkills(config.Player.Skills),
                StartingItems = config.Player.StartingItems.ToList(),
                EnemySpawns = BuildEnemySpawns(config.Enemies),
                MapItems = BuildMapItems(config.Items)
            };

            if (config.Player.StartPosition != null)
            {
                parameters.PlayerStartPosition = new Coordinate(
                    config.Player.StartPosition.X,
                    config.Player.StartPosition.Y);
            }

            return parameters;
        }

        private static List<EnemySpawn> BuildEnemySpawns(List<EnemySpawnConfig> spawns)
        {
            List<EnemySpawn> result = [];
            foreach (EnemySpawnConfig spawn in spawns)
            {
                if (!Enum.TryParse(spawn.Id, true, out EnemyType enemyType))
                {
                    throw new InvalidDataException($"Unknown enemy id '{spawn.Id}'.");
                }

                if (spawn.Position == null)
                {
                    throw new InvalidDataException($"Enemy position is required for {spawn.Id}.");
                }

                result.Add(new EnemySpawn(
                    enemyType,
                    new Coordinate(spawn.Position.X, spawn.Position.Y),
                    spawn.Count));
            }

            return result;
        }

        private static List<MapItemSpawn> BuildMapItems(List<ItemConfig> items)
        {
            List<MapItemSpawn> result = [];
            foreach (ItemConfig item in items)
            {
                if (item.Position == null)
                {
                    throw new InvalidDataException($"Item position is required for {item.Id}.");
                }

                result.Add(new MapItemSpawn(
                    item.Id,
                    new Coordinate(item.Position.X, item.Position.Y),
                    item.Quantity));
            }

            return result;
        }

        private static Skills? BuildSkills(Dictionary<string, int> skillRanks)
        {
            if (skillRanks.Count == 0)
            {
                return null;
            }

            Skills skills = new();
            foreach (KeyValuePair<string, int> entry in skillRanks)
            {
                if (Enum.TryParse(entry.Key, true, out SkillType skillType))
                {
                    skills.SetRank(skillType, entry.Value);
                }
            }

            return skills;
        }

        private static ClassConfig ResolveClass(string? className)
        {
            ClassConfig[] classes =
            [
                Classes.Classes.Warrior,
                Classes.Classes.Thief,
                Classes.Classes.Alchemist,
                Classes.Classes.Unemployed
            ];

            if (!string.IsNullOrWhiteSpace(className))
            {
                foreach (ClassConfig config in classes)
                {
                    if (string.Equals(config.Name, className, StringComparison.OrdinalIgnoreCase))
                    {
                        return config;
                    }
                }
            }

            return Classes.Classes.Warrior;
        }
    }
}
