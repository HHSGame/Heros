using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Views
{
    public class MessageFrame : FrameView
    {
        public MessageFrame(MapFrame mapWindow, EventLoggerView eventLoggerView)
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