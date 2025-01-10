using System;
using System.Collections.Generic;

namespace HHSGame.Core
{
    public class EnemyFactory
    {
        private readonly Random _random;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly CollisionSystem _collisionSystem;
        private readonly MapState _mapState;

        public EnemyFactory(int mapWidth, int mapHeight, Random random, CollisionSystem collisionSystem, MapState mapState)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _random = random;
            _collisionSystem = collisionSystem;
            _mapState = mapState;
        }

        public List<Enemy> SpawnEnemies()
        {
            EventSystem.RaiseEvent("Spawning enemies...");
            
            var enemies = new List<Enemy>();
            
            // Spawn different enemy types
            SpawnEnemyType(enemies, EnemyType.Goblin, 100);
            SpawnEnemyType(enemies, EnemyType.Orc, 30);
            SpawnEnemyType(enemies, EnemyType.Troll, 1);
            
            EventSystem.RaiseEvent("Enemies spawned: 3 Goblins, 2 Orcs, 1 Troll");
            return enemies;
        }

        private void SpawnEnemyType(List<Enemy> enemies, EnemyType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x, y;
                do
                {
                    x = _random.Next(1, _mapWidth - 1);
                    y = _random.Next(1, _mapHeight - 1);
                } while (!_mapState.IsWalkable(x, y) || GetEnemyAt(enemies, x, y) != null);
                
                enemies.Add(new Enemy(type, x, y, _collisionSystem, _mapState.Pathfinder));
            }
        }

        private Enemy? GetEnemyAt(List<Enemy> enemies, int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }
    }
}
