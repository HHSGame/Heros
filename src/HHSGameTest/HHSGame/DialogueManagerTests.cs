using HHSGame.Core.Interactions;
using HHSGame.Core.Combat;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Enemies;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.Stats;
using HHSGame.UI;
using Terminal.Gui.ViewBase;

namespace HHSGame.Core
{
    [TestClass]
    public class DialogueManagerTests
    {
        [TestMethod]
        public void DialogueAssistBonusUnlocksRequirement()
        {
            GameContext context = CreateContext(8, 8);
            Player speaker = new(2, 2, context, "Speaker", 'S',
                new Attributes { Strength = 5, Perception = 5, Agility = 5, Charisma = 5, Intelligence = 5 },
                BuildSkills((SkillType.Barter, 2)));
            Player ally = new(2, 3, context, "Ally", 'A',
                new Attributes { Strength = 5, Perception = 5, Agility = 5, Charisma = 5, Intelligence = 5 },
                BuildSkills((SkillType.Barter, 6)));

            context.InitializeContext(new[] { speaker }, speaker);
            DialogueManager dialogueManager = context.DialogueManager;

            DialogueDefinition definition = BuildDialogueDefinition(9);
            dialogueManager.LoadDefinitions(new[] { definition });

            Npc npc = new(
                "Fixer",
                "Fixer",
                'F',
                ColorPresets.Npcs.Trader,
                3,
                2,
                new Attributes { Strength = 4, Perception = 4, Agility = 4, Charisma = 5, Intelligence = 4 },
                new Skills(),
                context.ItemCatalog,
                "fixer-dialogue");

            Assert.IsTrue(dialogueManager.TryStartDialogue(npc));
            Assert.AreEqual(0, dialogueManager.CurrentSession!.Options.Count);

            context.PartyState.SetPlayers(new[] { speaker, ally }, speaker);
            Assert.IsTrue(dialogueManager.TryStartDialogue(npc));
            Assert.AreEqual(1, dialogueManager.CurrentSession!.Options.Count);
        }

        [TestMethod]
        public void DialogueCanStartAndCompleteQuestInOneOption()
        {
            GameContext context = CreateContext(8, 8);
            Player speaker = new(2, 2, context, "Speaker", 'S',
                new Attributes { Strength = 5, Perception = 5, Agility = 5, Charisma = 5, Intelligence = 5 },
                new Skills());

            context.InitializeContext(new[] { speaker }, speaker);

            QuestDefinition quest = new(
                "missing-supply",
                "Missing Supply",
                "Find a potion.",
                "Potion around (2,2).",
                new[]
                {
                    new QuestObjectiveDefinition(QuestObjectiveKind.CollectItem, "HealthPotion", 1)
                },
                Array.Empty<QuestRewardDefinition>());

            context.QuestManager.LoadDefinitions(new[] { quest }, Array.Empty<AchievementDefinition>());

            DialogueDefinition definition = BuildImmediateCompletionDialogue();
            context.DialogueManager.LoadDefinitions(new[] { definition });

            Npc npc = new(
                "Fixer",
                "Fixer",
                'F',
                ColorPresets.Npcs.Trader,
                3,
                2,
                new Attributes { Strength = 4, Perception = 4, Agility = 4, Charisma = 5, Intelligence = 4 },
                new Skills(),
                context.ItemCatalog,
                "fixer-dialogue");

            Assert.IsTrue(context.DialogueManager.TryStartDialogue(npc));
            Assert.AreEqual(1, context.DialogueManager.CurrentSession!.Options.Count);

            Assert.IsTrue(context.DialogueManager.TrySelectOption(0));
            Assert.IsTrue(context.QuestManager.IsQuestInStatus("missing-supply", QuestStatus.Completed));
        }

        private static Skills BuildSkills(params (SkillType Skill, int Rank)[] ranks)
        {
            Skills skills = new();
            foreach ((SkillType skill, int rank) in ranks)
            {
                skills.SetRank(skill, rank);
            }

            return skills;
        }

        private static DialogueDefinition BuildDialogueDefinition(int minimumBarter)
        {
            DialogueOption option = new(
                "Pay me more.",
                null,
                new[]
                {
                    new DialogueRequirement(DialogueRequirementType.Skill, SkillType.Barter, null, null, null, minimumBarter)
                },
                Array.Empty<DialogueEffect>());

            DialogueNode node = new("start", "Test node.", new[] { option });
            return new DialogueDefinition("fixer-dialogue", "start", new Dictionary<string, DialogueNode>
            {
                ["start"] = node
            });
        }

        private static DialogueDefinition BuildImmediateCompletionDialogue()
        {
            DialogueOption option = new(
                "I can solve it with knowledge.",
                null,
                Array.Empty<DialogueRequirement>(),
                new[]
                {
                    new DialogueEffect(DialogueEffectType.StartQuest, "missing-supply", 0),
                    new DialogueEffect(DialogueEffectType.GiveItem, "HealthPotion", 1),
                    new DialogueEffect(DialogueEffectType.CompleteQuest, "missing-supply", 0)
                });

            DialogueNode node = new("start", "Test node.", new[] { option });
            return new DialogueDefinition("fixer-dialogue", "start", new Dictionary<string, DialogueNode>
            {
                ["start"] = node
            });
        }

        private static GameContext CreateContext(int width, int height)
        {
            GameParameters parameters = new()
            {
                MapStyle = MapStyle.Cave,
                MapWidth = width,
                MapHeight = height
            };

            MapState mapState = new();
            mapState.Init(MapLoader.CreateEmptyMap(width, height));

            ItemCatalog itemCatalog = new(
                new List<WeaponDefinition>
                {
                    new WeaponDefinition("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
                },
                new List<ArmorDefinition>
                {
                    new ArmorDefinition("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0)
                },
                new List<ItemDefinition>
                {
                    new ItemDefinition("HealthPotion", "HealthPotion", "Health Potion", ItemRarity.Common, 50, 0.5f, 10)
                });

            EnemyCatalog enemyCatalog = new(new List<EnemyDefinition>());
            ItemManager itemManager = new();
            InventoryManager inventoryManager = new();
            PartyState partyState = new();
            QuestManager questManager = new(partyState, inventoryManager, itemCatalog);
            EnemyManager enemyManager = new(itemManager, questManager);
            NpcManager npcManager = new();
            CollisionSystem collisionSystem = new(enemyManager, mapState, npcManager);
            Pathfinder pathfinder = new(mapState);
            EnemyFactory enemyFactory = new(enemyCatalog, itemCatalog, collisionSystem, mapState, pathfinder, new global::HHSGame.Core.Factions.FactionManager());
            NpcFactory npcFactory = new(itemCatalog);
            DialogueManager dialogueManager = new(partyState, questManager);
            SurroundingsManager surroundingsManager = new(itemManager, enemyManager, mapState, npcManager);
            TurnManager turnManager = new();
            GameStateMachine stateMachine = new();
            IDrawingContext drawingContext = new TestDrawingContext(width, height);

            return new GameContext(
                parameters,
                new Random(123),
                enemyFactory,
                collisionSystem,
                mapState,
                enemyManager,
                itemManager,
                itemCatalog,
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
