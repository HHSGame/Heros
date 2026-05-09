using HHSGame.Core.Interactions;
using System.Text;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Engine.Scripting;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.Dialogue;
using HHSGame.UI;
using Microsoft.Extensions.Logging.Abstractions;
using Terminal.Gui.ViewBase;

namespace HHSGame.Core.Engine.Scripting
{
    [TestClass]
    public class GameScriptRunnerTests
    {
        [TestMethod]
        public void LoadSupportsInputArray()
        {
            string path = CreateTempScriptFile("[\"C\", \"Q\"]");
            GameScriptLoader loader = new(new GameDataLoader(NullLogger<GameDataLoader>.Instance));

            GameScript script = loader.Load(path);

            Assert.AreEqual(2, script.Steps.Count);
            Assert.AreEqual("C", script.Steps[0].Input);
            Assert.AreEqual("Q", script.Steps[1].Input);
        }

        [TestMethod]
        public void ScriptRunnerExecutesCombatToggleFlow()
        {
            Game game = CreateGame(12);
            game.Context.EnemyManager.SetEnemies(new List<Enemy>());
            int startX = game.Player!.X;

            string scriptJson = "{\"inputs\":[\"C\",\"M\",\"Right\",\"Right\",\"Enter\",\"Enter\",\"C\",\"QUIT\"]}";
            string scriptPath = CreateTempScriptFile(scriptJson);
            GameScriptLoader loader = new(new GameDataLoader(NullLogger<GameDataLoader>.Instance));
            GameScript script = loader.Load(scriptPath);

            GameScriptRunner runner = new(script, new GameScriptInputAdapter(game), new ScriptRunnerOptions
            {
                MaxTotalSteps = 200
            });

            ScriptRunnerResult result = runner.RunToCompletion();

            Assert.AreEqual(ScriptRunnerStatus.Completed, result.Status, result.FailureReason);
            Assert.AreEqual(startX + 2, game.Player!.X);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        private static Game CreateGame(int mapSize)
        {
            string mapPath = CreateTempMapFile(mapSize, mapSize);
            Random random = new(1234);

            GameParameters parameters = new()
            {
                MapStyle = MapStyle.Cave,
                MapWidth = mapSize,
                MapHeight = mapSize,
                UseCustomMap = true,
                CustomMapPath = mapPath,
                PlayerStartPosition = new Coordinate(2, 2)
            };

            GameCatalog catalogs = CreateCatalogs();
            ItemManager itemManager = new();
            MapState mapState = new();
            InventoryManager inventoryManager = new();
            PartyState partyState = new();
            QuestManager questManager = new(partyState, inventoryManager, catalogs.ItemCatalog);
            EnemyManager enemyManager = new(itemManager, questManager);
            NpcManager npcManager = new();
            CollisionSystem collisionSystem = new(enemyManager, mapState, npcManager);
            Pathfinder pathfinder = new(mapState);
            EnemyFactory enemyFactory = new(catalogs.EnemyCatalog, catalogs.ItemCatalog, collisionSystem, mapState, pathfinder, new global::HHSGame.Core.Factions.FactionManager());
            NpcFactory npcFactory = new(catalogs.ItemCatalog);
            DialogueManager dialogueManager = new(partyState, questManager);
            SurroundingsManager surroundingsManager = new(itemManager, enemyManager, mapState, npcManager);
            TurnManager turnManager = new();
            GameStateMachine stateMachine = new();
            IDrawingContext drawingContext = new TestDrawingContext(20, 10);

            GameContext context = new(
                parameters,
                random,
                enemyFactory,
                collisionSystem,
                mapState,
                enemyManager,
                itemManager,
                catalogs.ItemCatalog,
                npcFactory,
                npcManager,
                surroundingsManager,
                inventoryManager,
                new InteractableManager(),
                turnManager,
                stateMachine,
                questManager,
                dialogueManager,
                partyState,
                new global::HHSGame.Core.Factions.FactionManager(),
                drawingContext);

            GameWorld world = new(context);
            GameConfig config = new();
            Game game = new(NullLogger<Game>.Instance, context, world, catalogs.ClassCatalog, config);
            game.Start();
            return game;
        }

        private static GameCatalog CreateCatalogs()
        {
            List<WeaponDefinition> weapons =
            [
                new WeaponDefinition("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line),
                new WeaponDefinition("Sword", "Sword", ItemRarity.Common, 100, 2.0f, 10, 1, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
            ];

            List<ArmorDefinition> armors =
            [
                new ArmorDefinition("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0),
                new ArmorDefinition("Shield", "Shield", ItemRarity.Common, 150, 3.0f, 5)
            ];

            List<ItemDefinition> items =
            [
                new ItemDefinition("HealthPotion", "HealthPotion", "Health Potion", ItemRarity.Common, 50, 0.5f, 10)
            ];

            ItemCatalog itemCatalog = new(weapons, armors, items);

            List<ClassDefinition> classes =
            [
                new ClassDefinition(
                    "Warrior",
                    "Warrior",
                    new Stats.Attributes { Strength = 7, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Stats.Skills(),
                    "Sword",
                    "Shield")
            ];

            ClassCatalog classCatalog = new(classes, itemCatalog);

            EnemyCatalog enemyCatalog = new(new List<EnemyDefinition>());

            return new GameCatalog(itemCatalog, classCatalog, enemyCatalog);
        }

        private static string CreateTempMapFile(int width, int height)
        {
            string path = Path.Combine(Path.GetTempPath(), $"hhs-map-{Guid.NewGuid():N}.txt");
            string line = new('.', width);
            StringBuilder mapBuilder = new();
            for (int i = 0; i < height; i++)
            {
                mapBuilder.AppendLine(line);
            }

            File.WriteAllText(path, mapBuilder.ToString(), Encoding.ASCII);
            return path;
        }

        private static string CreateTempScriptFile(string contents)
        {
            string path = Path.Combine(Path.GetTempPath(), $"hhs-script-{Guid.NewGuid():N}.json");
            File.WriteAllText(path, contents, Encoding.ASCII);
            return path;
        }

        private sealed class TestDrawingContext(int width, int height) : IDrawingContext
        {
            public Viewport Viewport { get; } = new(0, 0, width, height);

            public void DrawAt((int X, int Y) pos, char tile)
            {
            }

            public void DrawAt((int X, int Y) pos, Cell cell)
            {
            }

            public void Clear()
            {
            }

            public void Render()
            {
            }

            public void AttachTo(View parent)
            {
            }
        }
    }
}
