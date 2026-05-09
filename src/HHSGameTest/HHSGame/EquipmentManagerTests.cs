using HHSGame.Core.Items;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class EquipmentManagerTests
    {
        private static InventoryManager CreateInventory()
        {
            return new InventoryManager();
        }

        private static Armor CreateArmor(string id, int armorValue, EquipmentSlot slot)
        {
            return new Armor(id, id, ItemRarity.Common, 100, 1.0f, armorValue, slot);
        }

        // ── Equip/Unequip ───────────────────────────────────────────────

        [TestMethod]
        public void Equip_AddsArmorToSlot()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);

            equipment.Equip(helmet);

            Assert.AreEqual(helmet, equipment.GetEquipped(EquipmentSlot.Head));
        }

        [TestMethod]
        public void Equip_ReturnsNull_WhenSlotEmpty()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor body = CreateArmor("LeatherArmor", 3, EquipmentSlot.Body);

            Armor? previous = equipment.Equip(body);

            Assert.IsNull(previous);
        }

        [TestMethod]
        public void Equip_ReturnsPreviousArmor_WhenSlotOccupied()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor leather = CreateArmor("LeatherArmor", 3, EquipmentSlot.Body);
            Armor plate = CreateArmor("PlateArmor", 8, EquipmentSlot.Body);

            equipment.Equip(leather);
            Armor? previous = equipment.Equip(plate);

            Assert.AreEqual(leather, previous);
            Assert.AreEqual(plate, equipment.GetEquipped(EquipmentSlot.Body));
        }

        [TestMethod]
        public void Equip_ReturnsPreviousArmorToInventory()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor leather = CreateArmor("LeatherArmor", 3, EquipmentSlot.Body);
            Armor plate = CreateArmor("PlateArmor", 8, EquipmentSlot.Body);

            equipment.Equip(leather);
            equipment.Equip(plate);

            Assert.IsTrue(inventory.ObservableItems.Contains(leather));
        }

        [TestMethod]
        public void Unequip_RemovesArmorFromSlot()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);

            equipment.Equip(helmet);
            Armor? removed = equipment.Unequip(EquipmentSlot.Head);

            Assert.AreEqual(helmet, removed);
            Assert.IsNull(equipment.GetEquipped(EquipmentSlot.Head));
        }

        [TestMethod]
        public void Unequip_ReturnsNull_WhenSlotEmpty()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);

            Armor? removed = equipment.Unequip(EquipmentSlot.Head);

            Assert.IsNull(removed);
        }

        [TestMethod]
        public void Unequip_ReturnsArmorToInventory()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);

            equipment.Equip(helmet);
            equipment.Unequip(EquipmentSlot.Head);

            Assert.IsTrue(inventory.ObservableItems.Contains(helmet));
        }

        // ── Total Armor ─────────────────────────────────────────────────

        [TestMethod]
        public void GetArmorValue_ReturnsZero_WhenEmpty()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);

            Assert.AreEqual(0, equipment.GetArmorValue());
        }

        [TestMethod]
        public void GetArmorValue_SumsAllEquippedSlots()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);
            Armor body = CreateArmor("LeatherArmor", 5, EquipmentSlot.Body);
            Armor legs = CreateArmor("Leggings", 2, EquipmentSlot.Legs);

            equipment.Equip(helmet);
            equipment.Equip(body);
            equipment.Equip(legs);

            Assert.AreEqual(10, equipment.GetArmorValue());
        }

        [TestMethod]
        public void GetArmorValue_UpdatesAfterUnequip()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);
            Armor body = CreateArmor("LeatherArmor", 5, EquipmentSlot.Body);

            equipment.Equip(helmet);
            equipment.Equip(body);
            Assert.AreEqual(8, equipment.GetArmorValue());

            equipment.Unequip(EquipmentSlot.Head);
            Assert.AreEqual(5, equipment.GetArmorValue());
        }

        // ── Multiple Slots ──────────────────────────────────────────────

        [TestMethod]
        public void MultipleSlots_CanEquipDifferentSlots()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);
            Armor body = CreateArmor("LeatherArmor", 5, EquipmentSlot.Body);
            Armor ring = CreateArmor("Ring", 1, EquipmentSlot.Accessory1);

            equipment.Equip(helmet);
            equipment.Equip(body);
            equipment.Equip(ring);

            Assert.AreEqual(helmet, equipment.GetEquipped(EquipmentSlot.Head));
            Assert.AreEqual(body, equipment.GetEquipped(EquipmentSlot.Body));
            Assert.AreEqual(ring, equipment.GetEquipped(EquipmentSlot.Accessory1));
        }

        [TestMethod]
        public void AccessorySlots_IndependentOfEachOther()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor ring1 = CreateArmor("Ring1", 1, EquipmentSlot.Accessory1);
            Armor ring2 = CreateArmor("Ring2", 2, EquipmentSlot.Accessory2);

            equipment.Equip(ring1);
            equipment.Equip(ring2);

            Assert.AreEqual(ring1, equipment.GetEquipped(EquipmentSlot.Accessory1));
            Assert.AreEqual(ring2, equipment.GetEquipped(EquipmentSlot.Accessory2));
            Assert.AreEqual(3, equipment.GetArmorValue());
        }

        // ── Slot Occupancy ──────────────────────────────────────────────

        [TestMethod]
        public void IsSlotOccupied_ReturnsFalse_WhenEmpty()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);

            Assert.IsFalse(equipment.IsSlotOccupied(EquipmentSlot.Head));
        }

        [TestMethod]
        public void IsSlotOccupied_ReturnsTrue_WhenEquipped()
        {
            InventoryManager inventory = CreateInventory();
            EquipmentManager equipment = new(inventory);
            Armor helmet = CreateArmor("Helmet", 3, EquipmentSlot.Head);

            equipment.Equip(helmet);

            Assert.IsTrue(equipment.IsSlotOccupied(EquipmentSlot.Head));
        }

        // ── EquipmentSlotRules ──────────────────────────────────────────

        [TestMethod]
        public void ParseSlot_ReturnsCorrectSlots()
        {
            Assert.AreEqual(EquipmentSlot.Head, EquipmentSlotRules.ParseSlot("head"));
            Assert.AreEqual(EquipmentSlot.Body, EquipmentSlotRules.ParseSlot("body"));
            Assert.AreEqual(EquipmentSlot.Legs, EquipmentSlotRules.ParseSlot("legs"));
            Assert.AreEqual(EquipmentSlot.Accessory1, EquipmentSlotRules.ParseSlot("accessory"));
            Assert.AreEqual(EquipmentSlot.Accessory1, EquipmentSlotRules.ParseSlot("accessory1"));
            Assert.AreEqual(EquipmentSlot.Accessory2, EquipmentSlotRules.ParseSlot("accessory2"));
        }

        [TestMethod]
        public void ParseSlot_IsCaseInsensitive()
        {
            Assert.AreEqual(EquipmentSlot.Head, EquipmentSlotRules.ParseSlot("Head"));
            Assert.AreEqual(EquipmentSlot.Body, EquipmentSlotRules.ParseSlot("BODY"));
        }

        [TestMethod]
        public void ParseSlot_ReturnsNull_ForUnknown()
        {
            Assert.IsNull(EquipmentSlotRules.ParseSlot("invalid"));
            Assert.IsNull(EquipmentSlotRules.ParseSlot(null));
        }

        // ── Armor.Slot ──────────────────────────────────────────────────

        [TestMethod]
        public void Armor_DefaultSlot_IsBody()
        {
            Armor armor = new("TestArmor", "Test", ItemRarity.Common, 100, 1.0f, 5);
            Assert.AreEqual(EquipmentSlot.Body, armor.Slot);
        }

        [TestMethod]
        public void Armor_HeadSlot_IsParsed()
        {
            Armor armor = new("Helmet", "Helmet", ItemRarity.Common, 100, 1.0f, 3, EquipmentSlot.Head);
            Assert.AreEqual(EquipmentSlot.Head, armor.Slot);
        }
    }
}