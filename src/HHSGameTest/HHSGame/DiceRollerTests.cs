using HHSGame.Core.CharacterCreation;
using HHSGame.Core.Stats;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class DiceRollerTests
    {
        // ── RollAttribute ──────────────────────────────────────────────

        [TestMethod]
        public void RollAttribute_ReturnsTotal_Between3And18()
        {
            Random random = new(1234);
            for (int i = 0; i < 100; i++)
            {
                DiceRollResult result = DiceRoller.RollAttribute(random);
                Assert.IsTrue(result.Total >= 3, $"Total {result.Total} should be >= 3");
                Assert.IsTrue(result.Total <= 18, $"Total {result.Total} should be <= 18");
            }
        }

        [TestMethod]
        public void RollAttribute_Returns4Rolls()
        {
            Random random = new(1234);
            DiceRollResult result = DiceRoller.RollAttribute(random);
            Assert.AreEqual(4, result.Rolls.Length);
        }

        [TestMethod]
        public void RollAttribute_EachRoll_Between1And6()
        {
            Random random = new(1234);
            for (int i = 0; i < 50; i++)
            {
                DiceRollResult result = DiceRoller.RollAttribute(random);
                for (int j = 0; j < 4; j++)
                {
                    Assert.IsTrue(result.Rolls[j] >= 1 && result.Rolls[j] <= 6);
                }
            }
        }

        [TestMethod]
        public void RollAttribute_TotalEqualsSumMinusDropped()
        {
            Random random = new(1234);
            for (int i = 0; i < 50; i++)
            {
                DiceRollResult result = DiceRoller.RollAttribute(random);
                int sum = result.Rolls.Sum();
                Assert.AreEqual(sum - result.Dropped, result.Total);
            }
        }

        [TestMethod]
        public void RollAttribute_DroppedIsTheLowest()
        {
            Random random = new(1234);
            for (int i = 0; i < 50; i++)
            {
                DiceRollResult result = DiceRoller.RollAttribute(random);
                int min = result.Rolls.Min();
                Assert.AreEqual(min, result.Dropped);
            }
        }

        // ── RollAllAttributes ──────────────────────────────────────────

        [TestMethod]
        public void RollAllAttributes_AllValuesBetween3And18()
        {
            Random random = new(1234);
            Attributes attrs = DiceRoller.RollAllAttributes(random);

            Assert.IsTrue(attrs.Strength >= 3 && attrs.Strength <= 18);
            Assert.IsTrue(attrs.Perception >= 3 && attrs.Perception <= 18);
            Assert.IsTrue(attrs.Agility >= 3 && attrs.Agility <= 18);
            Assert.IsTrue(attrs.Charisma >= 3 && attrs.Charisma <= 18);
            Assert.IsTrue(attrs.Intelligence >= 3 && attrs.Intelligence <= 18);
        }

        [TestMethod]
        public void RollAllAttributes_DifferentSeeds_GiveDifferentResults()
        {
            Random r1 = new(111);
            Random r2 = new(999);
            Attributes a1 = DiceRoller.RollAllAttributes(r1);
            Attributes a2 = DiceRoller.RollAllAttributes(r2);

            // Extremely unlikely to be identical
            bool allSame = a1.Strength == a2.Strength
                && a1.Perception == a2.Perception
                && a1.Agility == a2.Agility
                && a1.Charisma == a2.Charisma
                && a1.Intelligence == a2.Intelligence;
            Assert.IsFalse(allSame, "Different seeds should produce different attributes");
        }

        // ── RollWithBias ───────────────────────────────────────────────

        [TestMethod]
        public void RollWithBias_PreferredAttributes_GetBonus()
        {
            Random random = new(1234);
            PlaystylePreference pref = new()
            {
                Name = "Test",
                PreferredAttributes = [AttributeType.Strength, AttributeType.Agility]
            };

            // Run many times to verify bias
            int strSum = 0, agiSum = 0, intSum = 0;
            const int iterations = 200;
            for (int i = 0; i < iterations; i++)
            {
                Attributes attrs = DiceRoller.RollWithBias(random, pref);
                strSum += attrs.Strength;
                agiSum += attrs.Agility;
                intSum += attrs.Intelligence;
            }

            double strAvg = (double)strSum / iterations;
            double agiAvg = (double)agiSum / iterations;
            double intAvg = (double)intSum / iterations;

            // Biased attributes should have higher average
            Assert.IsTrue(strAvg > intAvg, $"STR avg {strAvg:F1} should be > INT avg {intAvg:F1}");
            Assert.IsTrue(agiAvg > intAvg, $"AGI avg {agiAvg:F1} should be > INT avg {intAvg:F1}");
        }

        [TestMethod]
        public void RollWithBias_CapsAt18()
        {
            // Use a seed that produces high rolls
            Random random = new(42);
            PlaystylePreference pref = new()
            {
                Name = "Test",
                PreferredAttributes = [AttributeType.Strength]
            };

            for (int i = 0; i < 100; i++)
            {
                Attributes attrs = DiceRoller.RollWithBias(random, pref);
                Assert.IsTrue(attrs.Strength <= 18, $"STR {attrs.Strength} should be <= 18");
            }
        }

        // ── GenerateName ───────────────────────────────────────────────

        [TestMethod]
        public void GenerateName_ReturnsNonEmptyString()
        {
            Random random = new(1234);
            string name = DiceRoller.GenerateName(random);
            Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        }

        [TestMethod]
        public void GenerateName_ReturnsDifferentNames()
        {
            Random random = new(1234);
            HashSet<string> names = new();
            for (int i = 0; i < 20; i++)
            {
                names.Add(DiceRoller.GenerateName(random));
            }
            Assert.IsTrue(names.Count > 1, "Should generate varied names");
        }
    }
}