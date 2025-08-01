using HHSGame.Core;
using HHSGame.Core.Map;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.Views;
using HHSGame.UI.Views;

namespace HHSGame.UI
{
    public class GameUI(MapFrame mapFrame,
            MessageFrame messageFrame,
            InventoryFrame inventoryFrame,
            SurroundingsFrame surroundingsFrame,
            UtilityWindow utilityWindow,
            PlayerSetupWizard playerSetupWizard,
            Game game) : IDisposable
    {
        public Toplevel Start()
        {
            // Create main window
            Toplevel top = new();
            top.Add(mapFrame, inventoryFrame, surroundingsFrame, messageFrame, utilityWindow);

            top.Add(playerSetupWizard);
            // Show player setup wizard before starting the game
            playerSetupWizard.Finished += (sender, args) =>
            {
                // Apply map selection from wizard
                if (playerSetupWizard.UseCustomMap && !string.IsNullOrEmpty(playerSetupWizard.SelectedMap))
                {
                    GameParameters parameters = game.Context.Parameters;

                    parameters.UseCustomMap = true;
                    parameters.CustomMapPath = Path.Combine("data/maps", playerSetupWizard.SelectedMap + ".txt");
                }
                // else if (!string.IsNullOrEmpty(playerSetupWizard.SelectedMap) && Enum.TryParse<MapStyle>(playerSetupWizard.SelectedMap, out var mapStyle))
                // {
                //     GameParameters parameters = game.Context.Parameters;

                //     parameters.MapStyle = mapStyle;
                // }

                game.Start();
                Application.KeyDown += HandleKeyEvent;
            };
            playerSetupWizard.Visible = true;

            return top;
        }

        private void HandleKeyEvent(object? sender, Key key)
        {
            Events.RaiseGameMessage($"Key pressed: {key}");

            if (playerSetupWizard.Visible)
            {
                return;
            }

            if (key.Handled)
            {
                return;
            }
            if (utilityWindow.Visible)
            {
                if (utilityWindow.HandleKeyEvent(key))
                {
                    key.Handled = true;
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
