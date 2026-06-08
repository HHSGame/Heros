using HHSGame.Core.Stats;

namespace HHSGame.Core.Items
{
    /// <summary>
    /// Manages equipment slots for a player. Each slot can hold one armor piece.
    /// </summary>
    public sealed class EquipmentManager
    {
        private readonly Dictionary<EquipmentSlot, Armor> equipped = new();
        private readonly InventoryManager inventory;

        public EquipmentManager(InventoryManager inventory)
        {
            this.inventory = inventory;
        }

        public IReadOnlyDictionary<EquipmentSlot, Armor> Equipped => equipped;

        public int TotalArmorValue => EquipmentSlotRules.GetTotalArmor(equipped);

        /// <summary>
        /// Equips armor in its designated slot. Returns the previously equipped armor (if any).
        /// </summary>
        public Armor? Equip(Armor armor)
        {
            Armor? previous = null;
            if (equipped.TryGetValue(armor.Slot, out Armor? existing))
            {
                inventory.AddItem(existing);
                previous = existing;
            }
            equipped[armor.Slot] = armor;
            return previous;
        }

        /// <summary>
        /// Gets the armor in a specific slot, or null if empty.
        /// </summary>
        public Armor? GetEquipped(EquipmentSlot slot)
        {
            return equipped.TryGetValue(slot, out Armor? armor) ? armor : null;
        }

        /// <summary>
        /// Returns the total armor value from all equipped armor.
        /// </summary>
        public int GetArmorValue()
        {
            int total = 0;
            foreach (Armor armor in equipped.Values)
            {
                total += armor.ArmorValue;
            }
            return total;
        }

        /// <summary>
        /// Unequips armor from a slot and adds it to inventory.
        /// </summary>
        public Armor? Unequip(EquipmentSlot slot)
        {
            if (equipped.TryGetValue(slot, out Armor? armor))
            {
                equipped.Remove(slot);
                inventory.AddItem(armor);
                return armor;
            }
            return null;
        }

        /// <summary>
        /// Checks if a specific slot has armor equipped.
        /// </summary>
        public bool IsSlotOccupied(EquipmentSlot slot)
        {
            return equipped.ContainsKey(slot);
        }

        /// <summary>
        /// Gets all occupied equipment slots.
        /// </summary>
        public IEnumerable<EquipmentSlot> GetOccupiedSlots()
        {
            return equipped.Keys;
        }

        /// <summary>
        /// Restores equipment state from save data (used by LoadManager).
        /// </summary>
        public void RestoreEquipment(EquipmentSlot slot, Armor armor)
        {
            equipped[slot] = armor;
        }

        /// <summary>
        /// Clears all equipped items (used during save/load reset).
        /// </summary>
        public void Clear()
        {
            equipped.Clear();
        }
    }
}