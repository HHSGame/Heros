using HHSGame.UI.Views;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Windows
{

    public class SurroundingsWindow : Window
    {

        public SurroundingsWindow(
            MapWindow mapWindow,
            InventoryWindow inventoryWindow,
            SurroundingsListView surroundingsListView)
        {
            Title = GUISettings.SurroundingsWindowTitle;
            X = Pos.Right(mapWindow);
            Y = Pos.Bottom(inventoryWindow);
            Width = GUISettings.SidebarWidth;
            Height = Dim.Percent(50);

            Add(surroundingsListView);
            SetScheme(GUISettings.CommonWindowColorScheme);
        }
    }
}