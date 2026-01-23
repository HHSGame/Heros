namespace HHSGame.Core.Quests
{
    public enum QuestObjectiveKind
    {
        KillEnemy,
        CollectItem,
        ReachMarker,
        TalkToNpc
    }

    public sealed record QuestObjectiveDefinition(
        QuestObjectiveKind Kind,
        string TargetId,
        int RequiredCount);

    public enum QuestRewardKind
    {
        Item,
        Currency,
        Experience
    }

    public sealed record QuestRewardDefinition(
        QuestRewardKind Kind,
        string TargetId,
        int Amount);

    public sealed record QuestDefinition(
        string Id,
        string Name,
        string Description,
        string Hint,
        IReadOnlyList<QuestObjectiveDefinition> Objectives,
        IReadOnlyList<QuestRewardDefinition> Rewards);

    public sealed record AchievementDefinition(
        string Id,
        string Name,
        string Description,
        IReadOnlyList<QuestObjectiveDefinition> Objectives);
}
