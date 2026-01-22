using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Classes
{
    public abstract class AbstractClass
    {
        public string Name { get; set; } = "Unknown";
        public Attributes Attributes { get; set; } = new();
        public Skills Skills { get; set; } = new();
        public abstract void ApplyClassBonuses(Player player);
        public abstract void ApplyStartupEquipment(Player player);

        public virtual void ApplySecretSkill(Player player, Enemy enemy) { }
    }

    public class TypedClass : AbstractClass
    {

        public Weapon Weapon { get; set; } = new Weapon("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight);
        public Armor Armor { get; set; } = new Armor("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0);

        public override void ApplyClassBonuses(Player player)
        {
            player.ApplyBaseStats(Attributes, Skills);
        }
        public override void ApplyStartupEquipment(Player player)
        {
            player.EquipWeapon(Weapon);
            player.EquipArmor(Armor);
        }
    }

    public record ClassConfig(string Name, Attributes Attributes, Skills Skills, Weapon startupWeapon, Armor startupArmor)
    {

        public TypedClass ToClass()
        {
            return new TypedClass()
            {
                Name = Name,
                Attributes = Attributes,
                Skills = Skills,
                Weapon = startupWeapon,
                Armor = startupArmor
            };
        }
    }

}
