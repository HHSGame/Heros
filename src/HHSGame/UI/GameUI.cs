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
            StatusBarView statusBarView,
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
            top.Add(mapFrame, inventoryFrame, surroundingsFrame, statusBarView, messageFrame, utilityWindow);

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
                bool wasUtilityVisible = utilityWindow.Visible;
                if (utilityWindow.HandleKeyEvent(key))
                {
                    if (wasUtilityVisible != utilityWindow.Visible)
                    {
                        SyncStateWithUtilityWindow();
                    }
                    key.Handled = true;
                    return;
                }
            }
            bool handled = false;
            // Process movement keys
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                case KeyCode.J:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(new Move.Forward(Direction.Up)),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.CursorDown:
                case KeyCode.K:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(new Move.Forward(Direction.Down)),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.CursorLeft:
                case KeyCode.H:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(new Move.Forward(Direction.Left)),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.CursorRight:
                case KeyCode.L:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(new Move.Forward(Direction.Right)),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.Y:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(ExtendedDirection.UpLeft.ToDirections()),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.U:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(ExtendedDirection.UpRight.ToDirections()),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.B:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(ExtendedDirection.DownLeft.ToDirections()),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.N:
                    handled = game.PerformPlayerAction(
                        () => game.Player.Move(ExtendedDirection.DownRight.ToDirections()),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.Q:
                    game.Stop();
                    Application.Shutdown();
                    return;
                case KeyCode.G:
                    handled = game.PerformPlayerAction(
                        () => game.Player.PickupItems(),
                        true,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.I:
                    utilityWindow.ToggleUtilityWindow();
                    SyncStateWithUtilityWindow();
                    handled = true;
                    break;
                default:
                    return;
            }
            if (handled)
            {
                key.Handled = true;
            }
            return;
        }

        private void SyncStateWithUtilityWindow()
        {
            GameStateType nextState = utilityWindow.Visible
                ? GameStateType.Inventory
                : GameStateType.Exploration;
            game.Context.StateMachine.TryChangeState(nextState);
        }

        public void Dispose()
        {
            Application.Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
