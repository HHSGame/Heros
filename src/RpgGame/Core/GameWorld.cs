using RpgGame.UI;

namespace RpgGame.Core
{
    public class GameWorld : IDrawable
    {
        public static int MapWidth { get; } = 150;
        public static int MapHeight { get; } = 30;
        
        private char[,] _map;
        private List<Enemy> _enemies;
        private List<Item> _loot;
        private Random _random;

        private TurnState _currentTurn = TurnState.PlayerTurn;
        
        public TurnState CurrentTurn => _currentTurn;

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
            EventSystem.RaiseEvent("Generating dungeon using cellular automata...");
            
            // Initialize random map
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    _map[y, x] = _random.Next(100) < 45 ? '#' : '.';
                }
            }

            // Apply cellular automata rules
            for (int i = 0; i < 5; i++)
            {
                SmoothMap();
            }

            // Ensure border walls
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1)
                    {
                        _map[y, x] = '#';
                    }
                }
            }

            // Add special terrain
            AddTerrainFeatures();
            
            EventSystem.RaiseEvent("Dungeon generated with natural cave-like structure");
        }

        private void SmoothMap()
        {
            char[,] newMap = new char[MapHeight, MapWidth];
            
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    int neighborWallTiles = GetSurroundingWallCount(x, y);

                    if (neighborWallTiles > 4)
                        newMap[y, x] = '#';
                    else if (neighborWallTiles < 4)
                        newMap[y, x] = '.';
                    else
                        newMap[y, x] = _map[y, x];
                }
            }
            
            _map = newMap;
        }

        private int GetSurroundingWallCount(int x, int y)
        {
            int wallCount = 0;
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    if (neighborX >= 0 && neighborX < MapWidth && 
                        neighborY >= 0 && neighborY < MapHeight)
                    {
                        if (neighborX != x || neighborY != y)
                        {
                            wallCount += _map[neighborY, neighborX] == '#' ? 1 : 0;
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
            return wallCount;
        }

        private void AddTerrainFeatures()
        {
            // Add water
            AddRandomFeature('~', 10, 5);
            // Add lava
            AddRandomFeature('^', 5, 3);
            // Add vegetation
            AddRandomFeature('*', 15, 7);
        }

        private void AddRandomFeature(char feature, int count, int maxSize)
        {
            for (int i = 0; i < count; i++)
            {
                int startX = _random.Next(1, MapWidth - 1);
                int startY = _random.Next(1, MapHeight - 1);
                
                if (_map[startY, startX] == '.')
                {
                    FloodFillFeature(startX, startY, feature, maxSize);
                }
            }
        }

        private void FloodFillFeature(int x, int y, char feature, int maxSize)
        {
            Queue<(int x, int y)> queue = new();
            queue.Enqueue((x, y));
            int filled = 0;

            while (queue.Count > 0 && filled < maxSize)
            {
                var (currentX, currentY) = queue.Dequeue();
                
                if (_map[currentY, currentX] == '.')
                {
                    _map[currentY, currentX] = feature;
                    filled++;
                    
                    // Add neighbors
                    if (currentX > 1) queue.Enqueue((currentX - 1, currentY));
                    if (currentX < MapWidth - 2) queue.Enqueue((currentX + 1, currentY));
                    if (currentY > 1) queue.Enqueue((currentX, currentY - 1));
                    if (currentY < MapHeight - 2) queue.Enqueue((currentX, currentY + 1));
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

            }
            EndEnemyTurn();

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
            if (_currentTurn == TurnState.EnemyTurn) {
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

        public void Draw(IDrawingContext ctx)
        {
            // Draw map tiles
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    ctx.DrawAt((x, y), _map[y, x]);
                }
            }

            // Draw enemies
            foreach (var enemy in _enemies)
            {
                if (enemy.X >= 0 && enemy.X < MapWidth &&
                    enemy.Y >= 0 && enemy.Y < MapHeight)
                {
                    enemy.Draw(ctx);
                }
            }

            // Draw loot
            foreach (var item in _loot)
            {
                if (item.X >= 0 && item.X < MapWidth &&
                    item.Y >= 0 && item.Y < MapHeight)
                {
                    ctx.DrawAt((item.X, item.Y), '*');
                }
            }
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
