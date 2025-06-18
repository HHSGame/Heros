using HHSGame.Core;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.Views;
using HHSGame.UI.Windows;

namespace HHSGame.UI
{
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
            Toplevel top = new();
            top.Add(mapWindow, inventoryWindow, surroundingsWindow, messageWindow, utilityWindow);

            game.Start();

            Application.KeyDown += HandleKeyEvent;
            return top;
        }

        private void HandleKeyEvent(object? sender, Key key)
        {
            EventSystem.RaiseGameMessage($"Key pressed: {key}");

            if (utilityWindow.Visible)
            {
                if (utilityWindow.HandleKeyEvent(key))
                {
                    return;
                }
            }
            // Process movement keys
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                case KeyCode.J:
                    game.Player.Move(new Move.Forward(Direction.Up));
                    break;
                case KeyCode.CursorDown:
                case KeyCode.K:
                    game.Player.Move(new Move.Forward(Direction.Down));
                    break;
                case KeyCode.CursorLeft:
                case KeyCode.H:
                    game.Player.Move(new Move.Forward(Direction.Left));
                    break;
                case KeyCode.CursorRight:
                case KeyCode.L:
                    game.Player.Move(new Move.Forward(Direction.Right));
                    break;
                case KeyCode.Y:
                    game.Player.Move(ExtendedDirection.UpLeft.ToDirections());
                    break;
                case KeyCode.U:
                    game.Player.Move(ExtendedDirection.UpRight.ToDirections());
                    break;
                case KeyCode.B:
                    game.Player.Move(ExtendedDirection.DownLeft.ToDirections());
                    break;
                case KeyCode.N:
                    game.Player.Move(ExtendedDirection.DownRight.ToDirections());
                    break;
                case KeyCode.Q:
                    game.Stop();
                    Application.Shutdown();
                    return;
                case KeyCode.G:
                    game.Player.PickupItems();
                    break;
                case KeyCode.I:
                    utilityWindow.ToggleUtilityWindow();
                    break;
                default:
                    return;
            }
            return;
        }

        public void Dispose()
        {
            Application.Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
