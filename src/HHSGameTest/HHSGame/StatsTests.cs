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
            Item item = new HealthPotion("HealthPotion", "Health Potion", ItemRarity.Common, 50, 0.5f, 10);
            int buyPrice = TradeService.GetBuyPrice(item, 0);
            int sellPrice = TradeService.GetSellPrice(item, 0);

            Assert.AreEqual(75, buyPrice);
            Assert.AreEqual(15, sellPrice);
        }

        [TestMethod]
        public void KarmaUsesMedianOfAttributes()
        {
            Attributes attributes = new()
            {
                Strength = 8,
                Perception = 6,
                Agility = 5,
                Charisma = 4,
                Intelligence = 4
            };

            Assert.AreEqual(5, attributes.Karma);
        }

        [TestMethod]
        public void SkillValueAddsParentAttributeAndRank()
        {
            Attributes attributes = new()
            {
                Strength = 6,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            skills.SetRank(SkillType.Melee, 10);

            Assert.AreEqual(16, skills.GetValue(SkillType.Melee, attributes));
        }

        [TestMethod]
        public void SkillValueClampsToHardCap()
        {
            Attributes attributes = new()
            {
                Strength = 10,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            skills.SetRank(SkillType.Melee, 95);

            Assert.AreEqual(100, skills.GetValue(SkillType.Melee, attributes));
        }

        [TestMethod]
        public void DerivedStatsComputeCorrectly()
        {
            Attributes attributes = new()
            {
                Strength = 5,
                Perception = 6,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            skills.SetRank(SkillType.Dexterity, 3);

            CharacterStats stats = new(attributes, skills);

            Assert.AreEqual(35, stats.MaxHp);
            Assert.AreEqual(50, stats.CarryCapacity);
            Assert.AreEqual(14, stats.Initiative);
        }

        [TestMethod]
        public void TrySpendApFailsWhenInsufficient()
        {
            Attributes attributes = new()
            {
                Strength = 5,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            CharacterStats stats = new(attributes, skills);
            stats.ResetTurn();

            int before = stats.CurrentAp;
            Assert.IsFalse(stats.TrySpendAp(before + 1));
            Assert.AreEqual(before, stats.CurrentAp);
        }

        [TestMethod]
        public void TrySpendApIgnoresNonPositiveCost()
        {
            Attributes attributes = new()
            {
                Strength = 5,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            CharacterStats stats = new(attributes, skills);
            stats.ResetTurn();

            int before = stats.CurrentAp;
            Assert.IsTrue(stats.TrySpendAp(0));
            Assert.AreEqual(before, stats.CurrentAp);
            Assert.IsTrue(stats.TrySpendAp(-2));
            Assert.AreEqual(before, stats.CurrentAp);
        }

        [TestMethod]
        public void ApplyDamageAndHealClampToBounds()
        {
            Attributes attributes = new()
            {
                Strength = 5,
                Perception = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            CharacterStats stats = new(attributes, skills);

            stats.ApplyDamage(stats.MaxHp + 10);
            Assert.AreEqual(0, stats.CurrentHp);

            stats.Heal(5);
            Assert.AreEqual(5, stats.CurrentHp);

            stats.Heal(stats.MaxHp);
            Assert.AreEqual(stats.MaxHp, stats.CurrentHp);
        }

        [TestMethod]
        public void AttributeModifierAppliesToCopy()
        {
            Attributes attributes = new()
            {
                Strength = 4,
                Perception = 5,
                Agility = 6,
                Charisma = 7,
                Intelligence = 8
            };

            AttributeModifier modifier = new(AttributeType.Strength, 3);
            Attributes modified = modifier.Apply(attributes);

            Assert.AreEqual(4, attributes.Strength);
            Assert.AreEqual(7, modified.Strength);
            Assert.AreEqual(attributes.Perception, modified.Perception);
        }

        [TestMethod]
        public void SkillModifierClampsToHardCap()
        {
            Skills skills = new();
            SkillModifier modifier = new(SkillType.Melee, 150);
            modifier.Apply(skills);

            Assert.AreEqual(ProgressionRules.HardSkillCap, skills.GetRank(SkillType.Melee));
        }

        [TestMethod]
        public void SkillValueUsesParentAttribute()
        {
            Attributes attributes = new()
            {
                Strength = 4,
                Perception = 7,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5
            };
            Skills skills = new();
            skills.SetRank(SkillType.Firearms, 2);

            Assert.AreEqual(9, skills.GetValue(SkillType.Firearms, attributes));
        }

        [TestMethod]
        public void TradeServiceClampsBuyPriceAndScalesSellPrice()
        {
            Item item = new HealthPotion("HealthPotion", "Health Potion", ItemRarity.Common, 50, 0.5f, 10);
            int buyPrice = TradeService.GetBuyPrice(item, 100);
            int sellPrice = TradeService.GetSellPrice(item, 100);

            Assert.AreEqual(1, buyPrice);
            Assert.AreEqual(115, sellPrice);
        }
    }
}
