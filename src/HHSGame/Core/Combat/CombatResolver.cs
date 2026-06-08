using HHSGame.Core.Items;
using HHSGame.Core.Stats;
using HHSGame.Utils;

namespace HHSGame.Core.Combat
{
    public static class CombatResolver
    {
        public static bool ResolveAttack(ICombatant attacker, ICombatant defender, Weapon weapon, Random random)
        {
            AttributeType attributeType = WeaponRules.GetAttribute(weapon.WeaponType);
            SkillType skillType = WeaponRules.GetSkill(weapon.WeaponType);

            int attributeValue = attacker.Stats.Attributes.Get(attributeType);
            int skillValue = attacker.Stats.Skills.GetValue(skillType, attacker.Stats.Attributes);
            int accuracyBonus = 0;
            if (attacker is Player player && WeaponRules.IsRanged(weapon.WeaponType))
            {
                accuracyBonus = player.RangedAccuracyBonus;
            }
            int target = attributeValue + skillValue + accuracyBonus - defender.EvasionBonus;

            int roll = random.Next(GameConstants.Combat.DiceMin, GameConstants.Combat.DiceMax + 1);
            bool criticalSuccess = roll == GameConstants.Combat.CriticalSuccessThreshold;
            bool criticalFailure = roll == GameConstants.Combat.CriticalFailureThreshold;
            bool hit = (roll <= target && !criticalFailure) || criticalSuccess;

            if (attacker is Player rangedPlayer && WeaponRules.IsRanged(weapon.WeaponType))
            {
                rangedPlayer.ConsumeRangedAccuracyBonus(weapon);
            }

            if (!hit)
            {
                Events.RaiseGameMessage(I18n.T("HHS.Core.Combat.Miss", attacker.Name, defender.Name, roll, target));
                return false;
            }

            int rawDamage = weapon.Damage;
            if (weapon.WeaponType is WeaponType.MeleeLight or WeaponType.MeleeHeavy)
            {
                rawDamage += attributeValue;
            }

            int effectiveArmor = Math.Max(0, defender.ArmorValue - weapon.Penetration);
            int damage = Math.Max(GameConstants.Combat.MinDamage, rawDamage - effectiveArmor);

            if (criticalSuccess)
            {
                damage = (int)Math.Ceiling(damage * GameConstants.Combat.CriticalDamageMultiplier);
            }

            defender.TakeDamage(damage);

            Events.RaiseGameMessage(I18n.T("HHS.Core.Combat.Hit", attacker.Name, defender.Name, damage, roll, target));
            return true;
        }
    }
}
