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

        public Weapon Weapon { get; set; } = new Weapon("Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight);
        public Armor Armor { get; set; } = new Armor("Unknown", ItemRarity.Common, 0, 0, 0);

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

    public class Weapons
    {
        public static readonly Weapon UnknownWeapon = new("Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight);
        public static readonly Weapon Sword = new("Sword", ItemRarity.Common, 100, 2.0f, 10, 1, 1, WeaponType.MeleeLight);
        public static readonly Weapon Pickaxe = new("Pickaxe", ItemRarity.Common, 120, 2.5f, 8, 1, 1, WeaponType.MeleeHeavy);
        public static readonly Weapon Cleaver = new("Cleaver", ItemRarity.Common, 90, 1.8f, 7, 0, 1, WeaponType.MeleeLight);
        public static readonly Weapon Hammer = new("Hammer", ItemRarity.Common, 110, 3.0f, 9, 2, 1, WeaponType.MeleeHeavy);
        public static readonly Weapon Axe = new("Axe", ItemRarity.Common, 100, 2.0f, 8, 1, 1, WeaponType.MeleeHeavy);
        public static readonly Weapon Bow = new("Bow", ItemRarity.Common, 90, 1.5f, 7, 0, 6, WeaponType.RangedAimed);
        public static readonly Weapon Dagger = new("Dagger", ItemRarity.Common, 60, 0.5f, 5, 0, 1, WeaponType.MeleeLight);
        public static readonly Weapon Potion = new("Potion", ItemRarity.Common, 70, 0.7f, 4, 0, 3, WeaponType.RangedSnap);

    }

    public class Armors
    {
        // Armor constants
        public static readonly Armor UnknownArmor = new("Unknown", ItemRarity.Common, 0, 0, 0);
        public static readonly Armor Shield = new("Shield", ItemRarity.Common, 150, 3.0f, 5);
        public static readonly Armor Helmet = new("Helmet", ItemRarity.Common, 100, 2.0f, 3);
        public static readonly Armor LeatherArmor = new("Leather Armor", ItemRarity.Common, 130, 1.8f, 3);
        public static readonly Armor Robe = new("Robe", ItemRarity.Common, 70, 0.5f, 1);
        public static readonly Armor Cloak = new("Cloak", ItemRarity.Common, 80, 0.6f, 2);
    }

    public class Classes
    {
        public static readonly ClassConfig Unemployed = new(
            "Unemployed",
            new Attributes { Strength = 5, Perception = 5, Agility = 5, Charisma = 5, Intelligence = 5 },
            new Skills(),
            Weapons.UnknownWeapon,
            Armors.UnknownArmor);

        public static readonly ClassConfig Warrior = new(
            "Warrior",
            new Attributes { Strength = 7, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
            new Skills(),
            Weapons.Sword,
            Armors.Shield);

        public static readonly ClassConfig Thief = new(
            "Thief",
            new Attributes { Strength = 4, Perception = 5, Agility = 7, Charisma = 5, Intelligence = 4 },
            new Skills(),
            Weapons.Dagger,
            Armors.Cloak);

        public static readonly ClassConfig Alchemist = new(
            "Alchemist",
            new Attributes { Strength = 4, Perception = 5, Agility = 5, Charisma = 5, Intelligence = 7 },
            new Skills(),
            Weapons.Potion,
            Armors.Robe);
    }
}
