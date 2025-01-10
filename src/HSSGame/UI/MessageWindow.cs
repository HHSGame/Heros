using Terminal.Gui;

namespace HHSGame.UI {

    public class MessageWindow: Window {
        
        readonly EventLogger _eventLogger;

        public MessageWindow(string title) : base(title) {
            ColorScheme = new ColorScheme {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
            };

            _eventLogger = new EventLogger();
            Add(_eventLogger);
        }
    }
}