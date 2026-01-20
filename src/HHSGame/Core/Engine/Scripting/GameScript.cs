namespace HHSGame.Core.Engine.Scripting
{
    public sealed class GameScript
    {
        public string? Name { get; init; }
        public List<ScriptStep> Steps { get; init; } = [];
    }

    public sealed class ScriptStep
    {
        public string? Input { get; init; }
        public int[]? Target { get; init; }
        public ScriptCondition? WaitUntil { get; init; }
        public ScriptCondition? When { get; init; }
        public List<ScriptStep>? Then { get; init; }
        public List<ScriptStep>? Else { get; init; }
        public ScriptRepeat? Repeat { get; init; }
        public ScriptCondition? Assert { get; init; }
    }

    public sealed class ScriptRepeat
    {
        public ScriptCondition? While { get; init; }
        public int? MaxIterations { get; init; }
        public List<ScriptStep> Steps { get; init; } = [];
    }

    public sealed class ScriptCondition
    {
        public string? State { get; init; }
        public bool? CombatActive { get; init; }
        public bool? NoVisibleEnemies { get; init; }
        public int? PlayerX { get; init; }
        public int? PlayerY { get; init; }
        public int? MinAp { get; init; }
        public int? MaxAp { get; init; }
        public int? MinTurnCount { get; init; }
        public int? MaxTurnCount { get; init; }
    }
}
