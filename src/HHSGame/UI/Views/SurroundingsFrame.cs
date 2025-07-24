using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using HHSGame.Core.Map;
using HHSGame.Core;

namespace HHSGame.UI.Views
{

    public class SurroundingsFrame : FrameView
    {
        private readonly SurroundingsManager surroundingsManager;
        private readonly ListView listView;

        public SurroundingsFrame(
            MapFrame mapWindow,
            InventoryFrame inventoryWindow,
            SurroundingsManager surroundingsManager)
        {
            Title = GUISettings.SurroundingsWindowTitle;
            X = Pos.Right(mapWindow);
            Y = Pos.Bottom(inventoryWindow);
            Width = GUISettings.SidebarWidth;
            Height = Dim.Percent(50);
            this.surroundingsManager = surroundingsManager;

            listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            listView.SetSource(surroundingsManager.VisibleEntities);

            Events.OnSurroundingsChange += HandleVisibleTilesChange;

            Add(listView);
            SetScheme(GUISettings.CommonWindowColorScheme);
        }

        private void HandleVisibleTilesChange(object? sender, SurroundingsChangeEventArgs e)
        {
            surroundingsManager.UpdateVisibleEntities();
        }
    }
}