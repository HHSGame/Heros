using HHSGame.Core.Classes;
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
                StartingItems = config.Player.StartingItems.ToList()
            };

            if (config.Player.StartPosition != null)
            {
                parameters.PlayerStartPosition = new Coordinate(
                    config.Player.StartPosition.X,
                    config.Player.StartPosition.Y);
            }

            return parameters;
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
