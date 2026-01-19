using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Views
{
    public class MapFrame : FrameView
    {

        public MapFrame(IDrawingContext drawingContext)
        {
            Title = GUISettings.MapWindowTitle;
            this.SetScheme(GUISettings.CommonWindowColorScheme);
            X = 0;
            Y = 0;
            Width = Dim.Fill()! - GUISettings.SidebarWidth;
            Height = Dim.Fill()! - (GUISettings.MessageWindowHeight + GUISettings.StatusBarHeight);

            drawingContext.AttachTo(this);
        }
    }
}
