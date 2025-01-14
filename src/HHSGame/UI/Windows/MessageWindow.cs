using Terminal.Gui;

namespace HHSGame.UI.Windows
{
    using Views;

    public class MessageWindow : Window
    {


        public MessageWindow(MapWindow mapWindow, EventLoggerView eventLoggerView) : base(GUISettings.MessageWindowTitle)
        {
            ColorScheme = GUISettings.CommonWindowColorScheme;
            X = 0;
            Y = Pos.Bottom(mapWindow);
            Width = Dim.Fill();
            Height = GUISettings.MessageWindowHeight;
            Add(eventLoggerView);
        }
    }
}