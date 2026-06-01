using HHSGame.Core.Interactions;
using System.Text;
using HHSGame.Core.Combat;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.Dialogue;
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
        public void PlannedActionsExecuteInInputOrderAcrossPlayers()
        {
            Game game = CreateGame(playerPositions: new[] { new Coordinate(2, 2), new Coordinate(4, 2) });
            IReadOnlyList<Player> players = game.Context.Players;
            game.Context.StateMachine.TryChangeState(GameStateType.Combat);
            foreach (Player player in players)
            {
                player.ResetTurn(true, player == game.Player);
            }

            List<string> executionOrder = [];

            Assert.IsTrue(game.TryQueuePlayerAction("A1", 1, () => executionOrder.Add("A1")));
            Assert.IsTrue(game.SwitchControlledPlayer(1));
            Assert.IsTrue(game.TryQueuePlayerAction("B1", 1, () => executionOrder.Add("B1")));
            Assert.IsTrue(game.SwitchControlledPlayer(1));
            Assert.IsTrue(game.TryQueuePlayerAction("A2", 1, () => executionOrder.Add("A2")));

            game.CommitPlayerActions();

            CollectionAssert.AreEqual(new[] { "A1", "B1", "A2" }, executionOrder);
        }

        [TestMethod]
        public void DiagonalMovementCostRoundsUpPerTwoSteps()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            List<Coordinate> path =
            [
                new Coordinate(1, 1),
                new Coordinate(2, 2),
                new Coordinate(3, 3)
            ];

            int cost = game.CalculateMovementApCost(player, path);

            Assert.AreEqual(3, cost);
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
        public void GameEndsWhenCurrencyAtLeastConditionMet()
        {
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "CurrencyAtLeast", Value = 10 }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config);
            game.Context.InventoryManager.AddCurrency(10);

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenQuestStatusConditionMet()
        {
            QuestDefinition quest = new(
                "Quest-1",
                "Quest-1",
                string.Empty,
                string.Empty,
                Array.Empty<QuestObjectiveDefinition>(),
                Array.Empty<QuestRewardDefinition>());
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "QuestStatus", Param = "Quest-1", Value = (int)QuestStatus.Active }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config, questDefinitions: new[] { quest });
            game.Context.QuestManager.StartQuest("Quest-1");

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenQuestObjectivesCompleteConditionMet()
        {
            QuestDefinition quest = new(
                "Quest-2",
                "Quest-2",
                string.Empty,
                string.Empty,
                new[]
                {
                    new QuestObjectiveDefinition(QuestObjectiveKind.TalkToNpc, "npc-1", 1)
                },
                Array.Empty<QuestRewardDefinition>());
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "QuestObjectivesComplete", Param = "Quest-2" }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config, questDefinitions: new[] { quest });
            game.Context.QuestManager.StartQuest("Quest-2");
            game.Context.QuestManager.NotifyNpcTalked("npc-1");

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenQuestReadyToTurnInConditionMet()
        {
            QuestDefinition quest = new(
                "Quest-3",
                "Quest-3",
                string.Empty,
                string.Empty,
                new[]
                {
                    new QuestObjectiveDefinition(QuestObjectiveKind.TalkToNpc, "npc-2", 1)
                },
                Array.Empty<QuestRewardDefinition>());
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "QuestReadyToTurnIn", Param = "Quest-3" }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config, questDefinitions: new[] { quest });
            game.Context.QuestManager.StartQuest("Quest-3");
            game.Context.QuestManager.NotifyNpcTalked("npc-2");

            bool result = game.PerformPlayerAction(() => { }, 1, false, GameStateType.Exploration);

            Assert.IsTrue(result);
            Assert.AreEqual(GameStateType.GameOver, game.Context.StateMachine.CurrentState);
        }

        [TestMethod]
        public void GameEndsWhenAchievementUnlockedConditionMet()
        {
            AchievementDefinition achievement = new(
                "Ach-1",
                "Ach-1",
                string.Empty,
                new[]
                {
                    new QuestObjectiveDefinition(QuestObjectiveKind.TalkToNpc, "npc-3", 1)
                });
            GameConfig config = new()
            {
                Conditions = new ConditionConfig
                {
                    WinEntries = new List<ConditionEntryConfig>
                    {
                        new ConditionEntryConfig { Id = "AchievementUnlocked", Param = "Ach-1" }
                    },
                    Lose = new List<string>()
                }
            };
            Game game = CreateGame(config, achievementDefinitions: new[] { achievement });
            game.Context.QuestManager.NotifyNpcTalked("npc-3");

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

        private static Game CreateGame(GameConfig? config = null, string? mapPathOverride = null, IReadOnlyList<Coordinate>? playerPositions = null, IReadOnlyList<QuestDefinition>? questDefinitions = null, IReadOnlyList<AchievementDefinition>? achievementDefinitions = null)
        {
            const int mapSize = 30;
            string mapPath = mapPathOverride ?? CreateTempMapFile(mapSize, mapSize);
            Random random = new(1234);

            GameParameters parameters = new()
            {
                MapStyle = MapStyle.UrbanStreet,
                MapWidth = mapSize,
                MapHeight = mapSize,
                UseCustomMap = true,
                CustomMapPath = mapPath
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
            GameConfig gameConfig = config ?? new GameConfig();
            Game game = new(NullLogger<Game>.Instance, context, world, catalogs.ClassCatalog, gameConfig);
            if (playerPositions != null)
            {
                parameters.PlayerSpawns = playerPositions
                    .Select((position, index) => new PlayerSpawn(
                        $"Player {index + 1}",
                        (char)('A' + index),
                        catalogs.ClassCatalog.GetDefault(),
                        null,
                        null,
                        position,
                        new List<string>()))
                    .ToList();
            }
            if (questDefinitions != null)
            {
                parameters.QuestDefinitions = questDefinitions.ToList();
            }
            if (achievementDefinitions != null)
            {
                parameters.AchievementDefinitions = achievementDefinitions.ToList();
            }
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
                    "Gangster", "Gangster", "Neutral", new Stats.Attributes { Strength = 4, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Stats.Skills(),
                    "Dagger",
                    "Cloak",
                    50,
                    'g',
                    HHSGame.UI.ColorPresets.Enemies.Occupier,
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
