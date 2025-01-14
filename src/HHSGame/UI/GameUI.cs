using Terminal.Gui;
using HHSGame.Core;

namespace HHSGame.UI
{
    using Windows;

    public class GameUI(MapWindow mapWindow,
            MessageWindow messageWindow,
            InventoryWindow inventoryWindow,
            SurroundingsWindow surroundingsWindow,
            UtilityWindow utilityWindow,
            Game game) : IDisposable
    {

        public MapWindow MapWindow => mapWindow;
        public MessageWindow MessageWindow => messageWindow;
        public InventoryWindow InventoryWindow => inventoryWindow;
        public UtilityWindow ItemUsageWindow => utilityWindow;
        public SurroundingsWindow SurroundingsWindow => surroundingsWindow;

        public Toplevel Start()
        {
            // Create main window
            var top = new Toplevel();
            top.Add(mapWindow, inventoryWindow, surroundingsWindow, messageWindow, utilityWindow);

            game.Start();

            Application.RootKeyEvent += HandleKeyEvent;
            return top;
        }

        private bool HandleKeyEvent(KeyEvent args)
        {
            EventSystem.RaiseGameMessage($"Key pressed: {args.Key}");
            // Process movement keys
            switch (args.Key)
            {
                case Key.CursorUp:
                    game.Player.Move(0, -1);
                    break;
                case Key.CursorDown:
                    game.Player.Move(0, 1);
                    break;
                case Key.CursorLeft:
                    game.Player.Move(-1, 0);
                    break;
                case Key.CursorRight:
                    game.Player.Move(1, 0);
                    break;
                case Key.Q:
                    game.Stop();
                    Application.Shutdown();
                    return true;
                case Key.g:
                    game.Player.PickupItems();
                    return true;
                case Key.u:
                    utilityWindow.ToggleUtilityWindow();
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
            Application.Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
