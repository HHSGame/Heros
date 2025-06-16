namespace HHSGame.Core.Classes
{
    using Enemies;
    using Items;

    public abstract class AbstractClass
    {
        public string Name { get; set; } = "Unknown";
        public int BaseHealth { get; set; }
        public int BaseStrength { get; set; }
        public int BaseDefense { get; set; }
        public abstract void ApplyClassBonuses(Player player);
        public abstract void ApplyStartupEquipment(Player player);

        public virtual void ApplySecretSkill(Player player, Enemy enemy)
        {
            // Default implementation: no secret skill
        }
    }

    public class TypedClass : AbstractClass
    {

        public Weapon Weapon { get; set; } = new Weapon("Unknown", ItemRarity.Common, 0, 0, 0, 0);
        public Armor Armor { get; set; } = new Armor("Unknown", ItemRarity.Common, 0, 0, 0);

        public override void ApplyClassBonuses(Player player)
        {
            player.MaxHealth = BaseHealth;
            player.Health = BaseHealth;
            player.Strength = BaseStrength;
            player.Defense = BaseDefense;
        }
        public override void ApplyStartupEquipment(Player player)
        {
            player.EquipWeapon(Weapon);
            player.EquipArmor(Armor);
        }
    }

    public record ClassConfig(string Name, int BaseHealth, int BaseStrength, int BaseDefense, Weapon startupWeapon, Armor startupArmor)
    {

        public TypedClass ToClass()
        {
            return new TypedClass()
            {
                Name = Name,
                BaseHealth = BaseHealth,
                BaseStrength = BaseStrength,
                BaseDefense = BaseDefense,
                Weapon = startupWeapon,
                Armor = startupArmor
            };
        }
    }

    public class Weapons
    {
        public static readonly Weapon UnknownWeapon = new("Unknown", ItemRarity.Common, 0, 0, 0, 0);
        public static readonly Weapon Sword = new("Sword", ItemRarity.Common, 100, 2.0f, 10, 1.0f);
        public static readonly Weapon Pickaxe = new("Pickaxe", ItemRarity.Common, 120, 2.5f, 8, 1.2f);
        public static readonly Weapon Cleaver = new("Cleaver", ItemRarity.Common, 90, 1.8f, 7, 1.1f);
        public static readonly Weapon Hammer = new("Hammer", ItemRarity.Common, 110, 3.0f, 9, 1.3f);
        public static readonly Weapon Axe = new("Axe", ItemRarity.Common, 100, 2.0f, 8, 1.1f);
        public static readonly Weapon Bow = new("Bow", ItemRarity.Common, 90, 1.5f, 7, 1.2f);
        public static readonly Weapon Dagger = new("Dagger", ItemRarity.Common, 60, 0.5f, 5, 0.8f);
        public static readonly Weapon Potion = new("Potion", ItemRarity.Common, 70, 0.7f, 4, 0.9f);

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
        public static readonly ClassConfig Unemployed = new("Unemployed", 10, 10, 10, Weapons.UnknownWeapon, Armors.UnknownArmor);
        public static readonly ClassConfig Warrior = new("Warrior", 120, 15, 7, Weapons.Sword, Armors.Shield);
        public static readonly ClassConfig Thief = new("Thief", 80, 8, 4, Weapons.Dagger, Armors.Cloak);
        public static readonly ClassConfig Alchemist = new("Alchemist", 85, 7, 3, Weapons.Potion, Armors.Robe);
    }
}
