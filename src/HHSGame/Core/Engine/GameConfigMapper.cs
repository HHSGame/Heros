using System.IO;
using HHSGame.Core;
using HHSGame.Core.Classes;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Map;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Engine
{
    public sealed class GameConfigMapper(GameCatalog catalogs)
    {
        private readonly ClassCatalog classCatalog = catalogs.ClassCatalog;
        private readonly EnemyCatalog enemyCatalog = catalogs.EnemyCatalog;
        private readonly Items.ItemCatalog itemCatalog = catalogs.ItemCatalog;

        public GameParameters ToParameters(GameConfig config)
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
                PlayerClass = classCatalog.Resolve(config.Player.Class),
                PlayerAttributes = config.Player.Attributes,
                PlayerSkills = config.Player.Skills.Count == 0 ? null : SkillMapper.BuildSkills(config.Player.Skills),
                StartingItems = ValidateStartingItems(config.Player.StartingItems),
                PlayerSpawns = BuildPlayerSpawns(config.Players),
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

        private List<EnemySpawn> BuildEnemySpawns(List<EnemySpawnConfig>? spawns)
        {
            List<EnemySpawn> result = [];
            if (spawns == null || spawns.Count == 0)
            {
                return result;
            }

            foreach (EnemySpawnConfig spawn in spawns)
            {
                string id = RequireId(spawn.Id, "enemy");
                if (!enemyCatalog.TryGetDefinition(id, out _))
                {
                    throw new InvalidDataException($"Unknown enemy id '{id}'.");
                }

                if (spawn.Position == null)
                {
                    throw new InvalidDataException($"Enemy position is required for {id}.");
                }

                result.Add(new EnemySpawn(
                    id,
                    new Coordinate(spawn.Position.X, spawn.Position.Y),
                    spawn.Count));
            }

            return result;
        }

        private List<MapItemSpawn> BuildMapItems(List<ItemConfig>? items)
        {
            List<MapItemSpawn> result = [];
            if (items == null || items.Count == 0)
            {
                return result;
            }

            foreach (ItemConfig item in items)
            {
                string id = RequireId(item.Id, "item");
                if (item.Position == null)
                {
                    throw new InvalidDataException($"Item position is required for {id}.");
                }

                if (!itemCatalog.TryCreateItem(id, out _))
                {
                    throw new InvalidDataException($"Unknown item id '{id}'.");
                }

                result.Add(new MapItemSpawn(
                    id,
                    new Coordinate(item.Position.X, item.Position.Y),
                    item.Quantity));
            }

            return result;
        }

        private List<PlayerSpawn> BuildPlayerSpawns(List<PlayerEntryConfig>? players)
        {
            List<PlayerSpawn> result = [];
            if (players == null || players.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < players.Count; i++)
            {
                PlayerEntryConfig entry = players[i];
                if (entry.StartPosition == null)
                {
                    throw new InvalidDataException("Player start position is required.");
                }

                string name = string.IsNullOrWhiteSpace(entry.Name)
                    ? $"Player {i + 1}"
                    : entry.Name.Trim();

                char glyph = string.IsNullOrWhiteSpace(entry.Glyph) ? '\0' : entry.Glyph.Trim()[0];

                ClassConfig classConfig = ResolvePlayerClass(entry.Class, name);

                List<string> startingItems = ValidateStartingItems(entry.StartingItems);
                Skills? skills = entry.Skills.Count == 0 ? null : SkillMapper.BuildSkills(entry.Skills);

                result.Add(new PlayerSpawn(
                    name,
                    glyph,
                    classConfig,
                    entry.Attributes,
                    skills,
                    new Coordinate(entry.StartPosition.X, entry.StartPosition.Y),
                    startingItems));
            }

            return result;
        }

        private List<string> ValidateStartingItems(List<string>? items)
        {
            List<string> result = [];
            if (items == null || items.Count == 0)
            {
                return result;
            }

            foreach (string itemId in items)
            {
                string id = RequireId(itemId, "starting item");
                if (!itemCatalog.TryCreateItem(id, out _))
                {
                    throw new InvalidDataException($"Unknown starting item id '{id}'.");
                }

                result.Add(id);
            }

            return result;
        }

        private ClassConfig ResolvePlayerClass(string? classId, string playerName)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return classCatalog.GetDefault();
            }

            if (!classCatalog.TryResolve(classId, out ClassConfig config))
            {
                throw new InvalidDataException($"Unknown class id '{classId}' for {playerName}.");
            }

            return config;
        }

        private static string RequireId(string value, string context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException($"Missing {context} id.");
            }

            return value.Trim();
        }

    }
}
