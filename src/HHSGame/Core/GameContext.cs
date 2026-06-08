
using HHSGame.Core.Rendering;
using HHSGame.Core.Combat;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Enemies;
using HHSGame.Core.Interactions;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
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
        public List<NpcSpawn> NpcSpawns { get; set; } = [];
        public List<DialogueDefinition> DialogueDefinitions { get; set; } = [];
        public List<QuestDefinition> QuestDefinitions { get; set; } = [];
        public List<AchievementDefinition> AchievementDefinitions { get; set; } = [];
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
        NpcFactory npcFactory,
        NpcManager npcManager,
        SurroundingsManager surroundingsManager,
        InventoryManager inventoryManager,
        InteractableManager interactableManager,
        TurnManager turnManager,
        GameStateMachine stateMachine,
        QuestManager questManager,
        DialogueManager dialogueManager,
        PartyState partyState,
        Factions.FactionManager factionManager,
        IDrawingContext drawingContext
    )
    {
        private Player? player;
        private IReadOnlyList<Player> players = Array.Empty<Player>();

        // 分组上下文
        public CombatContext CombatContext { get; } = new(enemyManager, turnManager, enemyFactory);
        public WorldContext WorldContext { get; } = new(mapState, collisionSystem, surroundingsManager, interactableManager);

        public GameParameters Parameters => parameters;
        public Random Random => random;
        public EnemyFactory EnemyFactory => enemyFactory;
        public CollisionSystem CollisionSystem => collisionSystem;
        public MapState MapState => mapState;
        public EnemyManager EnemyManager => enemyManager;
        public ItemManager ItemManager => itemManager;
        public Items.ItemCatalog ItemCatalog => itemCatalog;
        public NpcFactory NpcFactory => npcFactory;
        public NpcManager NpcManager => npcManager;
        public SurroundingsManager SurroundingsManager => surroundingsManager;
        public InventoryManager InventoryManager => inventoryManager;
        public InteractableManager InteractableManager => interactableManager;
        public TurnManager TurnManager => turnManager;
        public GameStateMachine StateMachine => stateMachine;
        public QuestManager QuestManager => questManager;
        public DialogueManager DialogueManager => dialogueManager;
        public PartyState PartyState => partyState;
        public Factions.FactionManager FactionManager => factionManager;
        public IDrawingContext DrawingContext => drawingContext;
        public Player Player => player!;
        public Player? PlayerOrNull => player;
        public IReadOnlyList<Player> Players => players;
        public int ActivePlayerIndex { get; private set; } = -1;

        public void InitializeContext(Player player)
        {
            InitializeContext([player], player);
        }

        public void InitializeContext(IReadOnlyList<Player> players, Player activePlayer)
        {
            enemyManager.SetEnemies(enemyFactory.SpawnEnemies(parameters.EnemySpawns));
            npcManager.SetNpcs(npcFactory.CreateNpcs(parameters.NpcSpawns));
            questManager.LoadDefinitions(parameters.QuestDefinitions, parameters.AchievementDefinitions);
            dialogueManager.LoadDefinitions(parameters.DialogueDefinitions);
            collisionSystem.SetPlayers(players);
            this.players = players;
            SetActivePlayer(activePlayer);
            partyState.SetPlayers(players, activePlayer);
        }

        /// <summary>
        /// Restores context from saved state. Definitions are loaded but enemies/NPCs
        /// are NOT re-spawned — they are restored from save data by LoadManager.
        /// </summary>
        public void RestoreContext(IReadOnlyList<Player> players, Player activePlayer)
        {
            questManager.LoadDefinitions(parameters.QuestDefinitions, parameters.AchievementDefinitions);
            dialogueManager.LoadDefinitions(parameters.DialogueDefinitions);
            collisionSystem.SetPlayers(players);
            this.players = players;
            SetActivePlayer(activePlayer);
            partyState.SetPlayers(players, activePlayer);
        }

        public void SetActivePlayer(Player player)
        {
            this.player = player;
            ActivePlayerIndex = FindPlayerIndex(player);
            partyState.SetActivePlayer(player);
        }

        private int FindPlayerIndex(Player player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (ReferenceEquals(players[i], player))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
