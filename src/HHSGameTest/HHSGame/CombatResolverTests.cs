using System.Collections.Generic;
using HHSGame.Core.Items;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Combat
{
    [TestClass]
    public class CombatResolverTests
    {
        private sealed class FixedRandom(params int[] values) : Random
        {
            private readonly Queue<int> queuedValues = new(values);

            public override int Next(int minValue, int maxValue)
            {
                return queuedValues.Count > 0 ? queuedValues.Dequeue() : minValue;
            }
        }

        private sealed class TestCombatant(string name, CharacterStats stats, int armorValue, int evasionBonus, Weapon weapon) : ICombatant
        {
            public string Name { get; } = name;
            public CharacterStats Stats { get; } = stats;
            public int ArmorValue { get; } = armorValue;
            public int EvasionBonus { get; } = evasionBonus;
            public Weapon EquippedWeapon { get; } = weapon;
            public int DamageTaken { get; private set; }

            public void TakeDamage(int damage)
            {
                DamageTaken += damage;
                Stats.ApplyDamage(damage);
            }
        }

        private static CharacterStats BuildStats(int strength, int perception, int agility, int charisma, int intelligence, SkillType? skill = null, int skillRank = 0)
        {
            Attributes attributes = new()
            {
                Strength = strength,
                Perception = perception,
                Agility = agility,
                Charisma = charisma,
                Intelligence = intelligence
            };
            Skills skills = new();
            if (skill.HasValue)
            {
                skills.SetRank(skill.Value, skillRank);
            }

            return new CharacterStats(attributes, skills);
        }

        [TestMethod]
        public void MeleeAttackAddsStrengthAndAppliesArmorPenetration()
        {
            CharacterStats attackerStats = BuildStats(8, 5, 5, 5, 5, SkillType.Melee, 4);
            CharacterStats defenderStats = BuildStats(5, 5, 5, 5, 5);
            Weapon weapon = new("Knife", "Knife", ItemRarity.Common, 10, 1f, 4, 1, 1, WeaponType.MeleeLight, WeaponTrajectory.Line);
            TestCombatant attacker = new("Attacker", attackerStats, 0, 0, weapon);
            TestCombatant defender = new("Defender", defenderStats, 5, 0, weapon);

            bool hit = CombatResolver.ResolveAttack(attacker, defender, weapon, new FixedRandom(10));

            Assert.IsTrue(hit);
            Assert.AreEqual(8, defender.DamageTaken);
            Assert.AreEqual(defenderStats.MaxHp - 8, defenderStats.CurrentHp);
        }

        [TestMethod]
        public void RangedAttackDoesNotAddStrength()
        {
            CharacterStats attackerStats = BuildStats(12, 6, 5, 5, 5, SkillType.Firearms, 4);
            CharacterStats defenderStats = BuildStats(5, 5, 5, 5, 5);
            Weapon weapon = new("Pistol", "Pistol", ItemRarity.Common, 10, 1f, 5, 0, 5, WeaponType.RangedSnap, WeaponTrajectory.Line);
            TestCombatant attacker = new("Attacker", attackerStats, 0, 0, weapon);
            TestCombatant defender = new("Defender", defenderStats, 0, 0, weapon);

            bool hit = CombatResolver.ResolveAttack(attacker, defender, weapon, new FixedRandom(5));

            Assert.IsTrue(hit);
            Assert.AreEqual(5, defender.DamageTaken);
        }

        [TestMethod]
        public void CriticalFailureAlwaysMisses()
        {
            CharacterStats attackerStats = BuildStats(10, 5, 5, 5, 5, SkillType.Melee, 20);
            CharacterStats defenderStats = BuildStats(5, 5, 5, 5, 5);
            Weapon weapon = new("Sword", "Sword", ItemRarity.Common, 10, 1f, 6, 0, 1, WeaponType.MeleeHeavy, WeaponTrajectory.Line);
            TestCombatant attacker = new("Attacker", attackerStats, 0, 0, weapon);
            TestCombatant defender = new("Defender", defenderStats, 0, 0, weapon);

            bool hit = CombatResolver.ResolveAttack(attacker, defender, weapon, new FixedRandom(20));

            Assert.IsFalse(hit);
            Assert.AreEqual(0, defender.DamageTaken);
        }

        [TestMethod]
        public void CriticalSuccessHitsAndAmplifiesDamage()
        {
            CharacterStats attackerStats = BuildStats(1, 5, 5, 5, 5, SkillType.Melee, 0);
            CharacterStats defenderStats = BuildStats(5, 5, 5, 5, 5);
            Weapon weapon = new("Dagger", "Dagger", ItemRarity.Common, 10, 1f, 2, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line);
            TestCombatant attacker = new("Attacker", attackerStats, 0, 0, weapon);
            TestCombatant defender = new("Defender", defenderStats, 0, 10, weapon);

            bool hit = CombatResolver.ResolveAttack(attacker, defender, weapon, new FixedRandom(1));

            Assert.IsTrue(hit);
            Assert.AreEqual(5, defender.DamageTaken);
        }

        [TestMethod]
        public void EvasionReducesHitChance()
        {
            CharacterStats attackerStats = BuildStats(5, 5, 5, 5, 5, SkillType.Melee, 0);
            Weapon weapon = new("Club", "Club", ItemRarity.Common, 10, 1f, 3, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line);
            TestCombatant attacker = new("Attacker", attackerStats, 0, 0, weapon);
            TestCombatant defenderLow = new("DefenderLow", BuildStats(5, 5, 5, 5, 5), 0, 0, weapon);
            TestCombatant defenderHigh = new("DefenderHigh", BuildStats(5, 5, 5, 5, 5), 0, 5, weapon);

            bool hitLow = CombatResolver.ResolveAttack(attacker, defenderLow, weapon, new FixedRandom(6));
            bool hitHigh = CombatResolver.ResolveAttack(attacker, defenderHigh, weapon, new FixedRandom(6));

            Assert.IsTrue(hitLow);
            Assert.IsFalse(hitHigh);
        }
    }
}
