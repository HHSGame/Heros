using HHSGame.Core.Items;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Combat
{
    public interface ICombatant
    {
        string Name { get; }
        CharacterStats Stats { get; }
        int ArmorValue { get; }
        int EvasionBonus { get; }
        Weapon EquippedWeapon { get; }
        void TakeDamage(int damage);
    }
}
