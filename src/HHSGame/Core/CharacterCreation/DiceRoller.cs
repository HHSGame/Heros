using HHSGame.Core.Stats;

namespace HHSGame.Core.CharacterCreation
{
    /// <summary>
    /// Result of a dice roll for character creation.
    /// </summary>
    public sealed record DiceRollResult(int[] Rolls, int Dropped, int Total);

    /// <summary>
    /// Generates random dice rolls for character attributes.
    /// Uses 4d6-drop-lowest method (D&D style).
    /// </summary>
    public static class DiceRoller
    {
        /// <summary>
        /// Rolls 4d6, drops the lowest, returns the sum.
        /// </summary>
        public static DiceRollResult RollAttribute(Random random)
        {
            int[] rolls = new int[4];
            for (int i = 0; i < 4; i++)
            {
                rolls[i] = random.Next(1, 7);
            }

            int minIndex = 0;
            for (int i = 1; i < 4; i++)
            {
                if (rolls[i] < rolls[minIndex])
                {
                    minIndex = i;
                }
            }

            int dropped = rolls[minIndex];
            int total = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i != minIndex)
                {
                    total += rolls[i];
                }
            }

            return new DiceRollResult(rolls, dropped, total);
        }

        /// <summary>
        /// Rolls all 5 attributes (4d6-drop-lowest each).
        /// </summary>
        public static Attributes RollAllAttributes(Random random)
        {
            return new Attributes
            {
                Strength = Math.Clamp(RollAttribute(random).Total, 3, 18),
                Perception = Math.Clamp(RollAttribute(random).Total, 3, 18),
                Agility = Math.Clamp(RollAttribute(random).Total, 3, 18),
                Charisma = Math.Clamp(RollAttribute(random).Total, 3, 18),
                Intelligence = Math.Clamp(RollAttribute(random).Total, 3, 18)
            };
        }

        /// <summary>
        /// Rolls attributes with a bias toward a preferred playstyle.
        /// The preferred attributes get a +2 bonus (capped at 18).
        /// </summary>
        public static Attributes RollWithBias(Random random, PlaystylePreference preference)
        {
            Attributes attrs = RollAllAttributes(random);

            foreach (AttributeType attr in preference.PreferredAttributes)
            {
                switch (attr)
                {
                    case AttributeType.Strength:
                        attrs.Strength = Math.Min(18, attrs.Strength + 2);
                        break;
                    case AttributeType.Perception:
                        attrs.Perception = Math.Min(18, attrs.Perception + 2);
                        break;
                    case AttributeType.Agility:
                        attrs.Agility = Math.Min(18, attrs.Agility + 2);
                        break;
                    case AttributeType.Charisma:
                        attrs.Charisma = Math.Min(18, attrs.Charisma + 2);
                        break;
                    case AttributeType.Intelligence:
                        attrs.Intelligence = Math.Min(18, attrs.Intelligence + 2);
                        break;
                }
            }

            return attrs;
        }

        /// <summary>
        /// Generates a random character name.
        /// </summary>
        public static string GenerateName(Random random)
        {
            string[] firstParts = ["Ar", "Bel", "Cor", "Dar", "El", "Fen", "Gal", "Hal", "Iv", "Kel",
                "Lor", "Mor", "Nor", "Ori", "Pel", "Quin", "Ral", "Sar", "Tor", "Val"];
            string[] secondParts = ["an", "en", "in", "on", "us", "ar", "or", "el", "ia", "ra",
                "don", "wen", "mir", "dor", "nor", "ith", "ath", "eth", "orn", "und"];

            string first = firstParts[random.Next(firstParts.Length)];
            string second = secondParts[random.Next(secondParts.Length)];
            return first + second;
        }
    }
}