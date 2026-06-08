namespace HHSGame.Core.Tutorial
{
    /// <summary>
    /// Defines a single tutorial step with a trigger condition and hint message.
    /// </summary>
    public sealed class TutorialStep
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public TutorialTrigger Trigger { get; init; }
        public string? TriggerParam { get; init; }
        public int Order { get; init; }
    }

    /// <summary>
    /// What triggers a tutorial hint to appear.
    /// </summary>
    public enum TutorialTrigger
    {
        /// <summary>Show immediately when the step becomes active.</summary>
        Immediate,
        /// <summary>Triggered when the player moves.</summary>
        OnMove,
        /// <summary>Triggered when combat starts.</summary>
        OnCombatStart,
        /// <summary>Triggered when the player picks up an item.</summary>
        OnItemPickup,
        /// <summary>Triggered when the player opens inventory.</summary>
        OnInventoryOpen,
        /// <summary>Triggered when the player talks to an NPC.</summary>
        OnNpcTalk,
        /// <summary>Triggered when the player uses a skill.</summary>
        OnSkillUse,
        /// <summary>Triggered when the player interacts with an object.</summary>
        OnObjectInteract,
        /// <summary>Triggered when the player reaches a specific position/marker.</summary>
        OnMarkerReached,
        /// <summary>Triggered when turn ends.</summary>
        OnTurnEnd,
        /// <summary>Triggered when save/load is used.</summary>
        OnSaveOrLoad,
        /// <summary>Triggered when combat mode is toggled.</summary>
        OnCombatToggle
    }

    /// <summary>
    /// The full tutorial scenario definition.
    /// </summary>
    public sealed class TutorialScenario
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? MapPath { get; init; }
        public List<TutorialStep> Steps { get; init; } = [];
    }
}