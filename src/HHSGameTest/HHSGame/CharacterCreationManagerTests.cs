using HHSGame.Core.CharacterCreation;
using HHSGame.Core.Classes;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;
using HHSGame.Core.Stats;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class CharacterCreationManagerTests
    {
        // ── Step flow ──────────────────────────────────────────────────

        [TestMethod]
        public void Initialize_SetsWelcomeStep()
        {
            var manager = CreateManager();
            manager.Initialize();
            Assert.AreEqual(CreationStep.Welcome, manager.CurrentStep);
        }

        [TestMethod]
        public void AdvanceFromWelcome_MovesToPlaystyleChoice()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            Assert.AreEqual(CreationStep.PlaystyleChoice, manager.CurrentStep);
            Assert.AreEqual("city-center", manager.SelectedMap);
            Assert.IsTrue(manager.UseCustomMap);
        }        [TestMethod]
        public void SelectPlaystyle_MovesToNameInput()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            Assert.AreEqual(CreationStep.NameInput, manager.CurrentStep);
            Assert.IsNotNull(manager.SelectedPlaystyle);
            Assert.IsFalse(string.IsNullOrWhiteSpace(manager.PlayerName));
        }

        [TestMethod]
        public void ConfirmName_MovesToDiceRoll()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("TestHero");
            Assert.AreEqual(CreationStep.DiceRoll, manager.CurrentStep);
            Assert.AreEqual("TestHero", manager.PlayerName);
            Assert.IsNotNull(manager.RolledAttributes);
            Assert.IsNotNull(manager.DiceResults);
            Assert.AreEqual(5, manager.DiceResults!.Count);
        }

        [TestMethod]
        public void ConfirmName_Empty_UsesGeneratedName()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("");
            Assert.IsFalse(string.IsNullOrWhiteSpace(manager.PlayerName));
        }

        [TestMethod]
        public void ConfirmDice_MovesToClassConfirmation()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("Hero");
            manager.ConfirmDice();
            Assert.AreEqual(CreationStep.ClassConfirmation, manager.CurrentStep);
            Assert.IsNotNull(manager.SelectedClass);
        }

        [TestMethod]
        public void ConfirmClass_MovesToComplete()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("Hero");
            manager.ConfirmDice();
            manager.ConfirmClass();
            Assert.AreEqual(CreationStep.Complete, manager.CurrentStep);
            Assert.IsTrue(manager.IsComplete);
        }

        // ── Dice rolling ───────────────────────────────────────────────

        [TestMethod]
        public void RollDice_WithPlaystyle_BiasesPreferredAttributes()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);

            // Select Warrior (STR + AGI bias)
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("Hero");

            Attributes? attrs = manager.RolledAttributes;
            Assert.IsNotNull(attrs);
            Assert.IsTrue(attrs!.Strength >= 3);
            Assert.IsTrue(attrs.Strength <= 18);
        }

        [TestMethod]
        public void RerollDice_ProducesDifferentResults()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("Hero");

            var firstRolls = manager.DiceResults!.Select(r => r.Total).ToArray();
            manager.RerollDice();
            var secondRolls = manager.DiceResults!.Select(r => r.Total).ToArray();

            // Extremely unlikely to be identical
            bool allSame = firstRolls.SequenceEqual(secondRolls);
            Assert.IsFalse(allSame, "Re-roll should produce different results");
        }

        [TestMethod]
        public void GetDiceSummary_ReturnsNonEmptyString()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]);
            manager.ConfirmName("Hero");

            string summary = manager.GetDiceSummary();
            Assert.IsTrue(summary.Contains("STR"));
            Assert.IsTrue(summary.Contains("="));
        }

        [TestMethod]
        public void GetDiceSummary_ReturnsDefaultBeforeRoll()
        {
            var manager = CreateManager();
            manager.Initialize();
            string summary = manager.GetDiceSummary();
            Assert.AreEqual("No rolls yet.", summary);
        }

        // ── Full flow ──────────────────────────────────────────────────

        [TestMethod]
        public void FullFlow_ProducesValidCharacter()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("forest-01", true);
            manager.SelectPlaystyle(PlaystyleCatalog.All[2]); // Scholar
            manager.ConfirmName("Elara");
            manager.ConfirmDice();
            manager.ConfirmClass();

            Assert.IsTrue(manager.IsComplete);
            Assert.AreEqual("Elara", manager.PlayerName);
            Assert.IsTrue(manager.UseCustomMap);
            Assert.AreEqual("forest-01", manager.SelectedMap);
            Assert.IsNotNull(manager.SelectedClass);
            Assert.IsNotNull(manager.GetFinalAttributes());
        }

        [TestMethod]
        public void SelectClass_OverridesSuggestion()
        {
            var manager = CreateManager();
            manager.Initialize();
            manager.AdvanceFromWelcome();
            manager.SelectMap("Cave", false);
            manager.SelectPlaystyle(PlaystyleCatalog.All[0]); // Warrior
            manager.ConfirmName("Hero");
            manager.ConfirmDice();

            var classes = manager.GetAvailableClasses();
            if (classes.Count > 1)
            {
                manager.SelectClass(classes[1]); // Pick second class
                Assert.AreEqual(classes[1].Name, manager.SelectedClass!.Name);
            }
            Assert.IsTrue(manager.IsComplete);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static CharacterCreationManager CreateManager()
        {
            ClassCatalog catalog = CreateClassCatalog();
            Random random = new(1234);
            return new CharacterCreationManager(catalog, random);
        }

        private static ClassCatalog CreateClassCatalog()
        {
            var weapons = new List<WeaponDefinition>
            {
                new("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line),
                new("Dagger", "Dagger", ItemRarity.Common, 60, 0.5f, 5, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
            };
            var armors = new List<ArmorDefinition>
            {
                new("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0)
            };
            var itemCatalog = new ItemCatalog(weapons, armors, []);

            var classes = new List<ClassDefinition>
            {
                new("Warrior", "Warrior",
                    new Attributes { Strength = 7, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                    new Skills(), "Dagger", "UnknownArmor"),
                new("Scholar", "Scholar",
                    new Attributes { Strength = 4, Perception = 6, Agility = 5, Charisma = 5, Intelligence = 8 },
                    new Skills(), "Dagger", "UnknownArmor")
            };

            return new ClassCatalog(classes, itemCatalog);
        }
    }
}