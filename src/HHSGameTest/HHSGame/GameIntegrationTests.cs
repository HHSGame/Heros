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
        public void PerformPlayerActionInExplorationEndsTurnWithoutAp()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            bool executed = false;

            bool result = game.PerformPlayerAction(() => executed = true, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.IsTrue(executed);
            if (game.Context.StateMachine.CurrentState == GameStateType.Combat)
            {
                Assert.AreEqual(player.Stats.MaxAp, player.Stats.CurrentAp);
            }
            else
            {
                Assert.AreEqual(0, player.Stats.CurrentAp);
            }
            Assert.IsTrue(game.Context.TurnManager.IsPlayerTurn());
            Assert.AreEqual(1, game.Context.TurnManager.TurnCount);
        }

        [TestMethod]
        public void PerformPlayerActionInCombatConsumesApWithoutEndingTurn()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            EnsureVisibleEnemy(game);
            game.Context.StateMachine.TryChangeState(GameStateType.Combat);
            player.ResetTurn(true);
            int startingAp = player.Stats.CurrentAp;

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Combat);

            Assert.IsTrue(result);
            Assert.AreEqual(startingAp - 1, player.Stats.CurrentAp);
            Assert.IsTrue(game.Context.TurnManager.IsPlayerTurn());
            Assert.AreEqual(0, game.Context.TurnManager.TurnCount);
        }

        [TestMethod]
        public void QueuedMoveExecutesOnce()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            MovePlayerToClearPosition(game);
            game.Context.StateMachine.TryChangeState(GameStateType.Combat);
            player.ResetTurn(true);
            int startX = player.X;

            bool queued = game.TryQueuePlayerAction("Move", ActionCosts.Movement, () => player.Move(new Move.Forward(Direction.Right)));
            Assert.IsTrue(queued);

            game.CommitPlayerActions();

            Assert.AreEqual(startX + 1, player.X);
        }

        [TestMethod]
        public void QueuedActionExecutesOnce()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            game.Context.StateMachine.TryChangeState(GameStateType.Combat);
            player.ResetTurn(true);
            int executions = 0;

            bool queued = game.TryQueuePlayerAction("Test", 1, () => executions++);
            Assert.IsTrue(queued);

            game.CommitPlayerActions();

            Assert.AreEqual(1, executions);
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
            MapState mapState = new();
            EnemyManager enemyManager = new(itemManager);
            CollisionSystem collisionSystem = new(enemyManager, mapState);
            Pathfinder pathfinder = new(mapState);
            EnemyFactory enemyFactory = new(collisionSystem, mapState, pathfinder);
            SurroundingsManager surroundingsManager = new(itemManager, enemyManager, mapState);
            InventoryManager inventoryManager = new();
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

        private static void EnsureVisibleEnemy(Game game)
        {
            Enemy? enemy = game.Context.EnemyManager.Enemies.FirstOrDefault();
            Assert.IsNotNull(enemy);

            Player player = game.Player!;
            enemy.X = player.X + 1;
            enemy.Y = player.Y;
        }

        private static void MovePlayerToClearPosition(Game game)
        {
            Player player = game.Player!;
            MapState map = game.Context.MapState;
            EnemyManager enemies = game.Context.EnemyManager;

            for (int y = 1; y < map.Height - 1; y++)
            {
                for (int x = 1; x < map.Width - 1; x++)
                {
                    if (!map.IsWalkable(x, y) || !map.IsWalkable(x + 1, y))
                    {
                        continue;
                    }

                    if (enemies.GetEnemyAt(x, y) != null || enemies.GetEnemyAt(x + 1, y) != null)
                    {
                        continue;
                    }

                    player.X = x;
                    player.Y = y;
                    player.UpdateFOV();
                    return;
                }
            }

            Assert.Fail("No clear position found for movement test.");
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
