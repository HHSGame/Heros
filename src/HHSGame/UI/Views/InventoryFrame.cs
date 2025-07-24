using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using HHSGame.Core.Items;

namespace HHSGame.UI.Views
{
    public class InventoryFrame : FrameView
    {
        private readonly ListView listView;

        public InventoryFrame(MapFrame mapWindow, InventoryManager inventoryManager)
        {
            Title = GUISettings.InventoryWindowTitle;
            X = Pos.Right(mapWindow);
            Y = 0;
            Width = GUISettings.SidebarWidth;
            Height = Dim.Fill(GUISettings.MessageWindowHeight)! - Dim.Percent(50)!;

            listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            listView.SetSource(inventoryManager.GetItems());

            SetScheme(GUISettings.CommonWindowColorScheme);
            Add(listView);
        }
    }
}