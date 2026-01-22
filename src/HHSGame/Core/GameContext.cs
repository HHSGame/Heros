
using HHSGame.UI;
using HHSGame.Core.Combat;
using HHSGame.Core.Map;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Classes;

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
        public Stats.Attributes? PlayerAttributes { get; set; }
        public Stats.Skills? PlayerSkills { get; set; }
        public ClassConfig? PlayerClass { get; set; }
        public List<string> StartingItems { get; set; } = new() { "HealthPotion" };
        public Coordinate? PlayerStartPosition { get; set; }
        public List<PlayerSpawn> PlayerSpawns { get; set; } = [];
        public List<EnemySpawn> EnemySpawns { get; set; } = [];
        public List<MapItemSpawn> MapItems { get; set; } = [];
    }

    public class GameContext(
        GameParameters parameters,
        Random random,
        EnemyFactory enemyFactory,
        CollisionSystem collisionSystem,
        MapState mapState,
        EnemyManager enemyManager,
        ItemManager itemManager,
        Items.ItemCatalog itemCatalog,
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
        public EnemyFactory EnemyFactory => enemyFactory;
        public CollisionSystem CollisionSystem => collisionSystem;
        public MapState MapState => mapState;
        public EnemyManager EnemyManager => enemyManager;
        public ItemManager ItemManager => itemManager;
        public Items.ItemCatalog ItemCatalog => itemCatalog;
        public SurroundingsManager SurroundingsManager => surroundingsManager;
        public InventoryManager InventoryManager => inventoryManager;
        public TurnManager TurnManager => turnManager;
        public GameStateMachine StateMachine => stateMachine;
        public IDrawingContext DrawingContext => drawingContext;
        public Player Player => player!;
        public Player? PlayerOrNull => player;

        public void InitializeContext(Player player)
        {
            InitializeContext([player], player);
        }

        public void InitializeContext(IReadOnlyList<Player> players, Player activePlayer)
        {
            enemyManager.SetEnemies(enemyFactory.SpawnEnemies(parameters.EnemySpawns));
            collisionSystem.SetPlayers(players);
            player = activePlayer;
        }

        public void SetActivePlayer(Player player)
        {
            this.player = player;
        }
    }
}
