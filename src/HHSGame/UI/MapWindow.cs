using Terminal.Gui;

namespace HHSGame.UI {
    public class MapWindow: Window {

        public IDrawingContext DrawingContext { get; set; }

        public MapWindow(string title) : base(title) {
            ColorScheme = new ColorScheme {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
            };
            DrawingContext = new MapViewDrawingContext(Dim.Fill(), Dim.Fill());
            Add(DrawingContext.View);
        }
    }
}