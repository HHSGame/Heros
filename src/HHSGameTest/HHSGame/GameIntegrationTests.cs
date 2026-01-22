using System.Text;
using HHSGame.Core.Combat;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
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

        [TestMethod]
        public void GameEndsWhenPlayerDeathConditionMet()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            player.Stats.ApplyDamage(player.Stats.CurrentHp);

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenWinConditionMet()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    Win = new List<string> { "AllEnemiesDefeated" },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config);
            game.Context.EnemyManager.SetEnemies(new List<Enemy>());

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenTurnLimitConditionMet()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    LoseEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "TurnLimit", Value = 1 }
                    }
                }
            };
            Game game = CreateGame(config);

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenEnemyCountAtLeastConditionMet()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    LoseEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "EnemyCountAtLeast", Value = 1 }
                    }
                }
            };
            Game game = CreateGame(config);
            EnsureVisibleEnemy(game);

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenEnemyCountAtMostConditionMet()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "EnemyCountAtMost", Value = 0 }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config);
            game.Context.EnemyManager.SetEnemies(new List<Enemy>());

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenHasItemConditionMet()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "HasItem", Param = "HealthPotion", Value = 1 }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config);

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenReachMarkerConditionMet()
        {
            string mapPath = CreateTempMapFile(new[]
            {
                "#####",
                "#@>.#",
                "#####"
            });
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "ReachMarker", Param = ">" }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config, mapPath);

            bool result = game.PerformPlayerAction(
                () => game.Player!.Move(new Move.Forward(Direction.Right)),
                ActionCosts.Movement,
                false,
                GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void UnknownConditionThrows()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    Win = new List<string> { "UnknownCondition" }
                }
            };

            Assert.ThrowsException<InvalidDataException>(() => CreateGame(config));
        }

        private static Game CreateGame(GameConfig? config = null, string? mapPathOverride = null)
        {
            const int mapSize = 30;
            string mapPath = mapPathOverride ?? CreateTempMapFile(mapSize, mapSize);
            Random random = new(1234);

            GameParameters parameters = new()
            {
                MapStyle = MapStyle.Cave,
                MapWidth = mapSize,
                MapHeight = mapSize,
                UseCustomMap = true,
                CustomMapPath = mapPath
            };

            GameCatalog catalogs = CreateCatalogs();
            ItemManager itemManager = new();
            MapState mapState = new();
            EnemyManager enemyManager = new(itemManager);
            CollisionSystem collisionSystem = new(enemyManager, mapState);
            Pathfinder pathfinder = new(mapState);
            EnemyFactory enemyFactory = new(catalogs.EnemyCatalog, catalogs.ItemCatalog, collisionSystem, mapState, pathfinder);
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
                catalogs.ItemCatalog,
                surroundingsManager,
                inventoryManager,
                turnManager,
                stateMachine,
                drawingContext);

            GameWorld world = new(context);
            GameConfig gameConfig = config ?? new GameConfig();
            Game game = new(NullLogger<Game>.Instance, context, world, catalogs.ClassCatalog, gameConfig);
            game.Start();
            return game;
        }

        private static void EnsureVisibleEnemy(Game game)
        {
            Player player = game.Player!;
            Enemy enemy = game.Context.EnemyFactory.CreateEnemy("Gangster", player.X + 1, player.Y);
            game.Context.EnemyManager.SetEnemies(new List<Enemy> { enemy });
        }

        private static GameCatalog CreateCatalogs()
        {
            List<WeaponDefinition> weapons =
            [
                new WeaponDefinition("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line),
                new WeaponDefinition("Sword", "Sword", ItemRarity.Common, 100, 2.0f, 10, 1, 1, WeaponType.MeleeLight, WeaponTrajectory.Line),
                new WeaponDefinition("Dagger", "Dagger", ItemRarity.Common, 60, 0.5f, 5, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
            ];

            List<ArmorDefinition> armors =
            [
                new ArmorDefinition("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0),
                new ArmorDefinition("Shield", "Shield", ItemRarity.Common, 150, 3.0f, 5),
                new ArmorDefinition("Cloak", "Cloak", ItemRarity.Common, 80, 0.6f, 2)
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

            List<EnemyDefinition> enemies =
            [
                new EnemyDefinition(
                    "Gangster",
                    "Gangster",
                    new Stats.Attributes { Strength = 4, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Stats.Skills(),
                    "Dagger",
                    "Cloak",
                    50,
                    'g',
                    HHSGame.UI.ColorPresets.Enemies.Gangster,
                    new List<EnemyLootEntry>(),
                    new List<EnemyAbilityDefinition>())
            ];

            EnemyCatalog enemyCatalog = new(enemies);

            return new GameCatalog(itemCatalog, classCatalog, enemyCatalog);
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

        private static string CreateTempMapFile(string[] lines)
        {
            string path = Path.Combine(Path.GetTempPath(), $"hhs-map-{Guid.NewGuid():N}.txt");
            StringBuilder mapBuilder = new();
            foreach (string line in lines)
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
