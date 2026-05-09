using HHSGame.Core.Stats;

namespace HHSGame.Core.CharacterCreation
{
    /// <summary>
    /// Represents a player's preferred playstyle, influencing attribute rolls and class suggestion.
    /// </summary>
    public sealed record PlaystylePreference
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public List<AttributeType> PreferredAttributes { get; init; } = [];
        public string? SuggestedClassId { get; init; }
    }

    /// <summary>
    /// Defines available playstyle preferences for character creation dialogue.
    /// </summary>
    public static class PlaystyleCatalog
    {
        public static readonly List<PlaystylePreference> All =
        [
            new PlaystylePreference
            {
                Name = "抵抗战士",
                Description = "你擅长潜行和破坏，敏捷和感知是你的生存之道。",
                PreferredAttributes = [AttributeType.Agility, AttributeType.Perception],
                SuggestedClassId = "Resistant"
            },
            new PlaystylePreference
            {
                Name = "占领军军官",
                Description = "你习惯以武力和权威解决问题，力量和魅力是你的资本。",
                PreferredAttributes = [AttributeType.Strength, AttributeType.Charisma],
                SuggestedClassId = "OccupierOfficer"
            },
            new PlaystylePreference
            {
                Name = "傀儡政权公务员",
                Description = "你在夹缝中生存，社交和智慧让你左右逢源。",
                PreferredAttributes = [AttributeType.Charisma, AttributeType.Intelligence],
                SuggestedClassId = "Bureaucrat"
            },
            new PlaystylePreference
            {
                Name = "沦陷区平民",
                Description = "你只是想活下去，灵活变通是你的本能。",
                PreferredAttributes = [AttributeType.Agility, AttributeType.Intelligence],
                SuggestedClassId = "Civilian"
            },
            new PlaystylePreference
            {
                Name = "流亡士兵",
                Description = "你经历过战争的残酷，力量和感知是你的武器。",
                PreferredAttributes = [AttributeType.Strength, AttributeType.Perception],
                SuggestedClassId = "ExiledSoldier"
            },
            new PlaystylePreference
            {
                Name = "匪徒",
                Description = "混乱就是你的秩序，力量和敏捷让你为所欲为。",
                PreferredAttributes = [AttributeType.Strength, AttributeType.Agility],
                SuggestedClassId = "Bandit"
            }
        ];

        public static PlaystylePreference? GetById(string id)
        {
            return All.FirstOrDefault(p => string.Equals(p.SuggestedClassId, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}