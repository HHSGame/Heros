using Terminal.Gui;
using HHSGame.Core;
using HHSGame.Core.Items;

namespace HHSGame.UI.Windows
{
    public class UtilityWindow : Window
    {
        private readonly InventoryManager inventoryManager;
        private readonly Lazy<Player> playerHolder;
        private readonly ListView listView;
        private readonly MapWindow mapWindow;

        public UtilityWindow(GameContext context, MapWindow mapWindow) : base(GUISettings.UtilityWindowTitle)
        {
            inventoryManager = context.InventoryManager;
            playerHolder = new(() => context.Player);
            X = Pos.Right(mapWindow);
            Y = 0;
            Height = Dim.Fill() - GUISettings.MessageWindowHeight;
            Visible = false;
            this.mapWindow = mapWindow;

            listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            listView.SetSource(inventoryManager.Items);

            Add(listView);

            listView.OpenSelectedItem += HandleOpenSelectedItem;

            EventSystem.OnInventoryChange += HandleInventoryChange;
        }

        private void HandleOpenSelectedItem(ListViewItemEventArgs args)
        {
            var index = args.Item;
            var item = inventoryManager.Items[index];
            item.Use(playerHolder.Value);
            inventoryManager.RemoveItem(item);
        }

        private void HandleInventoryChange(object? sender, InventoryChangeEventArgs e)
        {
            listView.SetSource(inventoryManager.Items);
        }

        public void ToggleUtilityWindow()
        {
            if (!Visible)
            {
                Visible = true;
                mapWindow.Width = Dim.Percent(75);
                mapWindow.SetNeedsDisplay();
                Width = Dim.Percent(25);
                this.SetNeedsDisplay();
            }
            else
            {
                Visible = false;
                mapWindow.Width = Dim.Fill() - 20;
                mapWindow.SetNeedsDisplay();
                this.SetNeedsDisplay();
            }
        }
    }
}
