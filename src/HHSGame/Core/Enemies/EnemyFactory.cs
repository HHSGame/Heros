using HHSGame.Core.Map;


namespace HHSGame.Core.Enemies
{
    public class EnemyFactory(Random random, CollisionSystem collisionSystem, MapState mapState, Pathfinder pathfinder)
    {
        public List<Enemy> SpawnEnemies()
        {
            Events.RaiseGameMessage("Spawning enemies...");

            List<Enemy> enemies = [];

            // Spawn different enemy types
            SpawnEnemyType(enemies, EnemyType.Gangster, 50);
            SpawnEnemyType(enemies, EnemyType.Bandit, 30);
            SpawnEnemyType(enemies, EnemyType.BanditLeader, 5);
            SpawnEnemyType(enemies, EnemyType.Thug, 100);
            SpawnEnemyType(enemies, EnemyType.Soldier, 60);
            SpawnEnemyType(enemies, EnemyType.Sniper, 5);

            Events.RaiseGameMessage("Enemies spawned: 50 Gangsters, 30 Bandits, 5 Bandit Leaders, 100 Thugs, 60 Soldiers, 5 Snipers");
            return enemies;
        }

        private void SpawnEnemyType(List<Enemy> enemies, EnemyType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x, y;
                do
                {
                    x = random.Next(1, mapState.Width - 1);
                    y = random.Next(1, mapState.Height - 1);
                } while (!mapState.IsWalkable(x, y) || GetEnemyAt(enemies, x, y) != null);

                EnemyRegistry.EnemyConfig config = EnemyRegistry.GetConfig(type);
                enemies.Add(new Enemy(type, x, y, collisionSystem, pathfinder, config));
            }
        }

        private static Enemy? GetEnemyAt(List<Enemy> enemies, int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }
    }
}
