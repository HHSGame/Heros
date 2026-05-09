using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Interactions;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.Stats;
using HHSGame.UI;
using Microsoft.Extensions.Logging.Abstractions;
using Terminal.Gui.ViewBase;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class ConditionEvaluatorTests
    {
        // ── ParseCondition ─────────────────────────────────────────────

        [TestMethod]
        public void ParseCondition_PlayerDeath_ReturnsSpec()
        {
            var spec = ConditionEvaluator.ParseCondition("PlayerDeath", null, null);
            Assert.AreEqual(ConditionEvaluator.ConditionKind.PlayerDeath, spec.Kind);
        }

        [TestMethod]
        public void ParseCondition_AllEnemiesDefeated_ReturnsSpec()
        {
            var spec = ConditionEvaluator.ParseCondition("AllEnemiesDefeated", null, null);
            Assert.AreEqual(ConditionEvaluator.ConditionKind.AllEnemiesDefeated, spec.Kind);
        }

        [TestMethod]
        public void ParseCondition_TurnLimit_WithPositiveValue()
        {
            var spec = ConditionEvaluator.ParseCondition("TurnLimit", 10, null);
            Assert.AreEqual(ConditionEvaluator.ConditionKind.TurnLimit, spec.Kind);
            Assert.AreEqual(10, spec.Value);
        }

        [TestMethod]
        public void ParseCondition_TurnLimit_ZeroValue_Throws()
        {
            Assert.ThrowsException<InvalidDataException>(() =>
                ConditionEvaluator.ParseCondition("TurnLimit", 0, null));
        }

        [TestMethod]
        public void ParseCondition_HasItem_WithParam()
        {
            var spec = ConditionEvaluator.ParseCondition("HasItem", 1, "HealthPotion");
            Assert.AreEqual(ConditionEvaluator.ConditionKind.HasItem, spec.Kind);
            Assert.AreEqual("HealthPotion", spec.Param);
            Assert.AreEqual(1, spec.Value);
        }

        [TestMethod]
        public void ParseCondition_HasItem_NoParam_Throws()
        {
            Assert.ThrowsException<InvalidDataException>(() =>
                ConditionEvaluator.ParseCondition("HasItem", 1, null));
        }

        [TestMethod]
        public void ParseCondition_ReachMarker_WithParam()
        {
            var spec = ConditionEvaluator.ParseCondition("ReachMarker", null, ">");
            Assert.AreEqual(ConditionEvaluator.ConditionKind.ReachMarker, spec.Kind);
            Assert.AreEqual(">", spec.Param);
        }

        [TestMethod]
        public void ParseCondition_ReachMarker_WithValue_Throws()
        {
            Assert.ThrowsException<InvalidDataException>(() =>
                ConditionEvaluator.ParseCondition("ReachMarker", 5, ">"));
        }

        [TestMethod]
        public void ParseCondition_CurrencyAtLeast()
        {
            var spec = ConditionEvaluator.ParseCondition("CurrencyAtLeast", 100, null);
            Assert.AreEqual(ConditionEvaluator.ConditionKind.CurrencyAtLeast, spec.Kind);
            Assert.AreEqual(100, spec.Value);
        }

        [TestMethod]
        public void ParseCondition_QuestStatus()
        {
            var spec = ConditionEvaluator.ParseCondition("QuestStatus", 1, "Quest-1");
            Assert.AreEqual(ConditionEvaluator.ConditionKind.QuestStatus, spec.Kind);
            Assert.AreEqual("Quest-1", spec.Param);
        }

        [TestMethod]
        public void ParseCondition_QuestObjectivesComplete()
        {
            var spec = ConditionEvaluator.ParseCondition("QuestObjectivesComplete", null, "Quest-2");
            Assert.AreEqual(ConditionEvaluator.ConditionKind.QuestObjectivesComplete, spec.Kind);
            Assert.AreEqual("Quest-2", spec.Param);
        }

        [TestMethod]
        public void ParseCondition_QuestReadyToTurnIn()
        {
            var spec = ConditionEvaluator.ParseCondition("QuestReadyToTurnIn", null, "Quest-3");
            Assert.AreEqual(ConditionEvaluator.ConditionKind.QuestReadyToTurnIn, spec.Kind);
        }

        [TestMethod]
        public void ParseCondition_AchievementUnlocked()
        {
            var spec = ConditionEvaluator.ParseCondition("AchievementUnlocked", null, "Ach-1");
            Assert.AreEqual(ConditionEvaluator.ConditionKind.AchievementUnlocked, spec.Kind);
        }

        [TestMethod]
        public void ParseCondition_Unknown_Throws()
        {
            Assert.ThrowsException<InvalidDataException>(() =>
                ConditionEvaluator.ParseCondition("UnknownCondition", null, null));
        }

        [TestMethod]
        public void ParseCondition_EmptyId_Throws()
        {
            Assert.ThrowsException<InvalidDataException>(() =>
                ConditionEvaluator.ParseCondition("", null, null));
        }

        [TestMethod]
        public void ParseCondition_NullId_Throws()
        {
            Assert.ThrowsException<InvalidDataException>(() =>
                ConditionEvaluator.ParseCondition(null, null, null));
        }

        // ── Condition evaluation ───────────────────────────────────────

        [TestMethod]
        public void CheckGameConditions_PlayerDead_ReturnsLose()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            player.Stats.ApplyDamage(player.Stats.CurrentHp);

            var evaluator = new ConditionEvaluator(game.Context, new ConditionConfig
            {
                Lose = new List<string> { "PlayerDeath" },
                Win = new List<string>()
            });

            var result = evaluator.CheckGameConditions(new List<Player> { player });
            Assert.IsTrue(result.IsMet);
            Assert.IsFalse(result.IsWin);
        }

        [TestMethod]
        public void CheckGameConditions_AllEnemiesDefeated_ReturnsWin()
        {
            Game game = CreateGame();
            game.Context.EnemyManager.SetEnemies(new List<Enemy>());

            var evaluator = new ConditionEvaluator(game.Context, new ConditionConfig
            {
                Win = new List<string> { "AllEnemiesDefeated" },
                Lose = new List<string>()
            });

            var result = evaluator.CheckGameConditions(new List<Player> { game.Player! });
            Assert.IsTrue(result.IsMet);
            Assert.IsTrue(result.IsWin);
        }

        [TestMethod]
        public void CheckGameConditions_NoConditionsMet_ReturnsNotMet()
        {
            Game game = CreateGame();
            EnsureVisibleEnemy(game);

            var evaluator = new ConditionEvaluator(game.Context, new ConditionConfig
            {
                Win = new List<string> { "AllEnemiesDefeated" },
                Lose = new List<string> { "PlayerDeath" }
            });

            var result = evaluator.CheckGameConditions(new List<Player> { game.Player! });
            Assert.IsFalse(result.IsMet);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static Game CreateGame()
        {
            GameParameters parameters = new()
            {
                MapStyle = MapStyle.Cave,
                MapWidth = 15,
                MapHeight = 15
            };

            GameCatalog catalogs = CreateCatalogs();
            Random random = new(1234);
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
            InteractableManager interactableManager = new();
            TurnManager turnManager = new();
            GameStateMachine stateMachine = new();
            IDrawingContext drawingContext = new TestDrawingContext(20, 10);

            GameContext context = new(
                parameters, random, enemyFactory, collisionSystem, mapState,
                enemyManager, itemManager, catalogs.ItemCatalog,
                npcFactory, npcManager, surroundingsManager,
                inventoryManager, interactableManager, turnManager, stateMachine,
                questManager, dialogueManager, partyState, new global::HHSGame.Core.Factions.FactionManager(), drawingContext);

            GameWorld world = new(context);
            Game game = new(NullLogger<Game>.Instance, context, world, catalogs.ClassCatalog, new GameConfig());
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
                new WeaponDefinition("Dagger", "Dagger", ItemRarity.Common, 60, 0.5f, 5, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
            ];

            List<ArmorDefinition> armors =
            [
                new ArmorDefinition("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0)
            ];

            List<ItemDefinition> items =
            [
                new ItemDefinition("HealthPotion", "HealthPotion", "Health Potion", ItemRarity.Common, 50, 0.5f, 10)
            ];

            ItemCatalog itemCatalog = new(weapons, armors, items);

            List<ClassDefinition> classes =
            [
                new ClassDefinition("Warrior", "Warrior",
                    new Attributes { Strength = 7, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(), "Dagger", "UnknownArmor")
            ];

            ClassCatalog classCatalog = new(classes, itemCatalog);

            List<EnemyDefinition> enemies =
            [
                new EnemyDefinition(
                    "Gangster", "Gangster", "Neutral", new Attributes { Strength = 4, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(), "Dagger", "UnknownArmor", 50, 'g',
                    ColorPresets.Enemies.Gangster,
                    new List<EnemyLootEntry>(), new List<EnemyAbilityDefinition>())
            ];

            EnemyCatalog enemyCatalog = new(enemies);
            return new GameCatalog(itemCatalog, classCatalog, enemyCatalog);
        }

        private sealed class TestDrawingContext(int width, int height) : IDrawingContext
        {
            public Viewport Viewport { get; } = new(0, 0, width, height);
            public void DrawAt((int X, int Y) pos, char tile) { }
            public void DrawAt((int X, int Y) pos, Cell cell) { }
            public void Clear() { }
            public void Render() { }
            public void AttachTo(View parent) { }
        }
    }
}