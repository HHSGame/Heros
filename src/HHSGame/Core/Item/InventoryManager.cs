using HHSGame.Services;

namespace HHSGame.Core
{
    public class InventoryManager
    {
        public List<Item> Items { get; private set; }
        public int Count => Items.Count;

        public InventoryManager()
        {
            Items = [];
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
            EventSystem.RaiseGameMessage(LocalizationService.GetString("PickedUpItem", item.Name));
            EventSystem.RaiseInventoryChange(InventoryEventType.PickUp, item);
        }

        public void RemoveItem(Item item)
        {
            EventSystem.RaiseInventoryChange(InventoryEventType.Remove, item);
            Items.Remove(item);
        }
    }
}