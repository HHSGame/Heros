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

    public abstract class Item(string name, ItemRarity rarity, int value, float weight)
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; } = name;
        public ItemRarity Rarity { get; } = rarity;
        public int Value { get; } = value;
        public float Weight { get; } = weight;

        public abstract void Use(Player player);


        public override string ToString()
        {
            return $"{Name} - {Rarity} [{Value}] {Weight}";
        }
    }

    public class HealthPotion(int healAmount) : Item("Health Potion", ItemRarity.Common, 50, 0.5f)
    {
        public override void Use(Player player)
        {
            player.Heal(healAmount);
        }
    }

    public class Weapon(string name, ItemRarity rarity, int value, float weight,
        int damage, float attackSpeed) : Item(name, rarity, value, weight)
    {
        public int Damage { get; } = damage;
        public float AttackSpeed { get; } = attackSpeed;

        public override void Use(Player player)
        {
            player.EquipWeapon(this);
        }
    }

    public class Armor(string name, ItemRarity rarity, int value, float weight,
        int defense) : Item(name, rarity, value, weight)
    {
        public int Defense { get; } = defense;

        public override void Use(Player player)
        {
            player.EquipArmor(this);
        }
    }
}
