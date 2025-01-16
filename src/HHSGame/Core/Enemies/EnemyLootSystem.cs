namespace HHSGame.Core.Enemies
{
    using Items;

    public class EnemyLootSystem(EnemyType enemyType, Random random)
    {
        private readonly Random random = random;
        private readonly EnemyType enemyType = enemyType;

        private static readonly Dictionary<EnemyType, (int Chance, int Amount)> lootTable = new()
        {
            { EnemyType.Gangster, (100, 10) },
            { EnemyType.Bandit, (50, 20) },
            { EnemyType.BanditLeader, (70, 30) }
        };

        public List<Item> GenerateLoot()
        {
            var loot = new List<Item>();
            var roll = random.Next(100);

            if (lootTable.TryGetValue(enemyType, out var lootData) && roll < lootData.Chance)
            {
                loot.Add(new HealthPotion(lootData.Amount));
            }

            return loot;
        }
    }
}
