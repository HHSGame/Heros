using System.Collections.ObjectModel;
using HHSGame.Utils;

namespace HHSGame.Core.Items
{
    public class InventoryManager
    {
        public ObservableCollection<Item> ObservableItems { get; private set; }
        public int Count => ObservableItems.Count;
        public int Currency { get; private set; }

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

        public void AddCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Currency += amount;
        }

        public bool TrySpendCurrency(int amount)
        {
            if (amount <= 0 || Currency < amount)
            {
                return false;
            }

            Currency -= amount;
            return true;
        }

        public ObservableCollection<Item> GetItems()
        {
            return this.ObservableItems;
        }
    }
}
