using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace RpgGame
{
    public class GameWorld
    {
        public static int MapWidth { get; } = 80;
        public static int MapHeight { get; } = 24;
        
        private char[,] _map;
        private List<Enemy> _enemies;
        private List<Item> _loot;
        private Random _random;

        private TurnState? _currentTurn;

        public GameWorld()
        {
            _map = new char[MapHeight, MapWidth];
            _enemies = new List<Enemy>();
            _loot = new List<Item>();
            _random = new Random();
            GenerateDungeon();
            SpawnEnemies();

            EventSystem.OnTurnChanged += HandleTurnChange;

        }

        private void HandleTurnChange(object? sender, TurnEvent e)
        {
            _currentTurn = e.State;
        }

        private void GenerateDungeon()
        {
            EventSystem.RaiseEvent("Generating dungeon...");
            
            // Generate walls and floors
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1)
                    {
                        _map[y, x] = '#'; // Walls
                    }
                    else
                    {
                        _map[y, x] = '.'; // Floor
                    }
                }
            }

            // Add some rooms
            AddRoom(5, 5, 10, 6);
            AddRoom(20, 15, 8, 8);
            AddRoom(40, 10, 12, 10);
            
            EventSystem.RaiseEvent("Dungeon generated with 3 main chambers");
        }

        private void AddRoom(int x, int y, int width, int height)
        {
            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    if (i >= 0 && i < MapHeight && j >= 0 && j < MapWidth)
                    {
                        _map[i, j] = '.';
                    }
                }
            }
        }

        private void SpawnEnemies()
        {
            EventSystem.RaiseEvent("Spawning enemies...");
            
            // Spawn different enemy types
            SpawnEnemyType(EnemyType.Goblin, 3);
            SpawnEnemyType(EnemyType.Orc, 2);
            SpawnEnemyType(EnemyType.Troll, 1);
            
            EventSystem.RaiseEvent("Enemies spawned: 3 Goblins, 2 Orcs, 1 Troll");
        }

        private void SpawnEnemyType(EnemyType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x, y;
                do
                {
                    x = _random.Next(1, MapWidth - 1);
                    y = _random.Next(1, MapHeight - 1);
                } while (!IsWalkable(x, y) || GetEnemyAt(x, y) != null);
                
                _enemies.Add(new Enemy(type, x, y));
            }
        }

        public void Update(Player player)
        {
            // Update all enemies
            foreach (var enemy in _enemies.ToArray())
            {
                if (enemy.Health <= 0)
                {
                    // Handle enemy death
                    _enemies.Remove(enemy);
                    DropLoot(enemy);
                    continue;
                }

                enemy.Update(player, _currentTurn == TurnState.EnemyTurn);

                EndEnemyTurn();
            }

            // Check for loot collection
            var lootAtPlayer = _loot.FirstOrDefault(l => l.X == player.X && l.Y == player.Y);
            if (lootAtPlayer != null)
            {
                player.AddItem(lootAtPlayer);
                _loot.Remove(lootAtPlayer);
            }
        }

        private void EndEnemyTurn()
        {
            if (this._currentTurn == TurnState.EnemyTurn) {
                EventSystem.RaiseTurnChanged(TurnState.PlayerTurn);
            }
        }

        private void DropLoot(Enemy enemy)
        {
            var loot = enemy.GenerateLoot();
            foreach (var item in loot)
            {
                item.X = enemy.X;
                item.Y = enemy.Y;
                _loot.Add(item);
            }
        }

        public void Draw(char[,] buffer)
        {
            // Draw map tiles
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    buffer[y, x] = _map[y, x];
                }
            }

            // Draw enemies
            foreach (var enemy in _enemies)
            {
                if (enemy.X >= 0 && enemy.X < MapWidth &&
                    enemy.Y >= 0 && enemy.Y < MapHeight)
                {
                    enemy.Draw(buffer);
                }
            }

            // Draw loot
            foreach (var item in _loot)
            {
                if (item.X >= 0 && item.X < MapWidth &&
                    item.Y >= 0 && item.Y < MapHeight)
                {
                    buffer[item.Y, item.X] = '*';
                }
            }
        }

        public void Clear(View view)
        {
            // Clear the entire map area
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    view.AddRune(x, y, ' ');
                }
            }
        }

        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight)
                return false;

            return _map[y, x] == '.';
        }

        public Enemy? GetEnemyAt(int x, int y)
        {
            return _enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }

        public List<Enemy> GetEnemiesInRange(int x, int y, int range)
        {
            return _enemies.Where(e => 
                Math.Abs(e.X - x) <= range && 
                Math.Abs(e.Y - y) <= range).ToList();
        }
    }

    public abstract class Item
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; protected set; }
        
        public Item(string name)
        {
            Name = name;
        }

        public abstract void Use(Player player);
    }

    public class HealthPotion : Item
    {
        private int _healAmount;

        public HealthPotion(int healAmount) : base("Health Potion")
        {
            _healAmount = healAmount;
        }

        public override void Use(Player player)
        {
            player.Heal(_healAmount);
        }
    }
}
