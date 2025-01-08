using Terminal.Gui;
using RpgGame.Core;

namespace RpgGame.UI
{

    public class GameUI : IDisposable
    {

        private readonly Game _game;
        private readonly Window mapWindow;
        private readonly Window messageWindow;
        private readonly EventLogger eventLogger;
        private readonly MapViewDrawingContext drawingContext;

        public GameUI()
        {
            mapWindow = new Window("Main Map")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 10,
                ColorScheme = new ColorScheme
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
                }
            };
            messageWindow = new Window("Messages")
            {
                X = 0,
                Y = Pos.Bottom(mapWindow),
                Width = Dim.Fill(),
                Height = 10,
                ColorScheme = new ColorScheme
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
                }
            };
            eventLogger = new EventLogger()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 10
            };
            drawingContext = new MapViewDrawingContext(Viewport.DefaultWidth, Viewport.DefaultHeight);
            _game = new Game(drawingContext);
        }

        public void Start()
        {
            // Create main window
            var top = Application.Top;
            mapWindow.Add(drawingContext.View);
            messageWindow.Add(eventLogger);

            top.Add(mapWindow, messageWindow);

            _game.Start();

            Application.RootKeyEvent += HandleKeyEvent;

            Application.Run();
            Application.Shutdown();
        }

        private bool HandleKeyEvent(KeyEvent args)
        {
            // Process movement keys
            switch (args.Key)
            {
                case Key.CursorUp:
                    _game.Player?.Move(0, -1);
                    break;
                case Key.CursorDown:
                    _game.Player?.Move(0, 1);
                    break;
                case Key.CursorLeft:
                    _game.Player?.Move(-1, 0);
                    break;
                case Key.CursorRight:
                    _game.Player?.Move(1, 0);
                    break;
                case Key.Q:
                    _game.Stop();
                    Application.RequestStop();
                    Application.Shutdown();
                    return true;
                default:
                    // Let other keys propagate
                    return false;
            }
            // Mark event as handled
            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Application.RequestStop();
            Application.Shutdown();
        }
    }
}