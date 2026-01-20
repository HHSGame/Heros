using HHSGame.Core.Items;
using HHSGame.Core.Stats;

namespace HHSGame.Core
{
    [TestClass]
    public class StatsTests
    {
        [TestMethod]
        public void BaseApIncludesAthleticsSynergy()
        {
            Attributes attributes = new()
            {
                Strength = 10,
                Perception = 5,
                Agility = 6,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            skills.SetRank(SkillType.Athletics, 40); // 10 + 40 = 50

            CharacterStats stats = new(attributes, skills);

            Assert.AreEqual(9, stats.MaxAp);
        }

        [TestMethod]
        public void EndTurnConvertsApToEvasion()
        {
            Attributes attributes = new()
            {
                Strength = 5,
                Perception = 5,
                Agility = 6,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            CharacterStats stats = new(attributes, skills);
            stats.ResetTurn();
            Assert.IsTrue(stats.TrySpendAp(2));
            stats.EndTurn();

            Assert.AreEqual(0, stats.CurrentAp);
            Assert.AreEqual(stats.MaxAp - 2, stats.EvasionBonus);
        }

        [TestMethod]
        public void ProgressionConsumesExperiencePerLevel()
        {
            Attributes attributes = new()
            {
                Strength = 5,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 6
            };

            CharacterProgression progression = new();
            int required = ProgressionRules.ExperienceForLevel(1);

            Assert.IsTrue(progression.TryAddExperience(required, attributes));
            Assert.AreEqual(2, progression.Level);
            Assert.AreEqual(0, progression.Experience);
            Assert.AreEqual(13, progression.UnspentSkillPoints);
        }

        [TestMethod]
        public void TradeServiceUsesBarterSkill()
        {
            Item item = new HealthPotion(10);
            int buyPrice = TradeService.GetBuyPrice(item, 0);
            int sellPrice = TradeService.GetSellPrice(item, 0);

            Assert.AreEqual(75, buyPrice);
            Assert.AreEqual(15, sellPrice);
        }
    }
}
