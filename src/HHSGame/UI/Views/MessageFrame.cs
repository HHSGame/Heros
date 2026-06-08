using HHSGame.Core.Rendering;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Views
{
    public class MessageFrame : FrameView
    {
        public MessageFrame(MapFrame mapWindow, StatusBarView statusBarView, EventLoggerView eventLoggerView)
        {
            Title = GUISettings.MessageWindowTitle;
            X = 0;
            Y = Pos.Bottom(statusBarView);
            Width = Dim.Fill()! - GUISettings.ActionSequenceWidth;
            Height = GUISettings.MessageWindowHeight;


            Add(eventLoggerView);
            SetScheme(GUISettingsExtensions.CommonWindowColorScheme);
        }
    }
}
