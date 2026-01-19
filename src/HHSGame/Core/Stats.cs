
namespace HHSGame.Core.Stats
{
    public record CoreAttributes
    {
        public int Strength { get; set; }
        public int Perception { get; set; }
        public int Agility { get; set; }
        public int Charisma { get; set; }
        public int Intelligence { get; set; }
    }

    public enum CoreAttributeType
    {
        Strength,
        Perception,
        Agility,
        Charisma,
        Intelligence,
    }

    public record CoreAttributeModifier(CoreAttributeType Type, int Value)
    {
        public CoreAttributes Apply(CoreAttributes origin)
        {
            return Type switch
            {
                CoreAttributeType.Strength => origin with { Strength = origin.Strength + Value },
                CoreAttributeType.Perception => origin with { Perception = origin.Perception + Value },
                CoreAttributeType.Agility => origin with { Agility = origin.Agility + Value },
                CoreAttributeType.Charisma => origin with { Charisma = origin.Charisma + Value },
                CoreAttributeType.Intelligence => origin with { Intelligence = origin.Intelligence + Value },
                _ => origin,
            };
        }
    }

    public record Skills
    {
        // strength
        public int Combat { get; set; }
        public int Stamina { get; set; }
        public int Survival { get; set; }

        // perception
        public int Comprehension { get; set; }
        public int Resolution { get; set; }
        public int Sense { get; set; }

        // agility
        public int Sneak { get; set; }
        public int Reflection { get; set; }
        public int Mechanics { get; set; }

        // charisma
        public int Barter { get; set; }
        public int Lead { get; set; }
        public int Speech { get; set; }

        // intelligence
        public int Knowledge { get; set; }
        public int Medical { get; set; }
        public int Religion { get; set; }
    }

    public enum SkillType
    {
        Combat,
        Stamina,
        Survival,
        Comprehension,
        Resolution,
        Sense,
        Sneak,
        Reflection,
        Mechanics,
        Barter,
        Lead,
        Speech,
        Knowledge,
        Medical,
        Religion,
    }

    public record SkillModifier(SkillType Type, int Value)
    {
        public Skills Apply(Skills origin)
        {
            return Type switch
            {
                SkillType.Combat => origin with { Combat = origin.Combat + Value },
                SkillType.Stamina => origin with { Stamina = origin.Stamina + Value },
                SkillType.Survival => origin with { Survival = origin.Survival + Value },
                SkillType.Comprehension => origin with { Comprehension = origin.Comprehension + Value },
                SkillType.Resolution => origin with { Resolution = origin.Resolution + Value },
                SkillType.Sense => origin with { Sense = origin.Sense + Value },
                SkillType.Sneak => origin with { Sneak = origin.Sneak + Value },
                SkillType.Reflection => origin with { Reflection = origin.Reflection + Value },
                SkillType.Mechanics => origin with { Mechanics = origin.Mechanics + Value },
                SkillType.Barter => origin with { Barter = origin.Barter + Value },
                SkillType.Lead => origin with { Lead = origin.Lead + Value },
                SkillType.Speech => origin with { Speech = origin.Speech + Value },
                SkillType.Knowledge => origin with { Knowledge = origin.Knowledge + Value },
                SkillType.Medical => origin with { Medical = origin.Medical + Value },
                SkillType.Religion => origin with { Religion = origin.Religion + Value },
                _ => origin,
            };
        }
    }
}