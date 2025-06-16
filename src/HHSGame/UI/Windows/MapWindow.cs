using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Windows
{
    public class MapWindow : Window
    {

        public MapWindow(IDrawingContext drawingContext)
        {
            Title = GUISettings.MapWindowTitle;
            this.SetScheme(GUISettings.CommonWindowColorScheme);
            X = 0;
            Y = 0;
            Width = Dim.Fill()! - GUISettings.SidebarWidth;
            Height = Dim.Fill()! - GUISettings.MessageWindowHeight;

            drawingContext.AttachTo(this);
        }
    }
}