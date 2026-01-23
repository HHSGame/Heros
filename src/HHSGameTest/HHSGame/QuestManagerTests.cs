using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;
using HHSGame.Core.Quests;

namespace HHSGame.Core
{
    [TestClass]
    public class QuestManagerTests
    {
        [TestMethod]
        public void CollectItemQuestCompletesAndRewardsCurrency()
        {
            InventoryManager inventoryManager = new();
            PartyState partyState = new();
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

            QuestManager questManager = new(partyState, inventoryManager, itemCatalog);
            QuestDefinition quest = new(
                "missing-supply",
                "Missing Supply",
                "Find a potion.",
                string.Empty,
                new List<QuestObjectiveDefinition>
                {
                    new(QuestObjectiveKind.CollectItem, "HealthPotion", 1)
                },
                new List<QuestRewardDefinition>
                {
                    new(QuestRewardKind.Currency, string.Empty, 10)
                });

            questManager.LoadDefinitions(new[] { quest }, Array.Empty<AchievementDefinition>());

            Assert.IsTrue(questManager.StartQuest("missing-supply"));

            questManager.NotifyItemCollected("HealthPotion", 1);

            QuestState state = questManager.Quests.Single(q => q.Definition.Id == "missing-supply");
            Assert.AreEqual(QuestStatus.Active, state.Status);
            Assert.IsTrue(questManager.CompleteQuest("missing-supply"));
            Assert.AreEqual(QuestStatus.Completed, state.Status);
            Assert.AreEqual(10, inventoryManager.Currency);
        }

        [TestMethod]
        public void AchievementUnlocksOnTalkObjective()
        {
            InventoryManager inventoryManager = new();
            PartyState partyState = new();
            ItemCatalog itemCatalog = new(
                new List<WeaponDefinition>
                {
                    new WeaponDefinition("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
                },
                new List<ArmorDefinition>
                {
                    new ArmorDefinition("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0)
                },
                new List<ItemDefinition>());

            QuestManager questManager = new(partyState, inventoryManager, itemCatalog);
            AchievementDefinition achievement = new(
                "first-contact",
                "First Contact",
                "Speak with a fixer.",
                new List<QuestObjectiveDefinition>
                {
                    new(QuestObjectiveKind.TalkToNpc, "Fixer", 1)
                });

            questManager.LoadDefinitions(Array.Empty<QuestDefinition>(), new[] { achievement });

            questManager.NotifyNpcTalked("Fixer");

            AchievementState state = questManager.Achievements.Single(a => a.Definition.Id == "first-contact");
            Assert.IsTrue(state.IsUnlocked);
        }
    }
}
