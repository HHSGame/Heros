using HHSGame.Core.Stats;

namespace HHSGame.Core.Engine.Config
{
    public sealed class GameConfig
    {
        public GameMetaConfig Game { get; init; } = new();
        public MapConfig Map { get; init; } = new();
        public PlayerConfig Player { get; init; } = new();
        public List<EnemySpawnConfig> Enemies { get; init; } = [];
        public List<ItemConfig> Items { get; init; } = [];
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
        public int Quantity { get; init; } = 1;
    }

    public sealed class ConditionConfig
    {
        public List<string> Win { get; init; } = [];
        public List<string> Lose { get; init; } = new() { "PlayerDeath" };
    }
}
