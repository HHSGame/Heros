namespace HHSGame.Core.Stats
{
    public enum AttributeType
    {
        Strength,
        Perception,
        Agility,
        Charisma,
        Intelligence,
        Karma
    }

    public sealed record Attributes
    {
        public int Strength { get; set; }
        public int Perception { get; set; }
        public int Agility { get; set; }
        public int Charisma { get; set; }
        public int Intelligence { get; set; }

        public int Karma => CalculateKarma(Strength, Perception, Agility, Charisma, Intelligence);

        public int Get(AttributeType type)
        {
            return type switch
            {
                AttributeType.Strength => Strength,
                AttributeType.Perception => Perception,
                AttributeType.Agility => Agility,
                AttributeType.Charisma => Charisma,
                AttributeType.Intelligence => Intelligence,
                AttributeType.Karma => Karma,
                _ => 0
            };
        }

        private static int CalculateKarma(int strength, int perception, int agility, int charisma, int intelligence)
        {
            int[] values = [strength, perception, agility, charisma, intelligence];
            Array.Sort(values);
            return values[values.Length / 2];
        }
    }

    public enum SkillType
    {
        Melee,
        Athletics,
        Survival,
        Firearms,
        Awareness,
        Resolution,
        Stealth,
        Dexterity,
        Mechanics,
        Barter,
        Leadership,
        Persuasion,
        Knowledge,
        Medicine,
        Religion
    }

    public static class SkillDefinitions
    {
        private static readonly Dictionary<SkillType, AttributeType> ParentAttributes = new()
        {
            { SkillType.Melee, AttributeType.Strength },
            { SkillType.Athletics, AttributeType.Strength },
            { SkillType.Survival, AttributeType.Strength },
            { SkillType.Firearms, AttributeType.Perception },
            { SkillType.Awareness, AttributeType.Perception },
            { SkillType.Resolution, AttributeType.Perception },
            { SkillType.Stealth, AttributeType.Agility },
            { SkillType.Dexterity, AttributeType.Agility },
            { SkillType.Mechanics, AttributeType.Agility },
            { SkillType.Barter, AttributeType.Charisma },
            { SkillType.Leadership, AttributeType.Charisma },
            { SkillType.Persuasion, AttributeType.Charisma },
            { SkillType.Knowledge, AttributeType.Intelligence },
            { SkillType.Medicine, AttributeType.Intelligence },
            { SkillType.Religion, AttributeType.Intelligence }
        };

        public static AttributeType GetParent(SkillType type)
        {
            return ParentAttributes[type];
        }
    }

    public sealed class Skills
    {
        private readonly Dictionary<SkillType, int> ranks = new();

        public int GetRank(SkillType type)
        {
            return ranks.TryGetValue(type, out int value) ? value : 0;
        }

        public void SetRank(SkillType type, int value)
        {
            ranks[type] = Math.Clamp(value, 0, ProgressionRules.HardSkillCap);
        }

        public void AddRank(SkillType type, int delta)
        {
            SetRank(type, GetRank(type) + delta);
        }

        public int GetValue(SkillType type, Attributes attributes)
        {
            AttributeType parent = SkillDefinitions.GetParent(type);
            int baseValue = attributes.Get(parent);
            int total = baseValue + GetRank(type);
            return Math.Clamp(total, 0, ProgressionRules.HardSkillCap);
        }

        public Skills Clone()
        {
            Skills copy = new();
            foreach (KeyValuePair<SkillType, int> entry in ranks)
            {
                copy.SetRank(entry.Key, entry.Value);
            }
            return copy;
        }
    }

    public static class ProgressionRules
    {
        public const int HardSkillCap = 100;

        public static int ExperienceForLevel(int level)
        {
            return (int)Math.Round(100 * Math.Pow(level, 1.5));
        }

        public static int SkillPointsPerLevel(Attributes attributes)
        {
            return 10 + (attributes.Intelligence / 2);
        }

        public static int SoftSkillCap(int level)
        {
            return level * 5 + 20;
        }
    }

    public sealed class CharacterProgression
    {
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int UnspentSkillPoints { get; private set; }

        public bool TryAddExperience(int amount, Attributes attributes)
        {
            if (amount <= 0)
            {
                return false;
            }

            Experience += amount;
            bool leveledUp = false;

            int required = ProgressionRules.ExperienceForLevel(Level);
            while (Experience >= required)
            {
                Experience -= required;
                Level++;
                UnspentSkillPoints += ProgressionRules.SkillPointsPerLevel(attributes);
                leveledUp = true;
                required = ProgressionRules.ExperienceForLevel(Level);
            }

            return leveledUp;
        }

        public bool TrySpendSkillPoints(int cost)
        {
            if (cost <= 0 || cost > UnspentSkillPoints)
            {
                return false;
            }

            UnspentSkillPoints -= cost;
            return true;
        }

        public void SetLevel(int level)
        {
            Level = Math.Max(1, level);
        }

        public void SetExperience(int exp)
        {
            Experience = Math.Max(0, exp);
        }

        public void SetUnspentSkillPoints(int points)
        {
            UnspentSkillPoints = Math.Max(0, points);
        }
    }

    public static class DerivedStats
    {
        public static int MaxHp(Attributes attributes, Skills skills)
        {
            return 20 + (attributes.Strength * 2) + skills.GetValue(SkillType.Athletics, attributes);
        }

        public static int MaxSp(Attributes attributes, Skills skills)
        {
            return (attributes.Charisma + skills.GetValue(SkillType.Resolution, attributes)) * 2;
        }

        public static int BaseAp(Attributes attributes, Skills skills)
        {
            int ap = 5 + (attributes.Agility / 2);
            if (skills.GetValue(SkillType.Athletics, attributes) >= 50)
            {
                ap += 1;
            }
            return ap;
        }

        public static int CarryCapacity(Attributes attributes)
        {
            return attributes.Strength * 10;
        }

        public static int Initiative(Attributes attributes, Skills skills)
        {
            return attributes.Perception + skills.GetValue(SkillType.Dexterity, attributes);
        }
    }

    public sealed class CharacterStats
    {
        public Attributes Attributes { get; }
        public Skills Skills { get; }
        public CharacterProgression Progression { get; }

        public int CurrentHp { get; private set; }
        public int CurrentSp { get; private set; }
        public int CurrentAp { get; private set; }
        public int EvasionBonus { get; private set; }

        public CharacterStats(Attributes attributes, Skills skills, CharacterProgression? progression = null)
        {
            Attributes = attributes;
            Skills = skills;
            Progression = progression ?? new CharacterProgression();
            CurrentHp = MaxHp;
            CurrentSp = MaxSp;
            CurrentAp = MaxAp;
        }

        public int MaxHp => DerivedStats.MaxHp(Attributes, Skills);
        public int MaxSp => DerivedStats.MaxSp(Attributes, Skills);
        public int MaxAp => DerivedStats.BaseAp(Attributes, Skills);
        public int CarryCapacity => DerivedStats.CarryCapacity(Attributes);
        public int Initiative => DerivedStats.Initiative(Attributes, Skills);

        public void ResetTurn(bool includeAp = true)
        {
            CurrentAp = includeAp ? MaxAp : 0;
            EvasionBonus = 0;
        }

        public void EndTurn()
        {
            EvasionBonus = CurrentAp;
            CurrentAp = 0;
        }

        public bool TrySpendAp(int cost)
        {
            if (cost <= 0)
            {
                return true;
            }

            if (CurrentAp < cost)
            {
                return false;
            }

            CurrentAp -= cost;
            return true;
        }

        public void ApplyDamage(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            CurrentHp = Math.Max(0, CurrentHp - damage);
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        }

        public void SetHp(int hp)
        {
            CurrentHp = Math.Clamp(hp, 0, MaxHp);
        }

        public void SetSp(int sp)
        {
            CurrentSp = Math.Clamp(sp, 0, MaxSp);
        }

        public void RestoreSanity(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentSp = Math.Min(MaxSp, CurrentSp + amount);
        }
    }

    public record AttributeModifier(AttributeType Type, int Value)
    {
        public Attributes Apply(Attributes origin)
        {
            return Type switch
            {
                AttributeType.Strength => origin with { Strength = origin.Strength + Value },
                AttributeType.Perception => origin with { Perception = origin.Perception + Value },
                AttributeType.Agility => origin with { Agility = origin.Agility + Value },
                AttributeType.Charisma => origin with { Charisma = origin.Charisma + Value },
                AttributeType.Intelligence => origin with { Intelligence = origin.Intelligence + Value },
                _ => origin
            };
        }
    }

    public record SkillModifier(SkillType Type, int Value)
    {
        public Skills Apply(Skills origin)
        {
            origin.AddRank(Type, Value);
            return origin;
        }
    }
}
