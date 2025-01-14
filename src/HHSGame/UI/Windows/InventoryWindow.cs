using HHSGame.UI.Views;
using Terminal.Gui;

namespace HHSGame.UI.Windows
{
    public class InventoryWindow : Window
    {

        public InventoryWindow(MapWindow mapWindow, InventoryListView inventoryList) : base(GUISettings.InventoryWindowTitle)
        {
            ColorScheme = GUISettings.CommonWindowColorScheme;
            X = Pos.Right(mapWindow);
            Y = 0;
            Width = GUISettings.SidebarWidth;
            Height = Dim.Fill(GUISettings.MessageWindowHeight) - Dim.Percent(50);

            Add(inventoryList);
        }
    }
}