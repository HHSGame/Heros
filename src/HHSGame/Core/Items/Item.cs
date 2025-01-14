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

    public abstract class Item
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; protected set; }
        public ItemRarity Rarity { get; protected set; }
        public int Value { get; protected set; }
        public float Weight { get; protected set; }

        public Item(string name, ItemRarity rarity, int value, float weight)
        {
            Name = name;
            Rarity = rarity;
            Value = value;
            Weight = weight;
        }

        public abstract void Use(Player player);
    }

    public class HealthPotion : Item
    {
        private int _healAmount;

        public HealthPotion(int healAmount)
            : base("Health Potion", ItemRarity.Common, 50, 0.5f)
        {
            _healAmount = healAmount;
        }

        public override void Use(Player player)
        {
            player.Heal(_healAmount);
        }
    }

    public class Weapon : Item
    {
        public int Damage { get; private set; }
        public float AttackSpeed { get; private set; }

        public Weapon(string name, ItemRarity rarity, int value, float weight,
            int damage, float attackSpeed)
            : base(name, rarity, value, weight)
        {
            Damage = damage;
            AttackSpeed = attackSpeed;
        }

        public override void Use(Player player)
        {
            player.EquipWeapon(this);
        }
    }

    public class Armor : Item
    {
        public int Defense { get; private set; }

        public Armor(string name, ItemRarity rarity, int value, float weight,
            int defense)
            : base(name, rarity, value, weight)
        {
            Defense = defense;
        }

        public override void Use(Player player)
        {
            player.EquipArmor(this);
        }
    }
}
