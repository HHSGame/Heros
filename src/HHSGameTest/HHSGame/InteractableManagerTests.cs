using HHSGame.Core;
using HHSGame.Core.Interactions;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Combat;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Enemies;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.UI;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class InteractableManagerTests
    {
        [TestMethod]
        public void GetAt_ReturnsNull_WhenEmpty()
        {
            InteractableManager manager = new();
            Assert.IsNull(manager.GetAt(new Coordinate(0, 0)));
        }

        [TestMethod]
        public void GetAt_ReturnsObject_AtPosition()
        {
            InteractableManager manager = new();
            var obj = new TestInteractable("test-1", "Test", new Coordinate(5, 5));
            manager.Add(obj);
            Assert.AreEqual(obj, manager.GetAt(new Coordinate(5, 5)));
        }

        [TestMethod]
        public void GetAt_ReturnsNull_AtWrongPosition()
        {
            InteractableManager manager = new();
            var obj = new TestInteractable("test-1", "Test", new Coordinate(5, 5));
            manager.Add(obj);
            Assert.IsNull(manager.GetAt(new Coordinate(6, 6)));
        }

        [TestMethod]
        public void GetNearby_ReturnsEmpty_WhenEmpty()
        {
            InteractableManager manager = new();
            Assert.AreEqual(0, manager.GetNearby(new Coordinate(0, 0), 2).Count);
        }

        [TestMethod]
        public void GetNearby_ReturnsObjects_WithinRadius()
        {
            InteractableManager manager = new();
            manager.Add(new TestInteractable("a", "A", new Coordinate(5, 5)));
            manager.Add(new TestInteractable("b", "B", new Coordinate(6, 6)));
            manager.Add(new TestInteractable("c", "C", new Coordinate(20, 20)));
            var nearby = manager.GetNearby(new Coordinate(5, 5), 2);
            Assert.AreEqual(2, nearby.Count);
        }

        [TestMethod]
        public void GetInteractableNearPlayer_ReturnsOnlyInteractable()
        {
            InteractableManager manager = new();
            manager.Add(new TestInteractable("a", "A", new Coordinate(5, 5), canInteract: true));
            manager.Add(new TestInteractable("b", "B", new Coordinate(5, 6), canInteract: false));
            var result = manager.GetInteractableNearPlayer(new Coordinate(5, 5));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("a", result[0].Id);
        }

        [TestMethod]
        public void Clear_RemovesAll()
        {
            InteractableManager manager = new();
            manager.Add(new TestInteractable("a", "A", new Coordinate(5, 5)));
            manager.Add(new TestInteractable("b", "B", new Coordinate(6, 6)));
            manager.Clear();
            Assert.AreEqual(0, manager.All.Count);
        }

        [TestMethod]
        public void ExamineAt_ReturnsResult_WhenObjectExists()
        {
            InteractableManager manager = new();
            var obj = new TestInteractable("a", "Test", new Coordinate(5, 5), description: "A test object.");
            manager.Add(obj);
            var result = manager.ExamineAt(new Coordinate(5, 5), CreatePlayer());
            Assert.IsNotNull(result);
            Assert.IsTrue(result!.Success);
        }

        [TestMethod]
        public void ExamineAt_ReturnsNull_WhenNoObject()
        {
            InteractableManager manager = new();
            var result = manager.ExamineAt(new Coordinate(5, 5), CreatePlayer());
            Assert.IsNull(result);
        }

        [TestMethod]
        public void InteractAt_ReturnsNull_WhenNoObject()
        {
            InteractableManager manager = new();
            var result = manager.InteractAt(new Coordinate(5, 5), CreatePlayer());
            Assert.IsNull(result);
        }

        [TestMethod]
        public void InteractionResult_DefaultValues()
        {
            var result = new InteractionResult();
            Assert.IsFalse(result.Success);
            Assert.AreEqual(string.Empty, result.Message);
        }

        private static Player CreatePlayer()
        {
            return new Player(5, 5, CreateContext());
        }

        private static GameContext CreateContext()
        {
            var parameters = new GameParameters { MapStyle = MapStyle.UrbanStreet, MapWidth = 20, MapHeight = 20 };
            var random = new Random(1234);
            var itemManager = new ItemManager();
            var mapState = new MapState();
            var inventoryManager = new InventoryManager();
            var partyState = new PartyState();
            var weapons = new List<WeaponDefinition>
            {
                new("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
            };
            var armors = new List<ArmorDefinition> { new("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0) };
            var catalog = new ItemCatalog(weapons, armors, []);
            var questManager = new QuestManager(partyState, inventoryManager, catalog);
            var enemyManager = new EnemyManager(itemManager, questManager);
            var npcManager = new NpcManager();
            var collisionSystem = new CollisionSystem(enemyManager, mapState, npcManager);
            var pathfinder = new Pathfinder(mapState);
            var enemyFactory = new EnemyFactory(new EnemyCatalog([]), catalog, collisionSystem, mapState, pathfinder, new global::HHSGame.Core.Factions.FactionManager());
            var npcFactory = new NpcFactory(catalog);
            var factionManager = new global::HHSGame.Core.Factions.FactionManager();
            var dialogueManager = new DialogueManager(partyState, questManager, factionManager);
            var surroundingsManager = new SurroundingsManager(itemManager, enemyManager, mapState, npcManager);
            var interactableManager = new InteractableManager();
            var turnManager = new TurnManager();
            var stateMachine = new GameStateMachine();
            var drawingContext = new TestDrawingContext(20, 20);
            return new GameContext(parameters, random, enemyFactory, collisionSystem, mapState, enemyManager, itemManager, catalog, npcFactory, npcManager, surroundingsManager, inventoryManager, interactableManager, turnManager, stateMachine, questManager, dialogueManager, partyState, new global::HHSGame.Core.Factions.FactionManager(), drawingContext);
        }

        private sealed class TestInteractable : IInteractable
        {
            public string Id { get; }
            public string Name { get; }
            public string Description { get; }
            public char Glyph { get; } = '?';
            public Coordinate Position { get; }
            public bool IsInteracted { get; private set; }
            public bool CanInteract { get; }
            public TestInteractable(string id, string name, Coordinate position, string description = "", bool canInteract = true)
            {
                Id = id; Name = name; Position = position;
                Description = string.IsNullOrEmpty(description) ? $"A {name}." : description;
                CanInteract = canInteract;
            }
            public InteractionResult Examine(Player player) => new() { Success = true, Message = Description };
            public InteractionResult Interact(Player player) { IsInteracted = true; return new() { Success = true, Message = $"Used {Name}." }; }
        }

        private sealed class TestDrawingContext : IDrawingContext
        {
            public Viewport Viewport { get; }
            public TestDrawingContext(int w, int h) { Viewport = new Viewport(0, 0, w, h); }
            public void DrawAt((int X, int Y) pos, char tile) { }
            public void DrawAt((int X, int Y) pos, Cell cell) { }
            public void Clear() { }
            public void Render() { }
            public void AttachTo(Terminal.Gui.ViewBase.View parent) { }
        }
    }
}