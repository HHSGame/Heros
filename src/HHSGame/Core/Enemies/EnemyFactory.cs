using System.IO;
using HHSGame.Core;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;
using HHSGame.Core.Map;


namespace HHSGame.Core.Enemies
{
    public class EnemyFactory(EnemyCatalog enemyCatalog, ItemCatalog itemCatalog, CollisionSystem collisionSystem, MapState mapState, Pathfinder pathfinder)
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

                    enemies.Add(CreateEnemy(spawn.EnemyId, position.X, position.Y));
                }
            }

            return enemies;
        }

        public Enemy CreateEnemy(string enemyId, int x, int y)
        {
            EnemyDefinition definition = enemyCatalog.GetDefinition(enemyId);
            return new Enemy(x, y, collisionSystem, pathfinder, definition, itemCatalog);
        }

        private static Enemy? GetEnemyAt(List<Enemy> enemies, int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }
    }
}
