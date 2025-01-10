
using Terminal.Gui;
using RpgGame.Core;

namespace RpgGame.UI {

    public class SurroundingsWindow: Window {
        
        public ListView SurroundingsList { get; }

        private readonly SurroundingsManager _surroundingsManager;

        public SurroundingsWindow(string title, GameContext context) : base(title) {
            ColorScheme = new ColorScheme {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
            };

            SurroundingsList = new ListView() {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            _surroundingsManager = context.SurroundingsManager;
            Add(SurroundingsList);

            EventSystem.OnSurroundingsChange += HandleVisibleTilesChange;
        }

        private void HandleVisibleTilesChange(object? sender, SurroundingsChangeEvent e)
        {
            SurroundingsList.SetSource(_surroundingsManager.GetVisibleItemsAndEnemies()
                    .Select(s => s.Name).ToList());
        }
    }
}