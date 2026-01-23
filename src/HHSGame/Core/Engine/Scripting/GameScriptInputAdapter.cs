using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.UI;

namespace HHSGame.Core.Engine.Scripting
{
    public sealed class GameScriptInputAdapter(Game game) : IScriptInputAdapter
    {
        private bool isMoveSelection;
        private Coordinate moveTarget = new(0, 0);
        private bool isQuestLogOpen;
        private int dialogueOptionIndex;
        private GameStateType lastNonMenuState = GameStateType.Exploration;

        public ScriptGameStateSnapshot GetSnapshot()
        {
            Player? player = game.Player;
            bool hasVisibleEnemies = game.Context.EnemyManager.Enemies.Any(enemy =>
                game.Context.MapState.IsVisible(enemy.X, enemy.Y));

            return new ScriptGameStateSnapshot
            {
                State = game.Context.StateMachine.CurrentState,
                CombatActive = game.IsCombatActive(),
                HasVisibleEnemies = hasVisibleEnemies,
                PlayerX = player?.X ?? 0,
                PlayerY = player?.Y ?? 0,
                PlayerAp = player?.Stats.CurrentAp ?? 0,
                TurnCount = game.Context.TurnManager.TurnCount
            };
        }

        public bool ApplyInput(ScriptInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Token))
            {
                return false;
            }

            string token = NormalizeToken(input.Token);
            if (game.Context.StateMachine.CurrentState == GameStateType.Dialogue)
            {
                return HandleDialogueInput(token);
            }

            if (game.Context.StateMachine.CurrentState == GameStateType.Menu)
            {
                return HandleMenuInput(token);
            }

            if (isMoveSelection)
            {
                return HandleMoveSelectionInput(token);
            }

            if (game.IsCombatActive())
            {
                return HandleCombatInput(token);
            }

            return HandleExplorationInput(token);
        }

        private bool HandleCombatInput(string token)
        {
            switch (token)
            {
                case "M":
                case "MOVE":
                    BeginMoveSelection();
                    return true;
                case "A":
                case "ATTACK":
                    QueueAttack();
                    return true;
                case "G":
                case "PICKUP":
                    QueuePickup();
                    return true;
                case "U":
                case "USE":
                    Events.RaiseGameMessage("Inventory actions are not available in script mode.");
                    return true;
                case "S":
                case "SKILL":
                    Events.RaiseGameMessage("Skills are not available yet.");
                    return true;
                case "C":
                case "COMBAT":
                    game.ToggleCombatMode();
                    return true;
                case "ENTER":
                case "RETURN":
                    game.CommitPlayerActions();
                    return true;
                case "SPACE":
                    SwitchPlayer(1);
                    return true;
                case "TAB":
                    SwitchPlayer(-1);
                    return true;
                case "Q":
                case "QUEST":
                    ToggleQuestLog();
                    return true;
                case "QUIT":
                case "CTRLQ":
                    game.Stop();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleExplorationInput(string token)
        {
            switch (token)
            {
                case "UP":
                case "ARROWUP":
                case "CURSORUP":
                case "K":
                    return game.TryMovePlayer(
                        new Move.Forward(Direction.Up),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "DOWN":
                case "ARROWDOWN":
                case "CURSORDOWN":
                case "J":
                    return game.TryMovePlayer(
                        new Move.Forward(Direction.Down),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "LEFT":
                case "ARROWLEFT":
                case "CURSORLEFT":
                case "H":
                    return game.TryMovePlayer(
                        new Move.Forward(Direction.Left),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "RIGHT":
                case "ARROWRIGHT":
                case "CURSORRIGHT":
                case "L":
                    return game.TryMovePlayer(
                        new Move.Forward(Direction.Right),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "Y":
                    return game.TryMovePlayer(
                        ExtendedDirection.UpLeft.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "U":
                    return game.TryMovePlayer(
                        ExtendedDirection.UpRight.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "B":
                    return game.TryMovePlayer(
                        ExtendedDirection.DownLeft.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "N":
                    return game.TryMovePlayer(
                        ExtendedDirection.DownRight.ToDirections(),
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "G":
                case "PICKUP":
                    return game.PerformPlayerAction(
                        () => game.Player?.PickupItems(),
                        ActionCosts.Pickup,
                        false,
                        GameStateType.Exploration,
                        GameStateType.Combat);
                case "C":
                case "COMBAT":
                    return game.ToggleCombatMode();
                case "SPACE":
                    SwitchPlayer(1);
                    return true;
                case "TAB":
                    SwitchPlayer(-1);
                    return true;
                case "Q":
                case "QUEST":
                    ToggleQuestLog();
                    return true;
                case "QUIT":
                case "CTRLQ":
                    game.Stop();
                    return true;
                case "T":
                case "TALK":
                    return BeginDialogue();
                default:
                    return false;
            }
        }

        private bool HandleMoveSelectionInput(string token)
        {
            switch (token)
            {
                case "UP":
                case "ARROWUP":
                case "CURSORUP":
                    MoveSelectionBy(0, -1);
                    return true;
                case "DOWN":
                case "ARROWDOWN":
                case "CURSORDOWN":
                    MoveSelectionBy(0, 1);
                    return true;
                case "LEFT":
                case "ARROWLEFT":
                case "CURSORLEFT":
                    MoveSelectionBy(-1, 0);
                    return true;
                case "RIGHT":
                case "ARROWRIGHT":
                case "CURSORRIGHT":
                    MoveSelectionBy(1, 0);
                    return true;
                case "ENTER":
                case "RETURN":
                    ConfirmMoveSelection();
                    return true;
                case "ESC":
                case "ESCAPE":
                    CancelMoveSelection();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleDialogueInput(string token)
        {
            switch (token)
            {
                case "UP":
                case "ARROWUP":
                case "CURSORUP":
                    dialogueOptionIndex = Math.Max(0, dialogueOptionIndex - 1);
                    return true;
                case "DOWN":
                case "ARROWDOWN":
                case "CURSORDOWN":
                    dialogueOptionIndex++;
                    return true;
                case "ENTER":
                case "RETURN":
                    if (game.Context.DialogueManager.TrySelectOption(dialogueOptionIndex))
                    {
                        dialogueOptionIndex = 0;
                        if (game.Context.DialogueManager.CurrentSession == null)
                        {
                            game.EndDialogue();
                        }
                    }
                    return true;
                case "ESC":
                case "ESCAPE":
                    game.Context.DialogueManager.EndDialogue();
                    game.EndDialogue();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleMenuInput(string token)
        {
            switch (token)
            {
                case "Q":
                case "QUEST":
                case "ESC":
                case "ESCAPE":
                    ToggleQuestLog();
                    return true;
                case "QUIT":
                case "CTRLQ":
                    game.Stop();
                    return true;
                default:
                    return false;
            }
        }

        private void SwitchPlayer(int direction)
        {
            if (!game.SwitchControlledPlayer(direction))
            {
                Events.RaiseGameMessage("No other controllable characters.");
            }
            else
            {
                Events.RaiseGameMessage($"Switched control to {game.Player?.Name}.");
            }
        }

        private void BeginMoveSelection()
        {
            if (game.Player == null)
            {
                return;
            }

            moveTarget = game.Player.PlannedPosition;
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

        private void ToggleQuestLog()
        {
            isQuestLogOpen = !isQuestLogOpen;
            if (isQuestLogOpen)
            {
                lastNonMenuState = game.Context.StateMachine.CurrentState;
                game.Context.StateMachine.TryChangeState(GameStateType.Menu);
                return;
            }

            game.Context.StateMachine.TryChangeState(lastNonMenuState);
        }

        private bool BeginDialogue()
        {
            if (game.Player == null)
            {
                return false;
            }

            if (game.IsCombatActive())
            {
                return false;
            }

            IReadOnlyList<Npc> nearby = game.Context.NpcManager.GetAdjacentNpcs(game.Player.Position, 1);
            if (nearby.Count == 0)
            {
                return false;
            }

            dialogueOptionIndex = 0;
            return game.TryStartDialogue(nearby[0]);
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

            player.SetPlannedPosition(path[stepsToQueue]);

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
            return pathfinder.FindPath(game.Player.PlannedPosition, destination);
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
            Coordinate origin = player.PlannedPosition;
            List<Enemy> targets = CombatTargeting.GetTargetsInRange(
                game.Context.MapState,
                origin,
                game.Context.EnemyManager.Enemies,
                player.EquippedWeapon);

            if (targets.Count == 0)
            {
                Events.RaiseGameMessage("No enemies in range.");
                return;
            }

            Enemy enemy = targets[0];
            if (!game.TryQueuePlayerAction($"Attack {enemy.Name}", player.EquippedWeapon.ApCost, () => player.Attack(enemy)))
            {
                Events.RaiseGameMessage("Not enough AP to queue attack.");
                return;
            }

            Events.RaiseGameMessage($"Queued attack on {enemy.Name}.");
        }

        private void QueuePickup()
        {
            if (game.Player == null)
            {
                return;
            }

            if (!game.TryQueuePlayerAction("Pick up", ActionCosts.Pickup, () => game.Player?.PickupItems()))
            {
                Events.RaiseGameMessage("Not enough AP to queue pickup.");
                return;
            }

            Events.RaiseGameMessage("Queued item pickup.");
        }

        private static string NormalizeToken(string token)
        {
            string normalized = token.Trim();
            normalized = normalized.Replace(" ", string.Empty, StringComparison.Ordinal);
            normalized = normalized.Replace("-", string.Empty, StringComparison.Ordinal);
            return normalized.ToUpperInvariant();
        }
    }
}
