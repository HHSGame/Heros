
using RpgGame.UI;

namespace RpgGame.Core {

    public struct GameParameters {
        public MapStyle mapStyle;
    }

    public class GameContext {
        public static int MapWidth { get; } = 500;
        public static int MapHeight { get; } = 500;
        public GameParameters Parameters { get; }

        private readonly Random _random;
        private readonly MapGenerator _mapGenerator;
        private readonly EnemyFactory _enemyFactory;
        private readonly CollisionSystem _collisionSystem;
        private readonly MapState _mapState;
        private readonly EnemyManager _enemyManager;
        private readonly ItemManager _itemManager;
        private readonly SurroundingsManager _surroundingsManager;
        private readonly InventoryManager _inventoryManager;
        private IDrawingContext? _drawingContext;

        public Random Random => _random;
        public MapGenerator MapGenerator => _mapGenerator;
        public EnemyFactory EnemyFactory => _enemyFactory;
        public CollisionSystem CollisionSystem => _collisionSystem;
        public MapState MapState => _mapState;
        public EnemyManager EnemyManager => _enemyManager;
        public ItemManager ItemManager => _itemManager;
        public SurroundingsManager SurroundingsManager => _surroundingsManager;
        public InventoryManager InventoryManager => _inventoryManager;

        public IDrawingContext DrawingContext {
            get => _drawingContext ?? throw new InvalidOperationException("Drawing context not initialized");
        }

        public GameContext(GameParameters parameters) {
            Parameters = parameters;
            var mapStyle = parameters.mapStyle;
            _random = new Random();

            _itemManager = new ItemManager();
            _inventoryManager = new InventoryManager();
            _mapGenerator = new MapGenerator(MapWidth, MapHeight, _random, _itemManager, mapStyle);            
            _mapState = new MapState(MapWidth, MapHeight);
            _mapState.Init(_mapGenerator.GenerateDungeon());
            _enemyManager = new EnemyManager(_itemManager);
            _collisionSystem = new CollisionSystem(_enemyManager, _mapState);
            _enemyFactory = new EnemyFactory(MapWidth, MapHeight, _random, _collisionSystem, _mapState);
            _enemyManager.SetEnemies(_enemyFactory.SpawnEnemies());
            _surroundingsManager = new SurroundingsManager(_itemManager, _enemyManager, _mapState);
        }

        public void InitializeDrawingContext(IDrawingContext drawingContext) {
            _drawingContext = drawingContext;
            drawingContext.MapState = _mapState;
        }
    }
}