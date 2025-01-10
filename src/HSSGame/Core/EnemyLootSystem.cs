namespace HHSGame.Core
{
    public class EnemyLootSystem
    {
        private readonly Random _random;
        private readonly EnemyType _enemyType;

        private static readonly Dictionary<EnemyType, (int Chance, int Amount)> _lootTable = new()
        {
            { EnemyType.Goblin, (100, 10) },
            { EnemyType.Orc, (50, 20) },
            { EnemyType.Troll, (70, 30) }
        };

        public EnemyLootSystem(EnemyType enemyType, Random random)
        {
            _enemyType = enemyType;
            _random = random;
        }

        public List<Item> GenerateLoot()
        {
            var loot = new List<Item>();
            var roll = _random.Next(100);
            
            if (_lootTable.TryGetValue(_enemyType, out var lootData) && roll < lootData.Chance)
            {
                loot.Add(new HealthPotion(lootData.Amount));
            }
            
            return loot;
        }
    }
}
