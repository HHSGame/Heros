using Terminal.Gui;
using HHSGame.Core;
using HHSGame.Core.Items;

namespace HHSGame.UI.Windows
{
    public class UtilityWindow : Window
    {
        private readonly InventoryManager _inventoryManager;
        private readonly Lazy<Player> _playerHolder;
        private readonly ListView _listView;
        private readonly MapWindow _mapWindow;

        public UtilityWindow(GameContext context, MapWindow mapWindow) : base(GUISettings.UtilityWindowTitle)
        {
            _inventoryManager = context.InventoryManager;
            _playerHolder = new(() => context.Player);
            X = Pos.Right(mapWindow);
            Y = 0;
            Height = Dim.Fill() - GUISettings.MessageWindowHeight;
            Visible = false;
            _mapWindow = mapWindow;

            _listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            _listView.SetSource(_inventoryManager.Items);

            Add(_listView);

            _listView.OpenSelectedItem += HandleOpenSelectedItem;

            EventSystem.OnInventoryChange += HandleInventoryChange;
        }

        private void HandleOpenSelectedItem(ListViewItemEventArgs args)
        {
            var index = args.Item;
            var item = _inventoryManager.Items[index];
            item.Use(_playerHolder.Value);
            _inventoryManager.RemoveItem(item);
        }

        private void HandleInventoryChange(object? sender, InventoryChangeEventArgs e)
        {
            _listView.SetSource(_inventoryManager.Items);
        }

        public void ToggleUtilityWindow()
        {
            if (!Visible)
            {
                Visible = true;
                _mapWindow.Width = Dim.Percent(75);
                _mapWindow.SetNeedsDisplay();
                Width = Dim.Percent(25);
                this.SetNeedsDisplay();
            }
            else
            {
                Visible = false;
                _mapWindow.Width = Dim.Fill() - 20;
                _mapWindow.SetNeedsDisplay();
                this.SetNeedsDisplay();
            }
        }
    }
}
