using Terminal.Gui;
using HHSGame.Core;

namespace HHSGame.UI
{

    public class GameUI : IDisposable
    {

        private readonly Game _game;
        private readonly MapWindow mapWindow;
        private readonly MessageWindow messageWindow;
        private readonly InventoryWindow inventoryWindow;
        private readonly UtilityWindow itemUsageWindow;
        private readonly SurroundingsWindow surroundingsWindow;

        public MapWindow MapWindow => mapWindow;
        public MessageWindow MessageWindow => messageWindow;
        public InventoryWindow InventoryWindow => inventoryWindow;
        public UtilityWindow ItemUsageWindow => itemUsageWindow;
        public SurroundingsWindow SurroundingsWindow => surroundingsWindow;


        public GameUI()
        {
            var context = new GameContext(new GameParameters{mapStyle = MapStyle.Cave});

            mapWindow = new MapWindow("Main Map")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill() - 20,
                Height = Dim.Fill() - 10,
            };

            messageWindow = new MessageWindow("Messages")
            {
                X = 0,
                Y = Pos.Bottom(mapWindow),
                Width = Dim.Fill(),
                Height = 10,
            };
            
            inventoryWindow = new InventoryWindow("Inventory", context)
            {
                X = Pos.Right(mapWindow),
                Y = 0,
                Width = 20,
                Height = Dim.Fill(10) - Dim.Percent(50),
            };

            surroundingsWindow = new SurroundingsWindow("Surroundings", context)
            {
                X = Pos.Right(mapWindow),
                Y = Pos.Bottom(inventoryWindow),
                Width = 20,
                Height = Dim.Percent(50),
            };

            itemUsageWindow = new UtilityWindow("Utilities", context, this) {
                X = Pos.Right(mapWindow),
                Y = 0,
                Visible = false
            };

            context.InitializeDrawingContext(mapWindow.DrawingContext);

            _game = new Game(context);
        }

        public void Start()
        {
            // Create main window
            var top = new Toplevel();

            top.Add(mapWindow, inventoryWindow, surroundingsWindow, messageWindow, itemUsageWindow);

            _game.Start();

            Application.RootKeyEvent += HandleKeyEvent;
            Application.Run(top);
            top.Dispose();
            Application.Shutdown();
        }

        private bool HandleKeyEvent(KeyEvent args)
        {
            EventSystem.RaiseGameMessage($"Key pressed: {args.Key}");
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
                case Key.u:
                    itemUsageWindow.ToggleUtilityWindow();
                    break;
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
