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
            int target = attributeValue + skillValue - defender.EvasionBonus;

            int roll = random.Next(1, 21);
            bool criticalSuccess = roll == 1;
            bool criticalFailure = roll == 20;
            bool hit = (roll <= target && !criticalFailure) || criticalSuccess;

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
            int damage = Math.Max(1, rawDamage - effectiveArmor);

            if (criticalSuccess)
            {
                damage = (int)Math.Ceiling(damage * 1.5);
            }

            defender.TakeDamage(damage);

            Events.RaiseGameMessage(I18n.T("HHS.Core.Combat.Hit", attacker.Name, defender.Name, damage, roll, target));
            return true;
        }
    }
}
