using HHSGame.UI.Views;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Windows
{
    public class InventoryWindow : Window
    {

        public InventoryWindow(MapWindow mapWindow, InventoryListView inventoryList)
        {
            Title = GUISettings.InventoryWindowTitle;
            X = Pos.Right(mapWindow);
            Y = 0;
            Width = GUISettings.SidebarWidth;
            Height = Dim.Fill(GUISettings.MessageWindowHeight)! - Dim.Percent(50)!;
            SetScheme(GUISettings.CommonWindowColorScheme);
            Add(inventoryList);
        }
    }
}