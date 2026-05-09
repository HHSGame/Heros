using HHSGame.Core.Interactions;
using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Enemies;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.SkillActions;
using HHSGame.Core.Stats;
using HHSGame.UI;
using Microsoft.Extensions.Logging.Abstractions;
using Terminal.Gui.ViewBase;

namespace HHSGame.Core
{
    [TestClass]
    public class SkillActionTests
    {
        [TestMethod]
        public void InspireLocksFurtherSkillActions()
        {
            Game game = CreateGame();
            Player player = game.Player!;

            game.Context.TurnManager.BeginPlayerTurn();
            game.Context.StateMachine.TryChangeState(GameStateType.Exploration);
            Assert.IsTrue(game.TryUseSkillAction(SkillActionCatalog.Get(SkillActionId.Inspire), new SkillActionTarget()));
            Assert.IsTrue(game.Context.PartyState.InspireActive);
            Assert.IsTrue(player.IsSkillLocked);

            game.Context.TurnManager.BeginPlayerTurn();
            game.Context.StateMachine.TryChangeState(GameStateType.Exploration);
            Assert.IsFalse(game.TryUseSkillAction(SkillActionCatalog.Get(SkillActionId.Inspect), new SkillActionTarget()));
        }

        [TestMethod]
        public void SearchRevealsHiddenItems()
        {
            Game game = CreateGame();
            Player player = game.Player!;

            Assert.IsTrue(game.Context.ItemCatalog.TryCreateItem("HealthPotion", out Item hiddenItem));
            hiddenItem.X = player.X;
            hiddenItem.Y = player.Y;
            hiddenItem.IsHidden = true;
            game.Context.ItemManager.AddItem(hiddenItem);

            game.Context.TurnManager.BeginPlayerTurn();
            game.Context.StateMachine.TryChangeState(GameStateType.Exploration);
            Assert.IsTrue(game.TryUseSkillAction(SkillActionCatalog.Get(SkillActionId.Search), new SkillActionTarget()));
            Assert.IsFalse(hiddenItem.IsHidden);
        }

        [TestMethod]
        public void SneakPreventsLowAwarenessDetection()
        {
            Game game = CreateGame();
            Player player = game.Player!;
            player.Stats.Skills.SetRank(SkillType.Stealth, 10);

            Enemy enemy = game.Context.EnemyFactory.CreateEnemy("Gangster", player.X + 1, player.Y);
            game.Context.EnemyManager.SetEnemies(new List<Enemy> { enemy });

            game.Context.TurnManager.BeginPlayerTurn();
            game.Context.StateMachine.TryChangeState(GameStateType.Combat);
            player.ResetTurn(true, true);
            Assert.IsTrue(game.TryUseSkillAction(SkillActionCatalog.Get(SkillActionId.Sneak), new SkillActionTarget()));
            game.CommitPlayerActions();
            Assert.IsTrue(player.IsSneaking);
            Assert.IsFalse(enemy.CanDetectPlayer(player));
        }

        private static Game CreateGame()
        {
            GameParameters parameters = new()
            {
                MapStyle = MapStyle.Cave,
                MapWidth = 10,
                MapHeight = 10
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
                interactableManager,
                turnManager,
                stateMachine,
                questManager,
                dialogueManager,
                partyState,
                new global::HHSGame.Core.Factions.FactionManager(),
                drawingContext);

            GameWorld world = new(context);
            Game game = new(NullLogger<Game>.Instance, context, world, catalogs.ClassCatalog, new GameConfig());
            game.Start();
            return game;
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
                new ClassDefinition(
                    "Warrior",
                    "Warrior",
                    new Attributes { Strength = 7, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(),
                    "Dagger",
                    "UnknownArmor")
            ];

            ClassCatalog classCatalog = new(classes, itemCatalog);

            List<EnemyDefinition> enemies =
            [
                new EnemyDefinition(
                    "Gangster", "Gangster", "Neutral", new Attributes { Strength = 4, Perception = 3, Agility = 4, Charisma = 4, Intelligence = 3 },
                    new Skills(),
                    "Dagger",
                    "UnknownArmor",
                    10,
                    'g',
                    ColorPresets.Enemies.Gangster,
                    new List<EnemyLootEntry>(),
                    new List<EnemyAbilityDefinition>())
            ];

            EnemyCatalog enemyCatalog = new(enemies);

            return new GameCatalog(itemCatalog, classCatalog, enemyCatalog);
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
