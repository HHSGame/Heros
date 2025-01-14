using Terminal.Gui;

namespace HHSGame.UI.Windows
{
    public class MapWindow : Window
    {

        public MapWindow(IDrawingContext drawingContext) : base(GUISettings.MapWindowTitle)
        {
            ColorScheme = GUISettings.CommonWindowColorScheme;
            X = 0;
            Y = 0;
            Width = Dim.Fill() - GUISettings.SidebarWidth;
            Height = Dim.Fill() - GUISettings.MessageWindowHeight;

            Add(drawingContext.View);
        }
    }
}