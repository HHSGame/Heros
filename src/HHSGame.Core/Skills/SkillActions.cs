using HHSGame.Core.Stats;

namespace HHSGame.Core.SkillSystem
{
    public enum SkillActionId
    {
        Inspect,
        Search,
        Sneak,
        Pickpocket,
        Inspire,
        Shove,
        Leap,
        Forage,
        Aim,
        SteadyMind,
        PickLock,
        Appraise,
        Demoralize,
        TreatWounds,
        Bless,
        Examine,
        UseObject
    }

    public enum SkillActionTargetType
    {
        None,
        Direction,
        AdjacentEnemy,
        AdjacentNpc,
        AdjacentAllyOrSelf,
        AdjacentDoor,
        RangedEnemy,
        AdjacentObject
    }

    public sealed record SkillActionDefinition(
        SkillActionId Id,
        string Name,
        SkillType Skill,
        int ApCost,
        SkillActionTargetType TargetType,
        string Description);

    public static class SkillActionCatalog
    {
        private static readonly List<SkillActionDefinition> Actions =
        [
            new SkillActionDefinition(
                SkillActionId.Inspect,
                "Inspect",
                SkillType.Awareness,
                2,
                SkillActionTargetType.None,
                "Scan nearby tiles for hidden passages, traps, or doors."),
            new SkillActionDefinition(
                SkillActionId.Search,
                "Search",
                SkillType.Knowledge,
                2,
                SkillActionTargetType.None,
                "Search the current tile and nearby tiles for items, including hidden ones."),
            new SkillActionDefinition(
                SkillActionId.Sneak,
                "Sneak",
                SkillType.Stealth,
                1,
                SkillActionTargetType.None,
                "Enter or exit stealth. Only high-Awareness enemies will notice you."),
            new SkillActionDefinition(
                SkillActionId.Pickpocket,
                "Pickpocket",
                SkillType.Dexterity,
                3,
                SkillActionTargetType.AdjacentNpc,
                "Attempt to steal a small item from a nearby NPC."),
            new SkillActionDefinition(
                SkillActionId.Inspire,
                "Inspire",
                SkillType.Leadership,
                4,
                SkillActionTargetType.None,
                "Boost the party's checks for the day, at the cost of your own attributes."),
            new SkillActionDefinition(
                SkillActionId.Shove,
                "Shove",
                SkillType.Melee,
                3,
                SkillActionTargetType.AdjacentEnemy,
                "Push an adjacent enemy back one tile if space allows."),
            new SkillActionDefinition(
                SkillActionId.Leap,
                "Leap",
                SkillType.Athletics,
                2,
                SkillActionTargetType.Direction,
                "Leap two tiles in a chosen direction if the path is clear."),
            new SkillActionDefinition(
                SkillActionId.Forage,
                "Forage",
                SkillType.Survival,
                3,
                SkillActionTargetType.None,
                "Search the area for basic supplies."),
            new SkillActionDefinition(
                SkillActionId.Aim,
                "Aim",
                SkillType.Firearms,
                2,
                SkillActionTargetType.None,
                "Prepare a precise shot to increase ranged accuracy."),
            new SkillActionDefinition(
                SkillActionId.SteadyMind,
                "Steady Mind",
                SkillType.Resolution,
                2,
                SkillActionTargetType.None,
                "Shake off mental strain and recover some sanity."),
            new SkillActionDefinition(
                SkillActionId.PickLock,
                "Pick Lock",
                SkillType.Mechanics,
                2,
                SkillActionTargetType.AdjacentDoor,
                "Unlock an adjacent door if you can bypass the mechanism."),
            new SkillActionDefinition(
                SkillActionId.Appraise,
                "Appraise",
                SkillType.Barter,
                1,
                SkillActionTargetType.None,
                "Estimate the value of nearby or carried items."),
            new SkillActionDefinition(
                SkillActionId.Demoralize,
                "Demoralize",
                SkillType.Persuasion,
                3,
                SkillActionTargetType.AdjacentEnemy,
                "Rattle an adjacent enemy to delay their next action."),
            new SkillActionDefinition(
                SkillActionId.TreatWounds,
                "Treat Wounds",
                SkillType.Medicine,
                3,
                SkillActionTargetType.AdjacentAllyOrSelf,
                "Restore health to yourself or a nearby ally."),
            new SkillActionDefinition(
                SkillActionId.Bless,
                "Bless",
                SkillType.Religion,
                3,
                SkillActionTargetType.None,
                "Offer a blessing to restore sanity and ward off afflictions."),
            new SkillActionDefinition(
                SkillActionId.Examine,
                "Examine",
                SkillType.Awareness,
                1,
                SkillActionTargetType.AdjacentObject,
                "Examine an interactive object nearby to learn about it."),
            new SkillActionDefinition(
                SkillActionId.UseObject,
                "Use Object",
                SkillType.Mechanics,
                2,
                SkillActionTargetType.AdjacentObject,
                "Interact with a nearby object (open chest, read inscription, pull lever, etc.).")
        ];

        public static IReadOnlyList<SkillActionDefinition> All => Actions;

        public static SkillActionDefinition Get(SkillActionId id)
        {
            return Actions.First(action => action.Id == id);
        }
    }
}
