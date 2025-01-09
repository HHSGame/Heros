using Terminal.Gui;
using RpgGame.Core;

namespace RpgGame.UI
{

    public class GameUI : IDisposable
    {

        private readonly Game _game;
        private readonly Window mapWindow;
        private readonly Window messageWindow;
        private readonly Window inventoryWindow;
        private readonly EventLogger eventLogger;
        private readonly MapViewDrawingContext drawingContext;
        private readonly ListView inventoryList;

        public GameUI()
        {
            mapWindow = new Window("Main Map")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill() - 20,
                Height = Dim.Fill() - 10,
                ColorScheme = new ColorScheme
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
                }
            };
            
            inventoryWindow = new Window("Inventory")
            {
                X = Pos.Right(mapWindow),
                Y = 0,
                Width = 20,
                Height = Dim.Fill() - 10,
                ColorScheme = new ColorScheme
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
                }
            };
            
            inventoryList = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            inventoryWindow.Add(inventoryList);
            
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

            top.Add(mapWindow, inventoryWindow, messageWindow);

            _game.Start();
            
            // Update inventory display periodically
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(500), _ => {
                if (_game.Player != null)
                {
                    var items = _game.Player.GetInventory()
                        .Select(i => i.Name)
                        .ToList();
                    inventoryList.SetSource(items);
                }
                return true;
            });

            Application.RootKeyEvent += HandleKeyEvent;

            Application.Run();
            Application.Shutdown();
        }

        private bool HandleKeyEvent(KeyEvent args)
        {
            EventSystem.RaiseEvent($"Key pressed: {args.Key}");
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
                case Key.g:
                    _game.Player?.PickupItems();
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
