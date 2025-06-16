using HHSGame.Core;
using HHSGame.Core.Map;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Views
{
    public class SurroundingsListView : ListView
    {
        private readonly SurroundingsManager surroundingsManager;

        public SurroundingsListView(SurroundingsManager surroundingsManager)
        {
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            this.surroundingsManager = surroundingsManager;

            this.SetSource(surroundingsManager.VisibleEntities);

            EventSystem.OnSurroundingsChange += HandleVisibleTilesChange;
        }

        private void HandleVisibleTilesChange(object? sender, SurroundingsChangeEventArgs e)
        {
            surroundingsManager.UpdateVisibleEntities();
        }
    }
}