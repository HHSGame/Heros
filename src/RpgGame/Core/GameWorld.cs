using RpgGame.UI;

namespace RpgGame.Core
{
    public class GameWorld : IDrawable
    {
        public static int MapWidth { get; } = 500;
        public static int MapHeight { get; } = 500;
        
        private Cell[,] _map;
        private List<Enemy> _enemies;
        private List<Item> _loot;
        private readonly Random _random;
        private readonly MapGenerator _mapGenerator;
        private readonly EnemyFactory _enemyFactory;
        private readonly Pathfinder _pathfinder;
        private readonly CollisionSystem _collisionSystem;
        private readonly MapStyle _mapStyle;

        private TurnState _currentTurn = TurnState.PlayerTurn;
        
        public TurnState CurrentTurn => _currentTurn;

        public GameWorld(MapStyle mapStyle = MapStyle.Cave)
        {
            _mapStyle = mapStyle;
            _map = new Cell[MapHeight, MapWidth];
            _enemies = new List<Enemy>();
            _loot = new List<Item>();
            _random = new Random();
            
            _collisionSystem = new CollisionSystem(this);
            _pathfinder = new Pathfinder(this);
            _mapGenerator = new MapGenerator(MapWidth, MapHeight, _random, _mapStyle);
            _enemyFactory = new EnemyFactory(MapWidth, MapHeight, _random, this, _collisionSystem, _pathfinder);
            
            _map = _mapGenerator.GenerateDungeon();
            _enemies = _enemyFactory.SpawnEnemies();

            EventSystem.OnTurnChanged += HandleTurnChange;
        }

        private void HandleTurnChange(object? sender, TurnEvent e)
        {
            _currentTurn = e.State;
        }

        public Player NewPlayer() {
            // create a player at the center of the map but avoid any obstacles
            int x = MapWidth / 2, y = MapHeight / 2;
            int round = 0;
            while (!IsWalkable(x, y))
            {
                x = _random.Next(x - round, x + round);
                y = _random.Next(y - round, y + round);
                round ++;
            }
            return new Player(x, y, this, _collisionSystem);
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

            return _map[y, x].Character == '.';
        }

        public Enemy? GetEnemyAt(int x, int y)
        {
            return _enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }

        public List<(int x, int y)> GetPath((int x, int y) start, (int x, int y) end)
        {
            return _pathfinder.FindPath(start, end);
        }

        public void Draw(IDrawingContext ctx)
        {
            var viewport = ctx.Viewport;

            // Draw map tiles within viewport
            for (int y = viewport.Y; y < viewport.Y + viewport.Height; y++)
            {
                for (int x = viewport.X; x < viewport.X + viewport.Width; x++)
                {
                    if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
                    {
                        ctx.DrawAt((x, y), _map[y, x]);
                    }
                }
            }

            // Draw enemies within viewport
            foreach (var enemy in _enemies)
            {
                if (viewport.Contains((enemy.X, enemy.Y)))
                {
                    enemy.Draw(ctx);
                }
            }

            // Draw loot within viewport
            foreach (var item in _loot)
            {
                if (viewport.Contains((item.X, item.Y)))
                {
                    ctx.DrawAt((item.X, item.Y), '*');
                }
            }
        }
    }
}
