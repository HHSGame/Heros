using System.Text;
using HHSGame.Core.Engine.Scripting;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
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

            string scriptJson = "{\"inputs\":[\"C\",\"M\",\"Right\",\"Right\",\"Enter\",\"Enter\",\"C\",\"Q\"]}";
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

            ItemManager itemManager = new();
            ItemFactory itemFactory = new(random);
            MapGenerator mapGenerator = new(parameters, random, itemManager, itemFactory);
            MapState mapState = new();
            EnemyManager enemyManager = new(itemManager);
            CollisionSystem collisionSystem = new(enemyManager, mapState);
            Pathfinder pathfinder = new(mapState);
            EnemyFactory enemyFactory = new(random, collisionSystem, mapState, pathfinder);
            SurroundingsManager surroundingsManager = new(itemManager, enemyManager, mapState);
            InventoryManager inventoryManager = new();
            TurnManager turnManager = new();
            GameStateMachine stateMachine = new();
            IDrawingContext drawingContext = new TestDrawingContext(20, 10);

            GameContext context = new(
                parameters,
                random,
                mapGenerator,
                enemyFactory,
                collisionSystem,
                mapState,
                enemyManager,
                itemManager,
                itemFactory,
                surroundingsManager,
                inventoryManager,
                turnManager,
                stateMachine,
                drawingContext);

            GameWorld world = new(context);
            Game game = new(NullLogger<Game>.Instance, context, world);
            game.Start();
            return game;
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
