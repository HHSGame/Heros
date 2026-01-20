using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
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
            ActionSequenceFrame actionSequenceFrame,
            InventoryFrame inventoryFrame,
            SurroundingsFrame surroundingsFrame,
            UtilityWindow utilityWindow,
            PlayerSetupWizard playerSetupWizard,
            Game game) : IDisposable
    {
        private GameStateType lastNonInventoryState = GameStateType.Exploration;
        private bool isMoveSelection;
        private Coordinate moveTarget = new(0, 0);
        private bool isKeyHandlerRegistered;
        public Toplevel Start()
        {
            // Create main window
            Toplevel top = new();
            top.Add(mapFrame, inventoryFrame, surroundingsFrame, statusBarView, messageFrame, actionSequenceFrame, utilityWindow);

            top.Add(playerSetupWizard);
            // Show player setup wizard before starting the game
            playerSetupWizard.Finished += (sender, args) =>
            {
                GameParameters parameters = game.Context.Parameters;
                // Apply map selection from wizard
                if (playerSetupWizard.UseCustomMap && !string.IsNullOrEmpty(playerSetupWizard.SelectedMap))
                {
                    parameters.UseCustomMap = true;
                    parameters.CustomMapPath = Path.Combine("data/maps", playerSetupWizard.SelectedMap + ".txt");
                }
                parameters.PlayerClass = playerSetupWizard.SelectedClass;

                if (playerSetupWizard.UseCustomCharacter)
                {
                    parameters.PlayerAttributes = playerSetupWizard.SelectedAttributes;
                    parameters.PlayerSkills = playerSetupWizard.SelectedSkills;
                }
                else
                {
                    parameters.PlayerAttributes = null;
                    parameters.PlayerSkills = null;
                }
                // else if (!string.IsNullOrEmpty(playerSetupWizard.SelectedMap) && Enum.TryParse<MapStyle>(playerSetupWizard.SelectedMap, out var mapStyle))
                // {
                //     GameParameters parameters = game.Context.Parameters;

                //     parameters.MapStyle = mapStyle;
                // }

                game.Start();
                if (!isKeyHandlerRegistered)
                {
                    Application.KeyDown -= HandleKeyEvent;
                    Application.KeyDown += HandleKeyEvent;
                    isKeyHandlerRegistered = true;
                }
            };
            playerSetupWizard.Visible = true;

            return top;
        }

        private void HandleKeyEvent(object? sender, Key key)
        {
            if (playerSetupWizard.Visible)
            {
                return;
            }

            if (isMoveSelection)
            {
                if (HandleMoveSelectionKey(key))
                {
                    key.Handled = true;
                }
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

            if (game.IsCombatActive())
            {
                if (HandleCombatPlanningKey(key))
                {
                    key.Handled = true;
                }
                return;
            }
            bool handled = false;
            // Process movement keys
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                case KeyCode.J:
                    handled = game.TryMovePlayer(
                        new Move.Forward(Direction.Up),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.CursorDown:
                case KeyCode.K:
                    handled = game.TryMovePlayer(
                        new Move.Forward(Direction.Down),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.CursorLeft:
                case KeyCode.H:
                    handled = game.TryMovePlayer(
                        new Move.Forward(Direction.Left),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.CursorRight:
                case KeyCode.L:
                    handled = game.TryMovePlayer(
                        new Move.Forward(Direction.Right),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.Y:
                    handled = game.TryMovePlayer(
                        ExtendedDirection.UpLeft.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.U:
                    handled = game.TryMovePlayer(
                        ExtendedDirection.UpRight.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.B:
                    handled = game.TryMovePlayer(
                        ExtendedDirection.DownLeft.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.N:
                    handled = game.TryMovePlayer(
                        ExtendedDirection.DownRight.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.Q:
                    game.Stop();
                    Application.Shutdown();
                    return;
                case KeyCode.G:
                    handled = game.PerformPlayerAction(
                        () => game.Player?.PickupItems(),
                        ActionCosts.Pickup,
                        false,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                    break;
                case KeyCode.I:
                    utilityWindow.ToggleUtilityWindow();
                    SyncStateWithUtilityWindow();
                    handled = true;
                    break;
                case KeyCode.C:
                    handled = HandleCombatToggle();
                    break;
                case KeyCode.Space:
                    if (!game.SwitchControlledPlayer(1))
                    {
                        Events.RaiseGameMessage("No other controllable characters.");
                    }
                    else
                    {
                        Events.RaiseGameMessage($"Switched control to {game.Player?.Name}.");
                    }
                    handled = true;
                    break;
                case KeyCode.Tab:
                    if (!game.SwitchControlledPlayer(-1))
                    {
                        Events.RaiseGameMessage("No other controllable characters.");
                    }
                    else
                    {
                        Events.RaiseGameMessage($"Switched control to {game.Player?.Name}.");
                    }
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

        private bool HandleCombatPlanningKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.M:
                    BeginMoveSelection();
                    return true;
                case KeyCode.A:
                    QueueAttack();
                    return true;
                case KeyCode.U:
                    utilityWindow.ToggleUtilityWindow();
                    SyncStateWithUtilityWindow();
                    return true;
                case KeyCode.S:
                    Events.RaiseGameMessage("Skills are not available yet.");
                    return true;
                case KeyCode.C:
                    HandleCombatToggle();
                    return true;
                case KeyCode.Enter:
                    game.CommitPlayerActions();
                    return true;
                case KeyCode.Space:
                    if (!game.SwitchControlledPlayer(1))
                    {
                        Events.RaiseGameMessage("No other controllable characters.");
                    }
                    else
                    {
                        Events.RaiseGameMessage($"Switched control to {game.Player?.Name}.");
                    }
                    return true;
                case KeyCode.Tab:
                    if (!game.SwitchControlledPlayer(-1))
                    {
                        Events.RaiseGameMessage("No other controllable characters.");
                    }
                    else
                    {
                        Events.RaiseGameMessage($"Switched control to {game.Player?.Name}.");
                    }
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleMoveSelectionKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    MoveSelectionBy(0, -1);
                    return true;
                case KeyCode.CursorDown:
                    MoveSelectionBy(0, 1);
                    return true;
                case KeyCode.CursorLeft:
                    MoveSelectionBy(-1, 0);
                    return true;
                case KeyCode.CursorRight:
                    MoveSelectionBy(1, 0);
                    return true;
                case KeyCode.Enter:
                    ConfirmMoveSelection();
                    return true;
                case KeyCode.Esc:
                    CancelMoveSelection();
                    return true;
                default:
                    return false;
            }
        }

        private void BeginMoveSelection()
        {
            if (game.Player == null)
            {
                return;
            }

            moveTarget = game.Player.Position;
            isMoveSelection = true;
            Events.RaiseGameMessage("Move mode: use arrow keys, Enter to confirm, Esc to cancel.");
            UpdateMovePreview();
        }

        private void CancelMoveSelection()
        {
            isMoveSelection = false;
            game.ClearOverlayCells();
            Events.RaiseGameMessage("Move mode cancelled.");
        }

        private void MoveSelectionBy(int dx, int dy)
        {
            if (game.Player == null)
            {
                return;
            }

            Coordinate next = moveTarget.Target(dx, dy);
            if (!game.Context.MapState.IsInBounds(next))
            {
                return;
            }

            moveTarget = next;
            UpdateMovePreview();
        }

        private void UpdateMovePreview()
        {
            if (game.Player == null)
            {
                return;
            }

            List<Coordinate> path = GetMovePath(moveTarget);
            UpdateMoveOverlay(path);
            if (path.Count == 0)
            {
                Events.RaiseGameMessage($"No path to {moveTarget}.");
                return;
            }

            int steps = Math.Max(0, path.Count - 1);
            int turns = CalculateTurnsNeeded(steps, game.Player.Stats.MaxAp);
            int remainingAp = game.GetRemainingPlannedAp();
            Events.RaiseGameMessage($"Move target {moveTarget}, steps {steps}, turns {turns}, remaining AP {remainingAp}.");
        }

        private void ConfirmMoveSelection()
        {
            if (game.Player == null)
            {
                return;
            }

            Player player = game.Player;
            List<Coordinate> path = GetMovePath(moveTarget);
            if (path.Count <= 1)
            {
                Events.RaiseGameMessage("No movement queued.");
                isMoveSelection = false;
                game.ClearOverlayCells();
                return;
            }

            int steps = path.Count - 1;
            int remainingAp = game.GetRemainingPlannedAp();
            int stepsToQueue = Math.Min(steps, remainingAp);
            if (stepsToQueue <= 0)
            {
                int turns = CalculateTurnsNeeded(steps, game.Player.Stats.MaxAp);
                Events.RaiseGameMessage($"Not enough AP. Steps {steps}, turns {turns}.");
                isMoveSelection = false;
                game.ClearOverlayCells();
                return;
            }

            for (int i = 1; i <= stepsToQueue; i++)
            {
                Coordinate from = path[i - 1];
                Coordinate to = path[i];
                Move move = ToMove(from, to);
                if (move is Move.None)
                {
                    break;
                }

                if (!game.TryQueuePlayerAction("Move", ActionCosts.Movement, () => player.Move(move)))
                {
                    break;
                }
            }

            int turnsNeeded = CalculateTurnsNeeded(steps, game.Player.Stats.MaxAp);
            if (stepsToQueue < steps)
            {
                Events.RaiseGameMessage($"Queued {stepsToQueue}/{steps} steps towards {moveTarget} (turns needed {turnsNeeded}).");
            }
            else
            {
                Events.RaiseGameMessage($"Queued move to {moveTarget} (turns needed {turnsNeeded}).");
            }

            isMoveSelection = false;
            game.ClearOverlayCells();
        }

        private void UpdateMoveOverlay(List<Coordinate> path)
        {
            if (path.Count <= 1)
            {
                game.ClearOverlayCells();
                return;
            }

            List<(Coordinate Position, Cell Cell)> overlay = [];
            for (int i = 1; i < path.Count - 1; i++)
            {
                overlay.Add((path[i], new Cell
                {
                    Character = GUISettings.PathPreviewGlyph,
                    Attribute = ColorPresets.PathPreview
                }));
            }

            Coordinate target = path[^1];
            overlay.Add((target, new Cell
            {
                Character = GUISettings.TargetPreviewGlyph,
                Attribute = ColorPresets.TargetPreview
            }));

            game.SetOverlayCells(overlay);
        }

        private List<Coordinate> GetMovePath(Coordinate destination)
        {
            if (game.Player == null)
            {
                return [];
            }

            if (!game.Context.MapState.IsWalkable(destination))
            {
                return [];
            }

            Pathfinder pathfinder = new(game.Context.MapState);
            return pathfinder.FindPath(game.Player.Position, destination);
        }

        private static int CalculateTurnsNeeded(int steps, int maxAp)
        {
            if (steps <= 0 || maxAp <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(steps / (double)maxAp);
        }

        private static Move ToMove(Coordinate from, Coordinate to)
        {
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            return (dx, dy) switch
            {
                (1, 0) => new Move.Forward(Direction.Right),
                (-1, 0) => new Move.Forward(Direction.Left),
                (0, 1) => new Move.Forward(Direction.Down),
                (0, -1) => new Move.Forward(Direction.Up),
                _ => new Move.None()
            };
        }

        private void QueueAttack()
        {
            if (game.Player == null)
            {
                return;
            }

            Player player = game.Player;
            Enemy? enemy = game.Context.EnemyManager.Enemies.FirstOrDefault(target =>
                Math.Abs(target.X - player.X) + Math.Abs(target.Y - player.Y) == 1);

            if (enemy == null)
            {
                Events.RaiseGameMessage("No adjacent enemy to attack.");
                return;
            }

            if (!game.TryQueuePlayerAction($"Attack {enemy.Name}", player.EquippedWeapon.ApCost, () => player.Attack(enemy)))
            {
                Events.RaiseGameMessage("Not enough AP to queue attack.");
                return;
            }

            Events.RaiseGameMessage($"Queued attack on {enemy.Name}.");
        }

        private bool HandleCombatToggle()
        {
            bool wasCombat = game.IsCombatActive();
            bool toggled = game.ToggleCombatMode();
            if (!toggled)
            {
                Events.RaiseGameMessage("Cannot exit combat while enemies are alert.");
                return true;
            }

            if (wasCombat)
            {
                Events.RaiseGameMessage("Exited combat mode.");
            }
            else
            {
                Events.RaiseGameMessage("Entered combat mode.");
            }

            return true;
        }

        private void SyncStateWithUtilityWindow()
        {
            if (utilityWindow.Visible)
            {
                GameStateType current = game.Context.StateMachine.CurrentState;
                if (current != GameStateType.Inventory)
                {
                    lastNonInventoryState = current;
                }
                game.Context.StateMachine.TryChangeState(GameStateType.Inventory);
                return;
            }

            game.Context.StateMachine.TryChangeState(lastNonInventoryState);
        }

        public void Dispose()
        {
            if (isKeyHandlerRegistered)
            {
                Application.KeyDown -= HandleKeyEvent;
                isKeyHandlerRegistered = false;
            }
            Application.Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
