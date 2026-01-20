using System.Text;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.UI;
using Microsoft.Extensions.Logging.Abstractions;
using Terminal.Gui.ViewBase;

namespace HHSGame.Core
{
    [TestClass]
    public class GameIntegrationTests
    {
        [TestMethod]
        public void PerformPlayerActionConsumesApWithoutEndingTurn()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            int startingAp = player.Stats.CurrentAp;
            bool executed = false;

            bool result = game.PerformPlayerAction(() => executed = true, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.IsTrue(executed);
            Assert.AreEqual(startingAp - 1, player.Stats.CurrentAp);
            Assert.IsTrue(game.Context.TurnManager.IsPlayerTurn());
            Assert.AreEqual(0, game.Context.TurnManager.TurnCount);
        }

        [TestMethod]
        public void PerformPlayerActionEndsTurnAndAdvancesEnemyTurn()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            int maxAp = player.Stats.MaxAp;
            Enemy? sampleEnemy = game.Context.EnemyManager.Enemies.FirstOrDefault();

            Assert.IsNotNull(sampleEnemy);
            int enemyStartingAp = sampleEnemy.Stats.CurrentAp;

            bool result = game.PerformPlayerAction(() => { }, maxAp, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(maxAp, player.Stats.CurrentAp);
            Assert.IsTrue(game.Context.TurnManager.IsPlayerTurn());
            Assert.AreEqual(1, game.Context.TurnManager.TurnCount);
            Assert.AreEqual(enemyStartingAp, sampleEnemy.Stats.MaxAp);
            Assert.AreEqual(0, sampleEnemy.Stats.CurrentAp);
        }

        private static Game CreateGame()
        {
            const int mapSize = 30;
            string mapPath = CreateTempMapFile(mapSize, mapSize);
            Random random = new(1234);

            GameParameters parameters = new()
            {
                MapStyle = MapStyle.Cave,
                MapWidth = mapSize,
                MapHeight = mapSize,
                UseCustomMap = true,
                CustomMapPath = mapPath
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
