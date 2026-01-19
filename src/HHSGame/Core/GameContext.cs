
using HHSGame.UI;
using HHSGame.Core.Combat;
using HHSGame.Core.Map;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;

namespace HHSGame.Core
{
    public class GameParameters
    {
        public MapStyle MapStyle { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public bool InitialItems { get; set; } = true;
        public string? CustomMapPath { get; set; }
        public bool UseCustomMap { get; set; }
    }

    public class GameContext(
        GameParameters parameters,
        Random random,
        MapGenerator mapGenerator,
        EnemyFactory enemyFactory,
        CollisionSystem collisionSystem,
        MapState mapState,
        EnemyManager enemyManager,
        ItemManager itemManager,
        ItemFactory itemFactory,
        SurroundingsManager surroundingsManager,
        InventoryManager inventoryManager,
        TurnManager turnManager,
        GameStateMachine stateMachine,
        IDrawingContext drawingContext
    )
    {
        private Player? player;
        public GameParameters Parameters => parameters;
        public Random Random => random;
        public MapGenerator MapGenerator => mapGenerator;
        public EnemyFactory EnemyFactory => enemyFactory;
        public CollisionSystem CollisionSystem => collisionSystem;
        public MapState MapState => mapState;
        public EnemyManager EnemyManager => enemyManager;
        public ItemManager ItemManager => itemManager;
        public ItemFactory ItemFactory => itemFactory;
        public SurroundingsManager SurroundingsManager => surroundingsManager;
        public InventoryManager InventoryManager => inventoryManager;
        public TurnManager TurnManager => turnManager;
        public GameStateMachine StateMachine => stateMachine;
        public IDrawingContext DrawingContext => drawingContext;
        public Player Player => player!;
        public Player? PlayerOrNull => player;

        public void InitializeContext(Player player)
        {
            enemyManager.SetEnemies(enemyFactory.SpawnEnemies());
            this.player = player;
        }
    }
}
