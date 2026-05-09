using HHSGame.Core.Stats;

namespace HHSGame.Core.Engine.Config
{
    public sealed class GameConfig
    {
        public GameMetaConfig Game { get; init; } = new();
        public MapConfig Map { get; init; } = new();
        public PlayerConfig Player { get; init; } = new();
        public List<PlayerEntryConfig> Players { get; init; } = [];
        public CatalogPathsConfig Catalogs { get; init; } = new();
        public List<EnemySpawnConfig> Enemies { get; init; } = [];
        public List<ItemConfig> Items { get; init; } = [];
        public List<NpcConfig> Npcs { get; init; } = [];
        public List<DialogueConfig> Dialogues { get; init; } = [];
        public List<QuestConfig> Quests { get; init; } = [];
        public List<AchievementConfig> Achievements { get; init; } = [];
        public ConditionConfig Conditions { get; init; } = new();
    }

    public sealed class GameMetaConfig
    {
        public string Name { get; init; } = "HHSGame";
        public int Version { get; init; } = 1;
    }

    public sealed class MapConfig
    {
        public string Style { get; init; } = "Cave";
        public int Width { get; init; } = 50;
        public int Height { get; init; } = 30;
        public bool UseCustomMap { get; init; }
        public string? CustomMapPath { get; init; }
    }

    public sealed class PlayerConfig
    {
        public string? Class { get; init; }
        public Attributes? Attributes { get; init; }
        public Dictionary<string, int> Skills { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public CoordinateConfig? StartPosition { get; init; }
        public List<string> StartingItems { get; init; } = [];
    }

    public sealed class PlayerEntryConfig
    {
        public string Name { get; init; } = string.Empty;
        public string Glyph { get; init; } = string.Empty;
        public string? Class { get; init; }
        public Attributes? Attributes { get; init; }
        public Dictionary<string, int> Skills { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public CoordinateConfig? StartPosition { get; init; }
        public List<string> StartingItems { get; init; } = [];
    }

    public sealed class CoordinateConfig
    {
        public int X { get; init; }
        public int Y { get; init; }
    }

    public sealed class EnemySpawnConfig
    {
        public string Id { get; init; } = string.Empty;
        public CoordinateConfig? Position { get; init; }
        public int Count { get; init; } = 1;
    }

    public sealed class ItemConfig
    {
        public string Id { get; init; } = string.Empty;
        public CoordinateConfig? Position { get; init; }
        public int Quantity { get; init; } = 1;
    }

    public sealed class NpcConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Glyph { get; init; } = string.Empty;
        public string ColorKey { get; init; } = string.Empty;
        public Attributes? Attributes { get; init; }
        public Dictionary<string, int> Skills { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public CoordinateConfig? Position { get; init; }
        public string DialogueId { get; init; } = string.Empty;
        public List<string> StartingItems { get; init; } = [];
    }

    public sealed class DialogueConfig
    {
        public string Id { get; init; } = string.Empty;
        public string StartNodeId { get; init; } = string.Empty;
        public List<DialogueNodeConfig> Nodes { get; init; } = [];
    }

    public sealed class DialogueNodeConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public List<DialogueOptionConfig> Options { get; init; } = [];
    }

    public sealed class DialogueOptionConfig
    {
        public string Text { get; init; } = string.Empty;
        public string? NextNodeId { get; init; }
        public List<DialogueRequirementConfig> Requirements { get; init; } = [];
        public List<DialogueEffectConfig> Effects { get; init; } = [];
    }

    public sealed class DialogueRequirementConfig
    {
        public string Type { get; init; } = string.Empty;
        public string Id { get; init; } = string.Empty;
        public int Min { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    public sealed class DialogueEffectConfig
    {
        public string Type { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
        public int Amount { get; init; }
    }

    public sealed class QuestConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Hint { get; init; } = string.Empty;
        public List<QuestObjectiveConfig> Objectives { get; init; } = [];
        public List<QuestRewardConfig> Rewards { get; init; } = [];
    }

    public sealed class QuestObjectiveConfig
    {
        public string Type { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
        public int Count { get; init; } = 1;
    }

    public sealed class QuestRewardConfig
    {
        public string Type { get; init; } = string.Empty;
        public string Target { get; init; } = string.Empty;
        public int Amount { get; init; }
    }

    public sealed class AchievementConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public List<QuestObjectiveConfig> Objectives { get; init; } = [];
    }

    public sealed class ConditionConfig
    {
        public List<string> Win { get; init; } = [];
        public List<string> Lose { get; init; } = new() { "PlayerDeath" };
        public List<ConditionEntryConfig> WinEntries { get; init; } = [];
        public List<ConditionEntryConfig> LoseEntries { get; init; } = [];
        public string WinMessage { get; init; } = string.Empty;
        public string LoseMessage { get; init; } = string.Empty;
    }

    public sealed class ConditionEntryConfig
    {
        public string Id { get; init; } = string.Empty;
        public int Value { get; init; }
        public string Param { get; init; } = string.Empty;
    }
}
