using System.IO;
using HHSGame.Core;
using HHSGame.Core.Map;


namespace HHSGame.Core.Enemies
{
    public class EnemyFactory(CollisionSystem collisionSystem, MapState mapState, Pathfinder pathfinder)
    {
        public List<Enemy> SpawnEnemies(IEnumerable<EnemySpawn> spawns)
        {
            List<Enemy> enemies = [];
            foreach (EnemySpawn spawn in spawns)
            {
                if (spawn.Count <= 0)
                {
                    continue;
                }

                for (int i = 0; i < spawn.Count; i++)
                {
                    Coordinate position = spawn.Position;
                    if (!mapState.IsWalkable(position))
                    {
                        throw new InvalidDataException($"Enemy spawn is not walkable at {position}.");
                    }

                    if (GetEnemyAt(enemies, position.X, position.Y) != null)
                    {
                        throw new InvalidDataException($"Duplicate enemy spawn at {position}.");
                    }

                    enemies.Add(CreateEnemy(spawn.Type, position.X, position.Y));
                }
            }

            return enemies;
        }

        public Enemy CreateEnemy(EnemyType type, int x, int y)
        {
            EnemyRegistry.EnemyConfig config = EnemyRegistry.GetConfig(type);
            return new Enemy(type, x, y, collisionSystem, pathfinder, config);
        }

        private static Enemy? GetEnemyAt(List<Enemy> enemies, int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }
    }
}
