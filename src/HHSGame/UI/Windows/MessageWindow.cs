using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Windows
{
    using Views;

    public class MessageWindow : Window
    {


        public MessageWindow(MapWindow mapWindow, EventLoggerView eventLoggerView)
        {
            Title = GUISettings.MessageWindowTitle;
            X = 0;
            Y = Pos.Bottom(mapWindow);
            Width = Dim.Fill();
            Height = GUISettings.MessageWindowHeight;
            Add(eventLoggerView);
            SetScheme(GUISettings.CommonWindowColorScheme);
        }
    }
}