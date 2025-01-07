using RpgGame.UI;
using System.Collections.Generic;

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
        private MapGenerator _mapGenerator;
        private EnemyFactory _enemyFactory;

        private TurnState _currentTurn = TurnState.PlayerTurn;
        
        public TurnState CurrentTurn => _currentTurn;

        public GameWorld()
        {
            _map = new char[MapHeight, MapWidth];
            _enemies = new List<Enemy>();
            _loot = new List<Item>();
            _random = new Random();
            
            _mapGenerator = new MapGenerator(MapWidth, MapHeight, _random);
            _enemyFactory = new EnemyFactory(MapWidth, MapHeight, _random, this);
            
            _map = _mapGenerator.GenerateDungeon();
            _enemies = _enemyFactory.SpawnEnemies();

            EventSystem.OnTurnChanged += HandleTurnChange;
        }

        private void HandleTurnChange(object? sender, TurnEvent e)
        {
            _currentTurn = e.State;
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
}
