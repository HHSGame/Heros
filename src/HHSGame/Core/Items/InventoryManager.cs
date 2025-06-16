using System.Collections.ObjectModel;
using HHSGame.Utils;

namespace HHSGame.Core.Items
{
    public class InventoryManager
    {
        public ObservableCollection<Item> ObservableItems { get; private set; }
        public int Count => ObservableItems.Count;

        public InventoryManager()
        {
            ObservableItems = [];
        }

        public void AddItem(Item item)
        {
            ObservableItems.Add(item);
            EventSystem.RaiseGameMessage(I18n.GetString("PickedUpItem", item.Name));
            EventSystem.RaiseInventoryChange(InventoryEventType.PickUp, item);
        }

        public void RemoveItem(Item item)
        {
            EventSystem.RaiseInventoryChange(InventoryEventType.Remove, item);
            ObservableItems.Remove(item);
        }

        public ObservableCollection<Item> GetItems()
        {
            return this.ObservableItems;
        }
    }
}