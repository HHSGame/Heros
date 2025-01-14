
using HHSGame.Core;
using HHSGame.Core.Items;
using Terminal.Gui;

namespace HHSGame.UI.Views
{

    public class InventoryListView : ListView
    {
        private readonly InventoryManager inventoryManager;
        public InventoryListView(InventoryManager inventoryManager)
        {
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();

            this.inventoryManager = inventoryManager;

            EventSystem.OnInventoryChange += HandleInventoryChange;
        }

        private void HandleInventoryChange(object? sender, InventoryChangeEventArgs e)
        {
            this.SetSource(inventoryManager.Items.Select(i => i.Name).ToList());
        }


    }
}