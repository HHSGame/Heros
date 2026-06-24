using System.Text.Json;
using System.Text.Json.Serialization;
using HHSGame.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core
{
    public partial class GameDataLoader(ILogger<GameDataLoader> logger)
    {

        private JsonSerializerOptions Options { get; set; } = new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new FlexibleStringConverter(),
                new FlexibleEnumConverter<Items.ItemRarity>(),
                new FlexibleEnumConverter<Items.WeaponType>(),
                new FlexibleEnumConverter<Items.WeaponTrajectory>(),
                new FlexibleEnumConverter<Items.EquipmentSlot>(),
                new FlexibleEnumConverter<Stats.AttributeType>(),
                new FlexibleEnumConverter<Stats.SkillType>(),
                new FlexibleEnumConverter<Factions.Faction>(),
                new FlexibleEnumConverter<Quests.QuestStatus>(),
                new FlexibleEnumConverter<Quests.QuestObjectiveKind>(),
                new FlexibleEnumConverter<Quests.QuestRewardKind>(),
                new FlexibleEnumConverter<Dialogue.DialogueRequirementType>(),
                new FlexibleEnumConverter<Dialogue.DialogueEffectType>(),
                new FlexibleEnumConverter<Enemies.EnemyAbilityKind>(),
                new FlexibleEnumConverter<Map.MapStyle>(),
                new FlexibleEnumConverter<GameStateType>()
            }
        };

        public T? LoadData<T>(string path)
        {
            if (!Path.Exists(path))
            {
                throw new FileNotFoundException($"File cannot be found for type {typeof(T).Name}",
                    path);
            }

            using FileStream openStream = File.OpenRead(path);
            LogLoading(logger, typeof(T).Name, path);
            T? result = JsonSerializer.Deserialize<T>(openStream, Options);
            return result;
        }

        public byte[] LoadBytes(string path)
        {
            if (!Path.Exists(path))
            {
                throw new FileNotFoundException($"File cannot be found when reading bytes",
                    path);
            }

            byte[] result = File.ReadAllBytes(path);
            LogLoading(logger, "byte array", path);

            return result;
        }

        [LoggerMessage(LogLevel.Information, "Loading {type} from {path}")]
        static partial void LogLoading(ILogger logger, string type, string path);

    }

}
