using Terminal.Gui;
using HHSGame.Core;

namespace HHSGame.UI
{
    public class UtilityWindow : Window
    {
        private readonly InventoryManager _inventoryManager;
        private readonly Lazy<Player> _playerHolder;
        private readonly ListView _listView;
        private  readonly MapWindow _mapWindow;

        public UtilityWindow(string title, GameContext context, GameUI gameUI): base(title)
        {
            _inventoryManager = context.InventoryManager;
            _playerHolder = new (() => context.Player);
            Title = "Use Item";
            Height = Dim.Fill() - 10;
            Visible = false;
            _mapWindow = gameUI.MapWindow;

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

        private void HandleInventoryChange(object? sender, InventoryChangeEvent e)
        {
            _listView.SetSource(_inventoryManager.Items);
        }

        public void ToggleUtilityWindow()
        {
            if (!Visible) {
                Visible = true;
                _mapWindow.Width = Dim.Percent(75);
                _mapWindow.SetNeedsDisplay();
                Width = Dim.Percent(25);
                this.SetNeedsDisplay();
            } else {
                Visible = false;
                _mapWindow.Width = Dim.Fill() - 20;
                _mapWindow.SetNeedsDisplay();
                this.SetNeedsDisplay();
            }
        }
    }
}
