using System;
using System.Collections.Generic;

namespace RpgGame.Core
{
    public class EnemyFactory
    {
        private readonly Random _random;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly GameWorld _world;

        public EnemyFactory(int mapWidth, int mapHeight, Random random, GameWorld world)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _random = random;
            _world = world;
        }

        public List<Enemy> SpawnEnemies()
        {
            EventSystem.RaiseEvent("Spawning enemies...");
            
            var enemies = new List<Enemy>();
            
            // Spawn different enemy types
            SpawnEnemyType(enemies, EnemyType.Goblin, 3);
            SpawnEnemyType(enemies, EnemyType.Orc, 2);
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
                } while (!IsWalkable(x, y) || GetEnemyAt(enemies, x, y) != null);
                
                enemies.Add(new Enemy(type, x, y, _world));
            }
        }

        private bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _mapWidth || y >= _mapHeight)
                return false;

            return true; // Map validation will be handled by GameWorld
        }

        private Enemy? GetEnemyAt(List<Enemy> enemies, int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }
    }
}
