using HHSGame.Core;
using Terminal.Gui;

namespace HHSGame.UI {
    public class InventoryWindow : Window {
        private readonly InventoryManager _inventoryManager;

        public InventoryManager InventoryManager => _inventoryManager;

        public ListView InventoryList { get; }

        public InventoryWindow(string title, GameContext context) : base(title) {
            ColorScheme = new ColorScheme {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
            };

            InventoryList = new ListView() {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            Add(InventoryList);

            _inventoryManager = context.InventoryManager;
            EventSystem.OnInventoryChange += HandleInventoryChange;
        }

        private void HandleInventoryChange(object? sender, InventoryChangeEvent e)
        {
            InventoryList.SetSource(_inventoryManager.Items.Select(i => i.Name).ToList());
        }
    }
}