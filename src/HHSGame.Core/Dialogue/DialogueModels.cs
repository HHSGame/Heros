using HHSGame.Core.Quests;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Dialogue
{
    public enum DialogueRequirementType
    {
        Skill,
        Attribute,
        QuestStatus,
        QuestObjectiveComplete,
        LuaCondition  // 新增：Lua 条件
    }

    public sealed record DialogueRequirement(
        DialogueRequirementType Type,
        SkillType? Skill,
        AttributeType? Attribute,
        string? QuestId,
        QuestStatus? QuestStatus,
        int Minimum,
        string? LuaCondition = null);  // 新增：Lua 条件脚本

    public enum DialogueEffectType
    {
        StartQuest,
        CompleteQuest,
        GiveItem,
        AddCurrency,
        UnlockAchievement,
        ModifyReputation,
        SetRelation,
        LuaScript  // 新增：Lua 脚本
    }

    public sealed record DialogueEffect(
        DialogueEffectType Type,
        string Target,
        int Amount,
        string? LuaScript = null);  // 新增：Lua 脚本

    public sealed record DialogueOption(
        string Text,
        string? NextNodeId,
        IReadOnlyList<DialogueRequirement> Requirements,
        IReadOnlyList<DialogueEffect> Effects);

    public sealed record DialogueNode(
        string Id,
        string Text,
        IReadOnlyList<DialogueOption> Options);

    public sealed record DialogueDefinition(
        string Id,
        string StartNodeId,
        IReadOnlyDictionary<string, DialogueNode> Nodes);
}
