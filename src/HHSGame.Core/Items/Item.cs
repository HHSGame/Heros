using HHSGame.Core.Stats;

namespace HHSGame.Core.Items
{

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public abstract class Item(string id, string name, ItemRarity rarity, int value, float weight)
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Id { get; } = id;
        public string Name { get; } = name;
        public ItemRarity Rarity { get; } = rarity;
        public int Value { get; } = value;
        public float Weight { get; } = weight;
        public bool Consumable { get; set; }
        public bool IsHidden { get; set; }

        public List<AttributeModifier> AttributeModifiers { get; set; } = [];

        public List<SkillModifier> SkillModifiers { get; set; } = [];


        public abstract void Use(Player player);


        public override string ToString()
        {
            return $"{Name} - {Rarity} [{Value}] {Weight}";
        }
    }

    public enum WeaponType
    {
        MeleeLight,
        MeleeHeavy,
        RangedSnap,
        RangedAimed,
        Burst
    }

    public enum WeaponTrajectory
    {
        Line,
        Arc
    }

    public static class WeaponRules
    {
        public static int GetApCost(WeaponType type)
        {
            return type switch
            {
                WeaponType.MeleeLight => 3,
                WeaponType.MeleeHeavy => 5,
                WeaponType.RangedSnap => 3,
                WeaponType.RangedAimed => 5,
                WeaponType.Burst => 6,
                _ => 3
            };
        }

        public static SkillType GetSkill(WeaponType type)
        {
            return type switch
            {
                WeaponType.MeleeLight => SkillType.Melee,
                WeaponType.MeleeHeavy => SkillType.Melee,
                WeaponType.RangedSnap => SkillType.Firearms,
                WeaponType.RangedAimed => SkillType.Firearms,
                WeaponType.Burst => SkillType.Firearms,
                _ => SkillType.Melee
            };
        }

        public static AttributeType GetAttribute(WeaponType type)
        {
            return type switch
            {
                WeaponType.MeleeLight => AttributeType.Strength,
                WeaponType.MeleeHeavy => AttributeType.Strength,
                WeaponType.RangedSnap => AttributeType.Perception,
                WeaponType.RangedAimed => AttributeType.Perception,
                WeaponType.Burst => AttributeType.Perception,
                _ => AttributeType.Strength
            };
        }

        public static bool IsRanged(WeaponType type)
        {
            return type is WeaponType.RangedSnap or WeaponType.RangedAimed or WeaponType.Burst;
        }

        public static bool RequiresLineOfSight(Weapon weapon)
        {
            return weapon.Trajectory == WeaponTrajectory.Line && IsRanged(weapon.WeaponType);
        }
    }

    public class HealthPotion : Item
    {
        private readonly int healAmount;

        public HealthPotion(string id, string name, ItemRarity rarity, int value, float weight, int healAmount)
            : base(id, name, rarity, value, weight)
        {
            Consumable = true;
            this.healAmount = healAmount;
        }

        public override void Use(Player player)
        {
            player.Heal(healAmount);
        }
    }

    public class Weapon(string id, string name, ItemRarity rarity, int value, float weight,
        int damage, int penetration, int range, WeaponType weaponType, WeaponTrajectory trajectory) : Item(id, name, rarity, value, weight)
    {
        public int Damage { get; } = damage;
        public int Penetration { get; } = penetration;
        public int Range { get; } = range;
        public WeaponType WeaponType { get; } = weaponType;
        public WeaponTrajectory Trajectory { get; } = trajectory;

        public int ApCost => WeaponRules.GetApCost(WeaponType);

        public override void Use(Player player)
        {
            player.EquipWeapon(this);
        }
    }

    public enum EquipmentSlot
    {
        Weapon,
        Head,
        Body,
        Legs,
        Accessory1,
        Accessory2
    }

    public static class EquipmentSlotRules
    {
        public static string GetDisplayName(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => "Weapon",
                EquipmentSlot.Head => "Head",
                EquipmentSlot.Body => "Body",
                EquipmentSlot.Legs => "Legs",
                EquipmentSlot.Accessory1 => "Accessory 1",
                EquipmentSlot.Accessory2 => "Accessory 2",
                _ => "Unknown"
            };
        }

        public static EquipmentSlot? ParseSlot(string slotName)
        {
            return slotName?.ToLowerInvariant() switch
            {
                "head" => EquipmentSlot.Head,
                "body" => EquipmentSlot.Body,
                "legs" => EquipmentSlot.Legs,
                "accessory" or "accessory1" => EquipmentSlot.Accessory1,
                "accessory2" => EquipmentSlot.Accessory2,
                _ => null
            };
        }

        public static int GetTotalArmor(IReadOnlyDictionary<EquipmentSlot, Armor> equipped)
        {
            int total = 0;
            foreach (Armor armor in equipped.Values)
            {
                total += armor.ArmorValue;
            }
            return total;
        }
    }

    public class Armor(string id, string name, ItemRarity rarity, int value, float weight,
        int armorValue, EquipmentSlot slot = EquipmentSlot.Body) : Item(id, name, rarity, value, weight)
    {
        public int ArmorValue { get; } = armorValue;
        public EquipmentSlot Slot { get; } = slot;

        public override void Use(Player player)
        {
            player.EquipArmor(this);
        }
    }
}
