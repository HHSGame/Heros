using System;
using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.SkillActions;
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
            QuestLogWindow questLogWindow,
            SkillActionWindow skillActionWindow,
            DialogueWindow dialogueWindow,
            PlayerSetupWizard playerSetupWizard,
            UiStatusState uiStatus,
            GameUiOptions options,
            Game game) : IDisposable
    {
        private GameStateType lastNonInventoryState = GameStateType.Exploration;
        private GameStateType lastNonMenuState = GameStateType.Exploration;
        private bool isMoveSelection;
        private Coordinate moveTarget = new(0, 0);
        private bool isAttackSelection;
        private readonly List<Enemy> attackTargets = [];
        private int attackTargetIndex;
        private bool isTalkSelection;
        private readonly List<Npc> talkTargets = [];
        private Npc? selectedTalkTarget;
        private bool isSkillSelection;
        private SkillActionDefinition? selectedSkillAction;
        private SkillActionTargetType selectedSkillTargetType;
        private readonly List<Enemy> skillEnemyTargets = [];
        private int skillEnemyTargetIndex;
        private readonly List<Npc> skillNpcTargets = [];
        private Npc? selectedSkillNpc;
        private readonly List<Player> skillPlayerTargets = [];
        private int skillPlayerTargetIndex;
        private Coordinate skillTargetPosition = new(0, 0);
        private bool isKeyHandlerRegistered;
        private bool isGameStarted;
        private bool selectionBlinkOn;
        private bool selectionBlinkTimerActive;
        private readonly UiStatusState uiStatus = uiStatus;
        public Toplevel Start()
        {
            // Create main window
            Toplevel top = new();
            top.Add(mapFrame, inventoryFrame, surroundingsFrame, statusBarView, messageFrame, actionSequenceFrame, utilityWindow, questLogWindow, skillActionWindow, dialogueWindow);

            if (options.SkipWizard)
            {
                playerSetupWizard.Visible = false;
                StartGame(false);
            }
            else
            {
                top.Add(playerSetupWizard);
                // Show player setup wizard before starting the game
                playerSetupWizard.Finished += (sender, args) =>
                {
                    if (isGameStarted)
                    {
                        return;
                    }
                    StartGame(true);
                    playerSetupWizard.Visible = false;
                    top.Remove(playerSetupWizard);
                };
                playerSetupWizard.Visible = true;
            }

            return top;
        }

        private void StartGame(bool applyWizardSelections)
        {
            if (isGameStarted)
            {
                return;
            }

            isGameStarted = true;

            if (applyWizardSelections)
            {
                ApplyWizardSelections();
            }

            game.Start();
            game.Context.DialogueManager.SessionChanged += (_, __) =>
            {
                if (game.Context.DialogueManager.CurrentSession == null)
                {
                    game.EndDialogue();
                }
            };
            skillActionWindow.ActionSelected += (_, action) => BeginSkillAction(action);

            Application.AddTimeout(TimeSpan.Zero, () =>
            {
                game.RefreshFrame();
                return false;
            });

            if (!isKeyHandlerRegistered)
            {
                Application.KeyDown -= HandleKeyEvent;
                Application.KeyDown += HandleKeyEvent;
                isKeyHandlerRegistered = true;
            }
        }

        private void ApplyWizardSelections()
        {
            GameParameters parameters = game.Context.Parameters;
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

            if (isAttackSelection)
            {
                if (HandleAttackSelectionKey(key))
                {
                    key.Handled = true;
                }
                return;
            }

            if (isTalkSelection)
            {
                if (HandleTalkSelectionKey(key))
                {
                    key.Handled = true;
                }
                return;
            }

            if (isSkillSelection)
            {
                if (HandleSkillSelectionKey(key))
                {
                    key.Handled = true;
                }
                return;
            }

            if (key.Handled)
            {
                return;
            }

            if (dialogueWindow.Visible)
            {
                if (dialogueWindow.HandleKeyEvent(key))
                {
                    if (!dialogueWindow.Visible)
                    {
                        game.EndDialogue();
                    }
                    key.Handled = true;
                }
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

            if (skillActionWindow.Visible)
            {
                bool wasSkillVisible = skillActionWindow.Visible;
                if (skillActionWindow.HandleKeyEvent(key))
                {
                    if (wasSkillVisible != skillActionWindow.Visible)
                    {
                        SyncStateWithSkillWindow();
                    }
                    key.Handled = true;
                    return;
                }
            }

            if (questLogWindow.Visible)
            {
                bool wasQuestVisible = questLogWindow.Visible;
                if (questLogWindow.HandleKeyEvent(key))
                {
                    if (wasQuestVisible != questLogWindow.Visible)
                    {
                        SyncStateWithQuestLogWindow();
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
                    ToggleQuestLog();
                    handled = true;
                    break;
                case KeyCode.Q | KeyCode.CtrlMask:
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
                    if (questLogWindow.Visible)
                    {
                        questLogWindow.Toggle();
                        SyncStateWithQuestLogWindow();
                    }
                    utilityWindow.ToggleUtilityWindow();
                    SyncStateWithUtilityWindow();
                    handled = true;
                    break;
                case KeyCode.C:
                    handled = HandleCombatToggle();
                    break;
                case KeyCode.S:
                    ToggleSkillWindow();
                    handled = true;
                    break;
                case KeyCode.T:
                    handled = BeginTalkSelection();
                    break;
                case KeyCode.Space:
                    if (!game.SwitchControlledPlayer(1))
                    {
                        Events.RaiseGameMessage("No other controllable characters.");
                    }
                    else
                    {
                        Events.RaiseGameMessage($"Switched control to {game.Player?.Name}.");
                        RefreshSkillWindow();
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
                        RefreshSkillWindow();
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
                    BeginAttackSelection();
                    return true;
                case KeyCode.G:
                    QueuePickup();
                    return true;
                case KeyCode.U:
                    utilityWindow.ToggleUtilityWindow();
                    SyncStateWithUtilityWindow();
                    return true;
                case KeyCode.S:
                    ToggleSkillWindow();
                    return true;
                case KeyCode.C:
                    HandleCombatToggle();
                    return true;
                case KeyCode.Q:
                    ToggleQuestLog();
                    return true;
                case KeyCode.Q | KeyCode.CtrlMask:
                    game.Stop();
                    Application.Shutdown();
                    return true;
                case KeyCode.T:
                    BeginTalkSelection();
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
                        RefreshSkillWindow();
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
                        RefreshSkillWindow();
                    }
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleTalkSelectionKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    SelectTalkTargetByDirection(0, -1);
                    return true;
                case KeyCode.CursorDown:
                    SelectTalkTargetByDirection(0, 1);
                    return true;
                case KeyCode.CursorLeft:
                    SelectTalkTargetByDirection(-1, 0);
                    return true;
                case KeyCode.CursorRight:
                    SelectTalkTargetByDirection(1, 0);
                    return true;
                case KeyCode.Enter:
                    ConfirmTalkSelection();
                    return true;
                case KeyCode.Esc:
                    CancelTalkSelection();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleSkillSelectionKey(Key key)
        {
            if (selectedSkillAction == null)
            {
                return false;
            }

            switch (selectedSkillTargetType)
            {
                case SkillActionTargetType.Direction:
                case SkillActionTargetType.AdjacentDoor:
                case SkillActionTargetType.AdjacentNpc:
                    return HandleDirectionalSkillSelectionKey(key);
                case SkillActionTargetType.AdjacentEnemy:
                case SkillActionTargetType.RangedEnemy:
                    return HandleEnemySkillSelectionKey(key);
                case SkillActionTargetType.AdjacentAllyOrSelf:
                    return HandleAllySkillSelectionKey(key);
                default:
                    return false;
            }
        }

        private bool HandleDirectionalSkillSelectionKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    SelectSkillTargetByDirection(0, -1);
                    return true;
                case KeyCode.CursorDown:
                    SelectSkillTargetByDirection(0, 1);
                    return true;
                case KeyCode.CursorLeft:
                    SelectSkillTargetByDirection(-1, 0);
                    return true;
                case KeyCode.CursorRight:
                    SelectSkillTargetByDirection(1, 0);
                    return true;
                case KeyCode.Enter:
                    ConfirmSkillSelection();
                    return true;
                case KeyCode.Esc:
                    CancelSkillSelection();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleEnemySkillSelectionKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorLeft:
                case KeyCode.CursorUp:
                    CycleSkillEnemyTarget(-1);
                    return true;
                case KeyCode.CursorRight:
                case KeyCode.CursorDown:
                case KeyCode.Tab:
                    CycleSkillEnemyTarget(1);
                    return true;
                case KeyCode.Enter:
                    ConfirmSkillSelection();
                    return true;
                case KeyCode.Esc:
                    CancelSkillSelection();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleAllySkillSelectionKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorLeft:
                case KeyCode.CursorUp:
                    CycleSkillPlayerTarget(-1);
                    return true;
                case KeyCode.CursorRight:
                case KeyCode.CursorDown:
                case KeyCode.Tab:
                    CycleSkillPlayerTarget(1);
                    return true;
                case KeyCode.Enter:
                    ConfirmSkillSelection();
                    return true;
                case KeyCode.Esc:
                    CancelSkillSelection();
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
                case KeyCode.K:
                    MoveSelectionBy(0, -1);
                    return true;
                case KeyCode.CursorDown:
                case KeyCode.J:
                    MoveSelectionBy(0, 1);
                    return true;
                case KeyCode.CursorLeft:
                case KeyCode.H:
                    MoveSelectionBy(-1, 0);
                    return true;
                case KeyCode.CursorRight:
                case KeyCode.L:
                    MoveSelectionBy(1, 0);
                    return true;
                case KeyCode.Y:
                    MoveSelectionBy(-1, -1);
                    return true;
                case KeyCode.U:
                    MoveSelectionBy(1, -1);
                    return true;
                case KeyCode.B:
                    MoveSelectionBy(-1, 1);
                    return true;
                case KeyCode.N:
                    MoveSelectionBy(1, 1);
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

        private bool HandleAttackSelectionKey(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorLeft:
                case KeyCode.CursorUp:
                    CycleAttackTarget(-1);
                    return true;
                case KeyCode.CursorRight:
                case KeyCode.CursorDown:
                case KeyCode.Tab:
                    CycleAttackTarget(1);
                    return true;
                case KeyCode.Enter:
                    ConfirmAttackSelection();
                    return true;
                case KeyCode.Esc:
                    CancelAttackSelection();
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

            moveTarget = game.Player.PlannedPosition;
            isMoveSelection = true;
            Events.RaiseGameMessage("Move mode: use arrow keys, Enter to confirm, Esc to cancel.");
            EnsureSelectionBlinker();
            UpdateMovePreview();
        }

        private void BeginAttackSelection()
        {
            if (game.Player == null)
            {
                return;
            }

            List<Enemy> targets = GetAttackTargets();
            if (targets.Count == 0)
            {
                Events.RaiseGameMessage("No enemies in range.");
                return;
            }

            if (targets.Count == 1)
            {
                QueueAttack(targets[0]);
                return;
            }

            attackTargets.Clear();
            attackTargets.AddRange(targets);
            attackTargetIndex = 0;
            isAttackSelection = true;
            Events.RaiseGameMessage("Attack mode: arrow keys/Tab to switch target, Enter to confirm, Esc to cancel.");
            EnsureSelectionBlinker();
            UpdateAttackOverlay();
        }

        private bool BeginTalkSelection()
        {
            if (game.Player == null)
            {
                return false;
            }

            if (game.IsCombatActive())
            {
                Events.RaiseGameMessage("Cannot start dialogue during combat.");
                return true;
            }

            IReadOnlyList<Npc> nearby = game.Context.NpcManager.GetAdjacentNpcs(game.Player.Position, 1);
            if (nearby.Count == 0)
            {
                Events.RaiseGameMessage("No one nearby to talk to.");
                return true;
            }

            if (nearby.Count == 1)
            {
                StartDialogue(nearby[0]);
                return true;
            }

            talkTargets.Clear();
            talkTargets.AddRange(nearby);
            selectedTalkTarget = talkTargets[0];
            isTalkSelection = true;
            Events.RaiseGameMessage("Select a direction to choose who to talk to.");
            EnsureSelectionBlinker();
            UpdateTalkOverlay();
            return true;
        }

        private void BeginSkillAction(SkillActionDefinition action)
        {
            if (game.Player == null)
            {
                return;
            }

            if (skillActionWindow.Visible)
            {
                skillActionWindow.Toggle();
                SyncStateWithSkillWindow();
            }

            selectedSkillAction = action;
            selectedSkillTargetType = action.TargetType;

            switch (action.TargetType)
            {
                case SkillActionTargetType.None:
                    ExecuteSkillAction(action, new SkillActionTarget());
                    break;
                case SkillActionTargetType.AdjacentEnemy:
                    BeginSkillEnemySelection(GetAdjacentEnemies(1), "Select an adjacent enemy.");
                    break;
                case SkillActionTargetType.RangedEnemy:
                    BeginSkillEnemySelection(GetAttackTargets(), "Select a target in range.");
                    break;
                case SkillActionTargetType.AdjacentNpc:
                    BeginSkillNpcSelection();
                    break;
                case SkillActionTargetType.AdjacentAllyOrSelf:
                    BeginSkillAllySelection();
                    break;
                case SkillActionTargetType.Direction:
                    BeginSkillDirectionSelection("Select a direction.");
                    break;
                case SkillActionTargetType.AdjacentDoor:
                    BeginSkillDoorSelection();
                    break;
                default:
                    CancelSkillSelection();
                    break;
            }
        }

        private void BeginSkillEnemySelection(List<Enemy> targets, string message)
        {
            if (targets.Count == 0)
            {
                Events.RaiseGameMessage("No valid targets.");
                CancelSkillSelection();
                return;
            }

            if (targets.Count == 1)
            {
                ExecuteSkillAction(selectedSkillAction!, new SkillActionTarget(Enemy: targets[0]));
                return;
            }

            skillEnemyTargets.Clear();
            skillEnemyTargets.AddRange(targets);
            skillEnemyTargetIndex = 0;
            isSkillSelection = true;
            Events.RaiseGameMessage(message);
            EnsureSelectionBlinker();
            UpdateSkillTargetOverlay();
        }

        private void BeginSkillNpcSelection()
        {
            if (game.Player == null)
            {
                CancelSkillSelection();
                return;
            }

            IReadOnlyList<Npc> nearby = game.Context.NpcManager.GetAdjacentNpcs(game.Player.Position, 1);
            if (nearby.Count == 0)
            {
                Events.RaiseGameMessage("No NPC nearby.");
                CancelSkillSelection();
                return;
            }

            if (nearby.Count == 1)
            {
                ExecuteSkillAction(selectedSkillAction!, new SkillActionTarget(Npc: nearby[0]));
                return;
            }

            skillNpcTargets.Clear();
            skillNpcTargets.AddRange(nearby);
            selectedSkillNpc = skillNpcTargets[0];
            isSkillSelection = true;
            Events.RaiseGameMessage("Select a direction to choose a target.");
            EnsureSelectionBlinker();
            UpdateSkillTargetOverlay();
        }

        private void BeginSkillAllySelection()
        {
            if (game.Player == null)
            {
                CancelSkillSelection();
                return;
            }

            skillPlayerTargets.Clear();
            foreach (Player player in game.Context.PartyState.Players)
            {
                if (IsAdjacent(game.Player.Position, player.Position))
                {
                    skillPlayerTargets.Add(player);
                }
            }

            if (skillPlayerTargets.Count == 0)
            {
                skillPlayerTargets.Add(game.Player);
            }

            if (skillPlayerTargets.Count == 1)
            {
                ExecuteSkillAction(selectedSkillAction!, new SkillActionTarget(Player: skillPlayerTargets[0]));
                return;
            }

            skillPlayerTargetIndex = 0;
            isSkillSelection = true;
            Events.RaiseGameMessage("Select a target.");
            EnsureSelectionBlinker();
            UpdateSkillTargetOverlay();
        }

        private void BeginSkillDirectionSelection(string message)
        {
            if (game.Player == null)
            {
                CancelSkillSelection();
                return;
            }

            Coordinate origin = game.Player.Position;
            Coordinate initial = origin.Target(0, -2);
            if (!game.Context.MapState.IsInBounds(initial))
            {
                initial = origin.Target(0, 2);
            }
            skillTargetPosition = initial;
            isSkillSelection = true;
            Events.RaiseGameMessage(message);
            EnsureSelectionBlinker();
            UpdateSkillTargetOverlay();
        }

        private void BeginSkillDoorSelection()
        {
            if (game.Player == null)
            {
                CancelSkillSelection();
                return;
            }

            List<Coordinate> doors = GetAdjacentDoors(game.Player.Position);
            if (doors.Count == 0)
            {
                Events.RaiseGameMessage("No adjacent door.");
                CancelSkillSelection();
                return;
            }

            if (doors.Count == 1)
            {
                ExecuteSkillAction(selectedSkillAction!, new SkillActionTarget(Position: doors[0]));
                return;
            }

            skillTargetPosition = doors[0];
            isSkillSelection = true;
            Events.RaiseGameMessage("Select a direction to choose a door.");
            EnsureSelectionBlinker();
            UpdateSkillTargetOverlay();
        }

        private void CancelSkillSelection()
        {
            isSkillSelection = false;
            selectedSkillAction = null;
            selectedSkillTargetType = SkillActionTargetType.None;
            skillEnemyTargets.Clear();
            skillNpcTargets.Clear();
            selectedSkillNpc = null;
            skillPlayerTargets.Clear();
            game.ClearOverlayCells();
            Events.RaiseGameMessage("Skill selection cancelled.");
            ClearSelectionStatus();
        }

        private void ConfirmSkillSelection()
        {
            if (selectedSkillAction == null)
            {
                return;
            }

            SkillActionTarget target = selectedSkillTargetType switch
            {
                SkillActionTargetType.AdjacentEnemy or SkillActionTargetType.RangedEnemy =>
                    skillEnemyTargets.Count > 0 ? new SkillActionTarget(Enemy: skillEnemyTargets[skillEnemyTargetIndex]) : new SkillActionTarget(),
                SkillActionTargetType.AdjacentNpc =>
                    selectedSkillNpc != null ? new SkillActionTarget(Npc: selectedSkillNpc) : new SkillActionTarget(),
                SkillActionTargetType.AdjacentAllyOrSelf =>
                    skillPlayerTargets.Count > 0 ? new SkillActionTarget(Player: skillPlayerTargets[skillPlayerTargetIndex]) : new SkillActionTarget(),
                SkillActionTargetType.AdjacentDoor or SkillActionTargetType.Direction =>
                    new SkillActionTarget(Position: skillTargetPosition),
                _ => new SkillActionTarget()
            };

            ExecuteSkillAction(selectedSkillAction, target);
            isSkillSelection = false;
            selectedSkillAction = null;
            selectedSkillTargetType = SkillActionTargetType.None;
            skillEnemyTargets.Clear();
            skillNpcTargets.Clear();
            selectedSkillNpc = null;
            skillPlayerTargets.Clear();
            game.ClearOverlayCells();
            ClearSelectionStatus();
        }

        private void ExecuteSkillAction(SkillActionDefinition action, SkillActionTarget target)
        {
            if (!game.TryUseSkillAction(action, target))
            {
                return;
            }

            if (skillActionWindow.Visible)
            {
                skillActionWindow.RefreshEntries();
            }
        }

        private void CancelMoveSelection()
        {
            isMoveSelection = false;
            game.ClearOverlayCells();
            Events.RaiseGameMessage("Move mode cancelled.");
            ClearSelectionStatus();
        }

        private void CancelAttackSelection()
        {
            isAttackSelection = false;
            attackTargets.Clear();
            game.ClearOverlayCells();
            Events.RaiseGameMessage("Attack mode cancelled.");
            ClearSelectionStatus();
        }

        private void CancelTalkSelection()
        {
            isTalkSelection = false;
            talkTargets.Clear();
            selectedTalkTarget = null;
            game.ClearOverlayCells();
            Events.RaiseGameMessage("Talk selection cancelled.");
            ClearSelectionStatus();
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

        private void CycleAttackTarget(int delta)
        {
            if (!isAttackSelection || attackTargets.Count == 0)
            {
                return;
            }

            attackTargetIndex = (attackTargetIndex + delta) % attackTargets.Count;
            if (attackTargetIndex < 0)
            {
                attackTargetIndex += attackTargets.Count;
            }

            UpdateAttackOverlay();
        }

        private void UpdateMovePreview()
        {
            if (game.Player == null)
            {
                return;
            }

            List<Coordinate> path = game.GetMovePath(moveTarget);
            UpdateMoveOverlayAndStatus(path);
            if (path.Count == 0)
            {
                Events.RaiseGameMessage($"No path to {moveTarget}.");
                return;
            }

            int steps = Math.Max(0, path.Count - 1);
            int apCost = game.CalculateMovementApCost(game.Player, path);
            int turns = ActionSequence.CalculateTurnsNeeded(apCost, game.Player.Stats.MaxAp);
            int remainingAp = game.GetRemainingPlannedAp();
            Events.RaiseGameMessage($"Move target {moveTarget}, steps {steps}, cost {apCost}, turns {turns}, remaining AP {remainingAp}.");
        }

        private void UpdateMoveOverlayAndStatus(List<Coordinate> path)
        {
            UpdateMoveOverlay(path);
            UpdateMoveStatus(path);
        }

        private void ConfirmMoveSelection()
        {
            if (game.Player == null)
            {
                return;
            }

            Player player = game.Player;
            List<Coordinate> path = game.GetMovePath(moveTarget);
            if (path.Count <= 1)
            {
                Events.RaiseGameMessage("No movement queued.");
                isMoveSelection = false;
                game.ClearOverlayCells();
                ClearSelectionStatus();
                return;
            }

            int steps = path.Count - 1;
            int stepsQueued = 0;
            for (int i = 1; i <= steps; i++)
            {
                Coordinate from = path[i - 1];
                Coordinate to = path[i];
                Move move = from.ToMove(to);
                if (move is Move.None)
                {
                    break;
                }

                if (!game.TryQueuePlayerMove(move))
                {
                    break;
                }
                stepsQueued++;
            }

            if (stepsQueued <= 0)
            {
                int apCost = game.CalculateMovementApCost(player, path);
                int turns = ActionSequence.CalculateTurnsNeeded(apCost, game.Player.Stats.MaxAp);
                Events.RaiseGameMessage($"Not enough AP. Steps {steps}, cost {apCost}, turns {turns}.");
                isMoveSelection = false;
                game.ClearOverlayCells();
                ClearSelectionStatus();
                return;
            }

            player.SetPlannedPosition(path[stepsQueued]);

            int totalCost = game.CalculateMovementApCost(player, path);
            int turnsNeeded = ActionSequence.CalculateTurnsNeeded(totalCost, game.Player.Stats.MaxAp);
            if (stepsQueued < steps)
            {
                Events.RaiseGameMessage($"Queued {stepsQueued}/{steps} steps towards {moveTarget} (turns needed {turnsNeeded}).");
            }
            else
            {
                Events.RaiseGameMessage($"Queued move to {moveTarget} (turns needed {turnsNeeded}).");
            }

            isMoveSelection = false;
            game.ClearOverlayCells();
            ClearSelectionStatus();
        }

        private void ConfirmAttackSelection()
        {
            if (!isAttackSelection || attackTargets.Count == 0)
            {
                return;
            }

            Enemy selected = attackTargets[attackTargetIndex];
            isAttackSelection = false;
            attackTargets.Clear();
            game.ClearOverlayCells();
            ClearSelectionStatus();
            QueueAttack(selected);
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
                Attribute = SelectionHighlightAttribute
            }));

            game.SetOverlayCells(overlay);
        }

        private void UpdateMoveStatus(IReadOnlyList<Coordinate> path)
        {
            if (game.Player == null)
            {
                return;
            }

            int steps = Math.Max(0, path.Count - 1);
            int apCost = game.CalculateMovementApCost(game.Player, path);
            int turns = ActionSequence.CalculateTurnsNeeded(apCost, game.Player.Stats.MaxAp);
            int remainingAp = game.GetRemainingPlannedAp();
            SetSelectionStatus("Move", $"{moveTarget.X},{moveTarget.Y} steps {steps} cost {apCost} turns {turns} AP {remainingAp}");
        }

        private void UpdateAttackOverlay()
        {
            if (!isAttackSelection || game.Player == null)
            {
                game.ClearOverlayCells();
                return;
            }

            Player player = game.Player;
            Coordinate origin = player.PlannedPosition;
            Weapon weapon = player.EquippedWeapon;
            List<(Coordinate Position, Cell Cell)> overlay = [];
            foreach (Coordinate cell in CombatTargeting.GetRangeCells(game.Context.MapState, origin, weapon))
            {
                overlay.Add((cell, new Cell
                {
                    Character = GUISettings.RangePreviewGlyph,
                    Attribute = ColorPresets.RangePreview
                }));
            }

            if (attackTargets.Count > 0)
            {
                Enemy selected = attackTargets[attackTargetIndex];
                overlay.Add((selected.Position, new Cell
                {
                    Character = GUISettings.TargetPreviewGlyph,
                    Attribute = SelectionHighlightAttribute
                }));
            }

            game.SetOverlayCells(overlay);
            UpdateAttackStatus();
        }

        private void UpdateAttackStatus()
        {
            if (!isAttackSelection || attackTargets.Count == 0)
            {
                return;
            }

            Enemy selected = attackTargets[attackTargetIndex];
            SetSelectionStatus("Attack", $"{selected.Name} {selected.X},{selected.Y} ({attackTargetIndex + 1}/{attackTargets.Count})");
        }

        private void UpdateTalkOverlay()
        {
            if (!isTalkSelection || selectedTalkTarget == null)
            {
                game.ClearOverlayCells();
                return;
            }

            game.SetOverlayCells(new[]
            {
                (selectedTalkTarget.Position, new Cell
                {
                    Character = GUISettings.TargetPreviewGlyph,
                    Attribute = SelectionHighlightAttribute
                })
            });
            UpdateTalkStatus();
        }

        private void UpdateTalkStatus()
        {
            if (!isTalkSelection || selectedTalkTarget == null)
            {
                return;
            }

            SetSelectionStatus("Talk", $"{selectedTalkTarget.Name} {selectedTalkTarget.X},{selectedTalkTarget.Y}");
        }

        private void UpdateSkillTargetOverlay()
        {
            if (!isSkillSelection || selectedSkillAction == null || game.Player == null)
            {
                game.ClearOverlayCells();
                return;
            }

            List<(Coordinate Position, Cell Cell)> overlay = [];
            if (selectedSkillTargetType == SkillActionTargetType.RangedEnemy)
            {
                Weapon weapon = game.Player.EquippedWeapon;
                foreach (Coordinate cell in CombatTargeting.GetRangeCells(game.Context.MapState, game.Player.Position, weapon))
                {
                    overlay.Add((cell, new Cell
                    {
                        Character = GUISettings.RangePreviewGlyph,
                        Attribute = ColorPresets.RangePreview
                    }));
                }
            }

            Coordinate? targetPosition = selectedSkillTargetType switch
            {
                SkillActionTargetType.AdjacentEnemy or SkillActionTargetType.RangedEnemy =>
                    skillEnemyTargets.Count > 0 ? skillEnemyTargets[skillEnemyTargetIndex].Position : null,
                SkillActionTargetType.AdjacentNpc => selectedSkillNpc?.Position,
                SkillActionTargetType.AdjacentAllyOrSelf =>
                    skillPlayerTargets.Count > 0 ? skillPlayerTargets[skillPlayerTargetIndex].Position : null,
                SkillActionTargetType.Direction or SkillActionTargetType.AdjacentDoor => skillTargetPosition,
                _ => null
            };

            if (targetPosition != null)
            {
                overlay.Add((targetPosition, new Cell
                {
                    Character = GUISettings.TargetPreviewGlyph,
                    Attribute = SelectionHighlightAttribute
                }));
            }

            if (overlay.Count == 0)
            {
                game.ClearOverlayCells();
                return;
            }

            game.SetOverlayCells(overlay);
            UpdateSkillStatus();
        }

        private void UpdateSkillStatus()
        {
            if (!isSkillSelection || selectedSkillAction == null)
            {
                return;
            }

            string targetDescription = selectedSkillTargetType switch
            {
                SkillActionTargetType.AdjacentEnemy or SkillActionTargetType.RangedEnemy =>
                    skillEnemyTargets.Count > 0
                        ? BuildIndexedTargetDescription(skillEnemyTargets[skillEnemyTargetIndex].Name, skillEnemyTargets[skillEnemyTargetIndex].X, skillEnemyTargets[skillEnemyTargetIndex].Y, skillEnemyTargetIndex, skillEnemyTargets.Count)
                        : "No target",
                SkillActionTargetType.AdjacentNpc =>
                    selectedSkillNpc != null
                        ? $"{selectedSkillNpc.Name} {selectedSkillNpc.X},{selectedSkillNpc.Y}"
                        : "No target",
                SkillActionTargetType.AdjacentAllyOrSelf =>
                    skillPlayerTargets.Count > 0
                        ? BuildIndexedTargetDescription(skillPlayerTargets[skillPlayerTargetIndex].Name, skillPlayerTargets[skillPlayerTargetIndex].X, skillPlayerTargets[skillPlayerTargetIndex].Y, skillPlayerTargetIndex, skillPlayerTargets.Count)
                        : "No target",
                SkillActionTargetType.AdjacentDoor or SkillActionTargetType.Direction =>
                    $"{skillTargetPosition.X},{skillTargetPosition.Y}",
                _ => string.Empty
            };

            string detail = string.IsNullOrWhiteSpace(targetDescription)
                ? selectedSkillAction.Name
                : $"{selectedSkillAction.Name} -> {targetDescription}";

            SetSelectionStatus("Action", detail);
        }

        private static string BuildIndexedTargetDescription(string name, int x, int y, int index, int count)
        {
            string baseText = $"{name} {x},{y}";
            return count > 1 ? $"{baseText} ({index + 1}/{count})" : baseText;
        }

        private Terminal.Gui.Drawing.Attribute SelectionHighlightAttribute =>
            selectionBlinkOn ? ColorPresets.TargetPreviewHighlight : ColorPresets.TargetPreview;

        private void EnsureSelectionBlinker()
        {
            if (selectionBlinkTimerActive || !Application.Initialized)
            {
                return;
            }

            selectionBlinkTimerActive = true;
            Application.AddTimeout(TimeSpan.FromMilliseconds(350), () =>
            {
                if (!IsSelectionActive())
                {
                    selectionBlinkTimerActive = false;
                    return false;
                }

                selectionBlinkOn = !selectionBlinkOn;
                RefreshSelectionOverlay();
                return true;
            });
        }

        private bool IsSelectionActive()
        {
            return isMoveSelection || isAttackSelection || isTalkSelection || isSkillSelection;
        }

        private void RefreshSelectionOverlay()
        {
            if (isMoveSelection)
            {
                UpdateMoveOverlayAndStatus(game.GetMovePath(moveTarget));
                return;
            }

            if (isAttackSelection)
            {
                UpdateAttackOverlay();
                return;
            }

            if (isTalkSelection)
            {
                UpdateTalkOverlay();
                return;
            }

            if (isSkillSelection)
            {
                UpdateSkillTargetOverlay();
            }
        }

        private void SetSelectionStatus(string mode, string detail)
        {
            uiStatus.SetOverride(mode, detail);
        }

        private void ClearSelectionStatus()
        {
            selectionBlinkOn = false;
            if (utilityWindow.Visible)
            {
                uiStatus.SetOverride("Inventory", "Manage items");
                return;
            }

            if (questLogWindow.Visible)
            {
                uiStatus.SetOverride("Quest Log", "Browse quests");
                return;
            }

            if (skillActionWindow.Visible)
            {
                uiStatus.SetOverride("Skills", "Select an action");
                return;
            }

            uiStatus.ClearOverride();
        }

        private void SelectSkillTargetByDirection(int dx, int dy)
        {
            if (game.Player == null)
            {
                return;
            }

            Coordinate origin = game.Player.Position;
            Coordinate adjacent = origin.Target(dx, dy);

            switch (selectedSkillTargetType)
            {
                case SkillActionTargetType.AdjacentNpc:
                    Npc? npc = game.Context.NpcManager.GetNpcAt(adjacent.X, adjacent.Y);
                    if (npc != null && skillNpcTargets.Contains(npc))
                    {
                        selectedSkillNpc = npc;
                        UpdateSkillTargetOverlay();
                    }
                    break;
                case SkillActionTargetType.AdjacentDoor:
                    if (game.Context.MapState.IsInBounds(adjacent))
                    {
                        Cell cell = game.Context.MapState.GetCell(adjacent.X, adjacent.Y);
                        if (cell.Character == '+')
                        {
                            skillTargetPosition = adjacent;
                            UpdateSkillTargetOverlay();
                        }
                    }
                    break;
                case SkillActionTargetType.Direction:
                    Coordinate landing = origin.Target(dx * 2, dy * 2);
                    if (game.Context.MapState.IsInBounds(landing))
                    {
                        skillTargetPosition = landing;
                        UpdateSkillTargetOverlay();
                    }
                    break;
                default:
                    break;
            }
        }

        private void CycleSkillEnemyTarget(int delta)
        {
            if (skillEnemyTargets.Count == 0)
            {
                return;
            }

            skillEnemyTargetIndex = (skillEnemyTargetIndex + delta) % skillEnemyTargets.Count;
            if (skillEnemyTargetIndex < 0)
            {
                skillEnemyTargetIndex += skillEnemyTargets.Count;
            }

            UpdateSkillTargetOverlay();
        }

        private void CycleSkillPlayerTarget(int delta)
        {
            if (skillPlayerTargets.Count == 0)
            {
                return;
            }

            skillPlayerTargetIndex = (skillPlayerTargetIndex + delta) % skillPlayerTargets.Count;
            if (skillPlayerTargetIndex < 0)
            {
                skillPlayerTargetIndex += skillPlayerTargets.Count;
            }

            UpdateSkillTargetOverlay();
        }

        private List<Enemy> GetAdjacentEnemies(int radius)
        {
            if (game.Player == null)
            {
                return [];
            }

            List<Enemy> result = [];
            foreach (Enemy enemy in game.Context.EnemyManager.Enemies)
            {
                if (enemy.IsDead)
                {
                    continue;
                }

                if (IsWithinRadius(game.Player.Position, enemy.Position, radius))
                {
                    result.Add(enemy);
                }
            }

            return result;
        }

        private List<Coordinate> GetAdjacentDoors(Coordinate origin)
        {
            List<Coordinate> result = [];
            foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                Coordinate target = origin.Target(dx, dy);
                if (!game.Context.MapState.IsInBounds(target))
                {
                    continue;
                }

                Cell cell = game.Context.MapState.GetCell(target.X, target.Y);
                if (cell.Character == '+')
                {
                    result.Add(target);
                }
            }

            return result;
        }

        private static bool IsWithinRadius(Coordinate origin, Coordinate target, int radius)
        {
            int dx = Math.Abs(origin.X - target.X);
            int dy = Math.Abs(origin.Y - target.Y);
            return dx <= radius && dy <= radius;
        }

        private static bool IsAdjacent(Coordinate origin, Coordinate target)
        {
            return IsWithinRadius(origin, target, 1);
        }

        private void SelectTalkTargetByDirection(int dx, int dy)
        {
            if (game.Player == null)
            {
                return;
            }

            Coordinate origin = game.Player.Position;
            Npc? best = null;
            int bestDistance = int.MaxValue;
            foreach (Npc npc in talkTargets)
            {
                int deltaX = npc.X - origin.X;
                int deltaY = npc.Y - origin.Y;
                if (!MatchesDirection(deltaX, deltaY, dx, dy))
                {
                    continue;
                }

                int distance = (deltaX * deltaX) + (deltaY * deltaY);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = npc;
                }
            }

            if (best != null)
            {
                selectedTalkTarget = best;
                UpdateTalkOverlay();
            }
        }

        private static bool MatchesDirection(int deltaX, int deltaY, int dx, int dy)
        {
            if (dx == 0 && dy == -1)
            {
                return deltaY < 0 && Math.Abs(deltaY) >= Math.Abs(deltaX);
            }

            if (dx == 0 && dy == 1)
            {
                return deltaY > 0 && Math.Abs(deltaY) >= Math.Abs(deltaX);
            }

            if (dx == -1 && dy == 0)
            {
                return deltaX < 0 && Math.Abs(deltaX) >= Math.Abs(deltaY);
            }

            if (dx == 1 && dy == 0)
            {
                return deltaX > 0 && Math.Abs(deltaX) >= Math.Abs(deltaY);
            }

            return false;
        }

        private void ConfirmTalkSelection()
        {
            if (selectedTalkTarget == null)
            {
                return;
            }

            StartDialogue(selectedTalkTarget);
            isTalkSelection = false;
            talkTargets.Clear();
            selectedTalkTarget = null;
            game.ClearOverlayCells();
            ClearSelectionStatus();
        }

        private void StartDialogue(Npc npc)
        {
            if (game.TryStartDialogue(npc))
            {
                dialogueWindow.ShowDialogue();
            }
            else
            {
                game.Context.StateMachine.TryChangeState(GameStateType.Exploration);
            }
        }

        private void QueueAttack(Enemy enemy)
        {
            if (game.Player == null)
            {
                return;
            }

            Player player = game.Player;

            if (!game.TryQueuePlayerAction($"Attack {enemy.Name}", player.EquippedWeapon.ApCost, () => player.Attack(enemy)))
            {
                Events.RaiseGameMessage("Not enough AP to queue attack.");
                return;
            }

            Events.RaiseGameMessage($"Queued attack on {enemy.Name}.");
        }

        private List<Enemy> GetAttackTargets()
        {
            if (game.Player == null)
            {
                return [];
            }

            Player player = game.Player;
            Coordinate origin = player.PlannedPosition;
            return CombatTargeting.GetTargetsInRange(game.Context.MapState, origin, game.Context.EnemyManager.Enemies, player.EquippedWeapon);
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
                ScheduleFrameRefresh();
                uiStatus.SetOverride("Inventory", "Manage items");
                return;
            }

            game.Context.StateMachine.TryChangeState(lastNonInventoryState);
            ScheduleFrameRefresh();
            uiStatus.ClearOverride();
        }

        private void ToggleQuestLog()
        {
            if (!questLogWindow.Visible && utilityWindow.Visible)
            {
                utilityWindow.ToggleUtilityWindow();
                SyncStateWithUtilityWindow();
            }

            if (!questLogWindow.Visible && skillActionWindow.Visible)
            {
                skillActionWindow.Toggle();
                SyncStateWithSkillWindow();
            }

            questLogWindow.Toggle();
            SyncStateWithQuestLogWindow();
        }

        private void ToggleSkillWindow()
        {
            if (!skillActionWindow.Visible)
            {
                if (utilityWindow.Visible)
                {
                    utilityWindow.ToggleUtilityWindow();
                    SyncStateWithUtilityWindow();
                }

                if (questLogWindow.Visible)
                {
                    questLogWindow.Toggle();
                    SyncStateWithQuestLogWindow();
                }
            }

            skillActionWindow.Toggle();
            SyncStateWithSkillWindow();
        }

        private void RefreshSkillWindow()
        {
            if (skillActionWindow.Visible)
            {
                skillActionWindow.RefreshEntries();
            }
        }

        private void SyncStateWithQuestLogWindow()
        {
            if (questLogWindow.Visible)
            {
                GameStateType current = game.Context.StateMachine.CurrentState;
                if (current != GameStateType.Menu)
                {
                    lastNonMenuState = current;
                }
                game.Context.StateMachine.TryChangeState(GameStateType.Menu);
                ScheduleFrameRefresh();
                uiStatus.SetOverride("Quest Log", "Browse quests");
                return;
            }

            game.Context.StateMachine.TryChangeState(lastNonMenuState);
            ScheduleFrameRefresh();
            uiStatus.ClearOverride();
        }

        private void SyncStateWithSkillWindow()
        {
            if (skillActionWindow.Visible)
            {
                GameStateType current = game.Context.StateMachine.CurrentState;
                if (current != GameStateType.Menu)
                {
                    lastNonMenuState = current;
                }
                game.Context.StateMachine.TryChangeState(GameStateType.Menu);
                ScheduleFrameRefresh();
                uiStatus.SetOverride("Skills", "Select an action");
                return;
            }

            game.Context.StateMachine.TryChangeState(lastNonMenuState);
            ScheduleFrameRefresh();
            uiStatus.ClearOverride();
        }

        private void ScheduleFrameRefresh()
        {
            if (!Application.Initialized)
            {
                game.RefreshFrame();
                return;
            }

            Application.AddTimeout(TimeSpan.Zero, () =>
            {
                Application.LayoutAndDraw(true);
                game.RefreshFrame();
                Application.LayoutAndDraw(true);
                return false;
            });
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
