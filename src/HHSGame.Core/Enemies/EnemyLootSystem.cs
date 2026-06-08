using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;

namespace HHSGame.Core.Enemies
{
    public class EnemyLootSystem(IReadOnlyList<EnemyLootEntry> lootEntries, ItemCatalog itemCatalog, Random random)
    {
        private readonly Random random = random;
        private readonly IReadOnlyList<EnemyLootEntry> lootEntries = lootEntries;
        private readonly ItemCatalog itemCatalog = itemCatalog;

        public List<Item> GenerateLoot()
        {
            List<Item> loot = [];
            foreach (EnemyLootEntry entry in lootEntries)
            {
                if (entry.Chance <= 0)
                {
                    continue;
                }

                int roll = random.Next(100);
                if (roll >= entry.Chance)
                {
                    continue;
                }

                int quantity = Math.Max(1, entry.Quantity);
                for (int i = 0; i < quantity; i++)
                {
                    if (itemCatalog.TryCreateItem(entry.ItemId, entry.Amount, out Item item))
                    {
                        loot.Add(item);
                    }
                }
            }

            return loot;
        }
    }
}
