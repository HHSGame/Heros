
using Terminal.Gui;
using HHSGame.Core;
using HHSGame.UI.Views;

namespace HHSGame.UI.Windows
{

    public class SurroundingsWindow : Window
    {

        public SurroundingsWindow(
            MapWindow mapWindow,
            InventoryWindow inventoryWindow,
            SurroundingsListView surroundingsListView)
            : base(GUISettings.SurroundingsWindowTitle)
        {
            ColorScheme = GUISettings.CommonWindowColorScheme;
            X = Pos.Right(mapWindow);
            Y = Pos.Bottom(inventoryWindow);
            Width = GUISettings.SidebarWidth;
            Height = Dim.Percent(50);

            Add(surroundingsListView);
        }
    }
}