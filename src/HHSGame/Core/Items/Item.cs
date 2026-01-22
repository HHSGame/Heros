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
        int damage, int penetration, int range, WeaponType weaponType) : Item(id, name, rarity, value, weight)
    {
        public int Damage { get; } = damage;
        public int Penetration { get; } = penetration;
        public int Range { get; } = range;
        public WeaponType WeaponType { get; } = weaponType;

        public int ApCost => WeaponRules.GetApCost(WeaponType);

        public override void Use(Player player)
        {
            player.EquipWeapon(this);
        }
    }

    public class Armor(string id, string name, ItemRarity rarity, int value, float weight,
        int armorValue) : Item(id, name, rarity, value, weight)
    {
        public int ArmorValue { get; } = armorValue;

        public override void Use(Player player)
        {
            player.EquipArmor(this);
        }
    }
}
