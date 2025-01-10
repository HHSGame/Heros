
namespace HHSGame.Core {

    public class ItemFactory(Random _random)
    {
        
        public Item CreateRandomItem()
        {
            int itemType = _random.Next(3);
            ItemRarity rarity = GetRandomRarity();
            
            return itemType switch
            {
                0 => CreateRandomPotion(rarity),
                1 => CreateRandomWeapon(rarity),
                2 => CreateRandomArmor(rarity),
                _ => throw new InvalidOperationException("Invalid item type")
            };
        }

        private ItemRarity GetRandomRarity()
        {
            double chance = _random.NextDouble();
            return chance switch
            {
                < 0.5 => ItemRarity.Common,
                < 0.8 => ItemRarity.Uncommon,
                < 0.95 => ItemRarity.Rare,
                < 0.99 => ItemRarity.Epic,
                _ => ItemRarity.Legendary
            };
        }

        private Item CreateRandomPotion(ItemRarity rarity)
        {
            int healAmount = rarity switch
            {
                ItemRarity.Common => _random.Next(10, 20),
                ItemRarity.Uncommon => _random.Next(20, 40),
                ItemRarity.Rare => _random.Next(40, 60),
                ItemRarity.Epic => _random.Next(60, 80),
                ItemRarity.Legendary => _random.Next(80, 100),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new HealthPotion(healAmount);
        }

        private Item CreateRandomWeapon(ItemRarity rarity)
        {
            string[] weaponNames = { "Sword", "Axe", "Mace", "Dagger", "Spear" };
            string name = $"{rarity} {weaponNames[_random.Next(weaponNames.Length)]}";
            
            int damage = rarity switch
            {
                ItemRarity.Common => _random.Next(5, 10),
                ItemRarity.Uncommon => _random.Next(10, 20),
                ItemRarity.Rare => _random.Next(20, 30),
                ItemRarity.Epic => _random.Next(30, 40),
                ItemRarity.Legendary => _random.Next(40, 50),
                _ => throw new ArgumentOutOfRangeException()
            };

            float attackSpeed = (float)_random.NextDouble() * 2 + 0.5f;
            int value = damage * 10;
            float weight = (float)_random.NextDouble() * 5 + 1;

            return new Weapon(name, rarity, value, weight, damage, attackSpeed);
        }

        private Item CreateRandomArmor(ItemRarity rarity)
        {
            string[] armorNames = { "Helmet", "Chestplate", "Leggings", "Boots", "Shield" };
            string name = $"{rarity} {armorNames[_random.Next(armorNames.Length)]}";
            
            int defense = rarity switch
            {
                ItemRarity.Common => _random.Next(1, 3),
                ItemRarity.Uncommon => _random.Next(3, 6),
                ItemRarity.Rare => _random.Next(6, 10),
                ItemRarity.Epic => _random.Next(10, 15),
                ItemRarity.Legendary => _random.Next(15, 20),
                _ => throw new ArgumentOutOfRangeException()
            };

            int value = defense * 15;
            float weight = (float)_random.NextDouble() * 10 + 2;

            return new Armor(name, rarity, value, weight, defense);
        }
    }
}