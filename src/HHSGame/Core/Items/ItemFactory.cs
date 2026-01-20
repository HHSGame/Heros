
namespace HHSGame.Core.Items
{

    public class ItemFactory(Random random)
    {

        public Item CreateRandomItem()
        {
            int itemType = random.Next(3);
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
            double chance = random.NextDouble();
            return chance switch
            {
                < 0.5 => ItemRarity.Common,
                < 0.8 => ItemRarity.Uncommon,
                < 0.95 => ItemRarity.Rare,
                < 0.99 => ItemRarity.Epic,
                _ => ItemRarity.Legendary
            };
        }

        private HealthPotion CreateRandomPotion(ItemRarity rarity)
        {
            int healAmount = rarity switch
            {
                ItemRarity.Common => random.Next(10, 20),
                ItemRarity.Uncommon => random.Next(20, 40),
                ItemRarity.Rare => random.Next(40, 60),
                ItemRarity.Epic => random.Next(60, 80),
                ItemRarity.Legendary => random.Next(80, 100),
                _ => throw new ArgumentOutOfRangeException(nameof(rarity), "Invalid rarity value")
            };

            return new HealthPotion(healAmount);
        }

        private Weapon CreateRandomWeapon(ItemRarity rarity)
        {
            (string Name, WeaponType Type)[] weapons =
            [
                ("Sword", WeaponType.MeleeLight),
                ("Axe", WeaponType.MeleeHeavy),
                ("Mace", WeaponType.MeleeHeavy),
                ("Dagger", WeaponType.MeleeLight),
                ("Rifle", WeaponType.RangedAimed),
                ("SMG", WeaponType.RangedSnap)
            ];
            (string baseName, WeaponType weaponType) = weapons[random.Next(weapons.Length)];
            string name = $"{rarity} {baseName}";

            int damage = rarity switch
            {
                ItemRarity.Common => random.Next(5, 10),
                ItemRarity.Uncommon => random.Next(10, 20),
                ItemRarity.Rare => random.Next(20, 30),
                ItemRarity.Epic => random.Next(30, 40),
                ItemRarity.Legendary => random.Next(40, 50),
                _ => throw new ArgumentOutOfRangeException(nameof(rarity), "Invalid rarity value")
            };

            int penetration = rarity switch
            {
                ItemRarity.Common => 0,
                ItemRarity.Uncommon => 1,
                ItemRarity.Rare => 2,
                ItemRarity.Epic => 3,
                ItemRarity.Legendary => 4,
                _ => 0
            };

            int range = WeaponRules.IsRanged(weaponType) ? random.Next(6, 12) : 1;
            int value = damage * 10;
            float weight = (float)random.NextDouble() * 5 + 1;

            return new Weapon(name, rarity, value, weight, damage, penetration, range, weaponType);
        }

        private Armor CreateRandomArmor(ItemRarity rarity)
        {
            string[] armorNames = { "Helmet", "Chestplate", "Leggings", "Boots", "Shield" };
            string name = $"{rarity} {armorNames[random.Next(armorNames.Length)]}";

            int armorValue = rarity switch
            {
                ItemRarity.Common => random.Next(1, 3),
                ItemRarity.Uncommon => random.Next(3, 6),
                ItemRarity.Rare => random.Next(6, 10),
                ItemRarity.Epic => random.Next(10, 15),
                ItemRarity.Legendary => random.Next(15, 20),
                _ => throw new ArgumentOutOfRangeException(nameof(rarity), "Invalid rarity value")
            };

            int value = armorValue * 15;
            float weight = (float)random.NextDouble() * 10 + 2;

            return new Armor(name, rarity, value, weight, armorValue);
        }
    }
}
