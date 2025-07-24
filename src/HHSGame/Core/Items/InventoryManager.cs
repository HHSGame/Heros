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
            Events.RaiseGameMessage(I18n.T("PickedUpItem", item.Name));
            Events.RaiseInventoryChange(InventoryEventType.PickUp, item);
        }

        public void RemoveItem(Item item)
        {
            Events.RaiseInventoryChange(InventoryEventType.Remove, item);
            ObservableItems.Remove(item);
        }

        public ObservableCollection<Item> GetItems()
        {
            return this.ObservableItems;
        }
    }
}