using HHSGame.Core.Map;


namespace HHSGame.Core.Enemies
{
    public class EnemyFactory(Random random, CollisionSystem collisionSystem, MapState mapState, Pathfinder pathfinder)
    {
        public List<Enemy> SpawnEnemies()
        {
            EventSystem.RaiseGameMessage("Spawning enemies...");

            var enemies = new List<Enemy>();

            // Spawn different enemy types
            SpawnEnemyType(enemies, EnemyType.Gangster, 100);
            SpawnEnemyType(enemies, EnemyType.Bandit, 30);
            SpawnEnemyType(enemies, EnemyType.BanditLeader, 1);

            EventSystem.RaiseGameMessage("Enemies spawned: 3 Gangsters, 2 Bandits, 1 Bandit Leader");
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

                enemies.Add(new Enemy(type, x, y, collisionSystem, pathfinder));
            }
        }

        private static Enemy? GetEnemyAt(List<Enemy> enemies, int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }
    }
}
