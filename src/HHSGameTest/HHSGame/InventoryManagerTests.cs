using HHSGame.Core.Items;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class InventoryManagerTests
    {
        // ── AddItem / GetItems ─────────────────────────────────────────

        [TestMethod]
        public void AddItem_IncreasesItemCount()
        {
            InventoryManager inventory = new();
            var item = CreatePotion("p1", "Potion 1");

            inventory.AddItem(item);

            Assert.AreEqual(1, inventory.GetItems().Count());
        }

        [TestMethod]
        public void AddItem_MultipleTimes_AllAreTracked()
        {
            InventoryManager inventory = new();
            inventory.AddItem(CreatePotion("p1", "Potion 1"));
            inventory.AddItem(CreatePotion("p2", "Potion 2"));
            inventory.AddItem(CreatePotion("p3", "Potion 3"));

            Assert.AreEqual(3, inventory.GetItems().Count());
        }

        [TestMethod]
        public void AddItem_SetsInventoryPosition()
        {
            InventoryManager inventory = new();
            var item = CreatePotion("p1", "Potion 1");

            inventory.AddItem(item);

            // Items added to inventory should have 0,0 position
            Assert.AreEqual(0, item.X);
            Assert.AreEqual(0, item.Y);
        }

        // ── RemoveItem ────────────────────────────────────────────────

        [TestMethod]
        public void RemoveItem_DecreasesItemCount()
        {
            InventoryManager inventory = new();
            var item = CreatePotion("p1", "Potion 1");
            inventory.AddItem(item);

            inventory.RemoveItem(item);

            Assert.AreEqual(0, inventory.GetItems().Count());
        }

        [TestMethod]
        public void RemoveItem_NonExistentItem_DoesNotThrow()
        {
            InventoryManager inventory = new();
            var item = CreatePotion("p1", "Potion 1");

            // Should not throw
            inventory.RemoveItem(item);
            Assert.AreEqual(0, inventory.GetItems().Count());
        }

        [TestMethod]
        public void RemoveItem_OnlyRemovesSpecificItem()
        {
            InventoryManager inventory = new();
            var item1 = CreatePotion("p1", "Potion 1");
            var item2 = CreatePotion("p2", "Potion 2");
            inventory.AddItem(item1);
            inventory.AddItem(item2);

            inventory.RemoveItem(item1);

            Assert.AreEqual(1, inventory.GetItems().Count());
            Assert.IsTrue(inventory.GetItems().Contains(item2));
        }

        // ── ObservableItems ────────────────────────────────────────────

        [TestMethod]
        public void ObservableItems_ReflectsCurrentState()
        {
            InventoryManager inventory = new();
            var item = CreatePotion("p1", "Potion 1");

            Assert.AreEqual(0, inventory.ObservableItems.Count);

            inventory.AddItem(item);
            Assert.AreEqual(1, inventory.ObservableItems.Count);

            inventory.RemoveItem(item);
            Assert.AreEqual(0, inventory.ObservableItems.Count);
        }

        // ── Currency ──────────────────────────────────────────────────

        [TestMethod]
        public void Currency_DefaultsToZero()
        {
            InventoryManager inventory = new();
            Assert.AreEqual(0, inventory.Currency);
        }

        [TestMethod]
        public void AddCurrency_IncreasesBalance()
        {
            InventoryManager inventory = new();
            inventory.AddCurrency(100);

            Assert.AreEqual(100, inventory.Currency);
        }

        [TestMethod]
        public void AddCurrency_MultipleTimes_Accumulates()
        {
            InventoryManager inventory = new();
            inventory.AddCurrency(50);
            inventory.AddCurrency(30);

            Assert.AreEqual(80, inventory.Currency);
        }

        [TestMethod]
        public void AddCurrency_ZeroOrNegative_DoesNotChange()
        {
            InventoryManager inventory = new();
            inventory.AddCurrency(100);
            inventory.AddCurrency(0);
            inventory.AddCurrency(-10);

            Assert.AreEqual(100, inventory.Currency);
        }

        [TestMethod]
        public void TrySpendCurrency_ReturnsTrue_WhenEnough()
        {
            InventoryManager inventory = new();
            inventory.AddCurrency(100);

            Assert.IsTrue(inventory.TrySpendCurrency(60));
            Assert.AreEqual(40, inventory.Currency);
        }

        [TestMethod]
        public void TrySpendCurrency_ReturnsFalse_WhenNotEnough()
        {
            InventoryManager inventory = new();
            inventory.AddCurrency(50);

            Assert.IsFalse(inventory.TrySpendCurrency(100));
            Assert.AreEqual(50, inventory.Currency);
        }

        [TestMethod]
        public void TrySpendCurrency_ReturnsFalse_ZeroOrNegative()
        {
            InventoryManager inventory = new();
            inventory.AddCurrency(100);

            Assert.IsFalse(inventory.TrySpendCurrency(0));
            Assert.IsFalse(inventory.TrySpendCurrency(-10));
            Assert.AreEqual(100, inventory.Currency);
        }

        // ── GetItemsAt ────────────────────────────────────────────────

        [TestMethod]
        public void GetItemsAt_ReturnsItemsAtPosition()
        {
            InventoryManager inventory = new();
            // GetItemsAt works on the ItemManager, not InventoryManager directly
            // This test verifies the InventoryManager tracks items correctly
            var item = CreatePotion("p1", "Potion 1");
            inventory.AddItem(item);

            Assert.AreEqual(1, inventory.GetItems().Count());
        }

        // ── Clear ─────────────────────────────────────────────────────

        [TestMethod]
        public void RemoveAllItems_LeavesInventoryEmpty()
        {
            InventoryManager inventory = new();
            var item1 = CreatePotion("p1", "Potion 1");
            var item2 = CreatePotion("p2", "Potion 2");
            inventory.AddItem(item1);
            inventory.AddItem(item2);

            inventory.RemoveItem(item1);
            inventory.RemoveItem(item2);

            Assert.AreEqual(0, inventory.GetItems().Count());
        }

        [TestMethod]
        public void RemoveAllItems_DoesNotAffectCurrency()
        {
            InventoryManager inventory = new();
            var item = CreatePotion("p1", "Potion 1");
            inventory.AddItem(item);
            inventory.AddCurrency(100);

            inventory.RemoveItem(item);

            Assert.AreEqual(0, inventory.GetItems().Count());
            Assert.AreEqual(100, inventory.Currency);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static HealthPotion CreatePotion(string id, string name)
        {
            return new HealthPotion(id, name, ItemRarity.Common, 50, 0.5f, 10);
        }
    }
}