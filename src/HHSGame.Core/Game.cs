using System.IO;
using System.Threading;
using HHSGame.Core.Combat;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.SkillSystem;
using HHSGame.Core.Stats;
using HHSGame.Core.Rendering;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core
{
    public partial class Game(ILogger<Game> logger, GameContext context, GameWorld world, ClassCatalog classCatalog, GameConfig config, IEventBus eventBus, IApplicationHost? applicationHost = null)
    {
        private sealed record PlannedActionEntry(Player Player, QueuedAction Action);

        public Player? Player { get; private set; }
        private bool isRunning;
        private readonly List<Player> controlledPlayers = [];
        private int activePlayerIndex;
        private readonly List<(Coordinate Position, Cell Cell)> overlayCells = [];
        private readonly List<PlannedActionEntry> plannedActions = [];
        private int plannedActionCursor;
        private readonly Dictionary<Player, int> plannedDiagonalMoves = new();
        private bool manualCombatMode;
        private readonly ClassCatalog classCatalog = classCatalog;
        private readonly SkillActionExecutor skillActionExecutor = new(context);
        private readonly ConditionEvaluator conditionEvaluator = new(context, config.Conditions);

        public GameContext Context => context;
        public int ControlledPlayerCount => controlledPlayers.Count;

        public void Start()
        {
            Events.Initialize(eventBus);
            controlledPlayers.Clear();
            if (context.Parameters.PlayerSpawns.Count > 0)
            {
                world.InitializeMap();
                CreatePlayersFromSpawns();
            }
            else
            {
                Classes.ClassConfig classConfig = context.Parameters.PlayerClass ?? classCatalog.GetDefault();
                Player = world.NewPlayer(classConfig.ToClass());
                controlledPlayers.Add(Player);
                activePlayerIndex = 0;
            }

            if (Player == null)
            {
                throw new InvalidDataException("No player was created.");
            }

            LogStartup(logger, "Initializing Context");
            context.InitializeContext(controlledPlayers, Player);
            UpdateActivePlayer(Player);
            isRunning = true;
            context.StateMachine.TryChangeState(GameStateType.Exploration);
            foreach (Player player in controlledPlayers)
            {
                player.ResetTurn(false, player == Player);
            }
            context.TurnManager.BeginPlayerTurn();

            LogStartup(logger, "Player setup");
            if (context.Parameters.PlayerSpawns.Count == 0)
            {
                AddStartingItems(context.ItemCatalog, Player, context.Parameters.StartingItems);
            }
            AddMapItems(context, context.Parameters.MapItems);

            Events.OnGameMessageEvent += (sender, e) =>
            {
                LogMessage(logger, e.Message);
            };

            RenderFrame();
        }

        private void CreatePlayersFromSpawns()
        {
            HashSet<Coordinate> occupied = [];
            foreach (PlayerSpawn spawn in context.Parameters.PlayerSpawns)
            {
                Coordinate position = spawn.Position;
                if (!context.MapState.IsWalkable(position))
                {
                    throw new InvalidDataException($"Player spawn is not walkable at {position}.");
                }

                if (occupied.Contains(position))
                {
                    throw new InvalidDataException($"Duplicate player spawn at {position}.");
                }

                Player player = world.CreatePlayer(
                    spawn.ClassConfig,
                    position,
                    spawn.Attributes,
                    spawn.Skills,
                    spawn.Name,
                    spawn.Glyph == '\0' ? null : spawn.Glyph);
                AddStartingItems(context.ItemCatalog, player, spawn.StartingItems);
                controlledPlayers.Add(player);
                occupied.Add(position);
            }

            if (controlledPlayers.Count > 0)
            {
                activePlayerIndex = 0;
                Player = controlledPlayers[0];
            }
        }

        [LoggerMessage(LogLevel.Information, "{message}")]
        public static partial void LogMessage(ILogger logger, string message);
        [LoggerMessage(LogLevel.Information, "Game Starting: {message}")]
        public static partial void LogStartup(ILogger logger, string message);

        public bool PerformPlayerAction(Action action, int apCost, bool endTurn, params GameStateType[] allowedStates)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (!IsStateAllowed(allowedStates))
            {
                return false;
            }

            if (!context.TurnManager.IsPlayerTurn())
            {
                return false;
            }

            bool useAp = context.StateMachine.CurrentState == GameStateType.Combat;
            if (useAp && !Player.Stats.TrySpendAp(apCost))
            {
                return false;
            }

            action();

            RenderFrame();
            if (useAp)
            {
                if (endTurn || Player.Stats.CurrentAp <= 0)
                {
                    EndPlayerTurn();
                }
            }
            else
            {
                EndPlayerTurn();
            }
            return true;
        }

        public bool TryQueuePlayerAction(string description, int apCost, Action action)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (!IsCombatActive())
            {
                return false;
            }

            int remaining = GetRemainingPlannedAp();
            if (apCost > remaining)
            {
                return false;
            }

            QueuedAction queuedAction = new(description, apCost, action);
            Player.ActionSequence.Enqueue(queuedAction);
            plannedActions.Add(new PlannedActionEntry(Player, queuedAction));
            Events.RaiseActionSequenceChanged();
            return true;
        }

        public bool TryQueuePlayerMove(Move move)
        {
            if (Player == null)
            {
                return false;
            }

            int apCost = GetMovementApCost(Player, move);
            if (!TryQueuePlayerAction("Move", apCost, () => Player.Move(move)))
            {
                return false;
            }

            RegisterQueuedMove(Player, move);
            return true;
        }

        public void ClearPlayerActions()
        {
            foreach (Player player in controlledPlayers)
            {
                player.ActionSequence.Clear();
                player.ResetPlannedPosition();
            }
            plannedActions.Clear();
            plannedActionCursor = 0;
            plannedDiagonalMoves.Clear();
            Events.RaiseActionSequenceChanged();
        }

        public int GetRemainingPlannedAp()
        {
            if (Player == null)
            {
                return 0;
            }

            return Player.ActionSequence.RemainingApForTurn(Player.Stats.MaxAp);
        }

        public int GetPlannedApCost()
        {
            return Player?.ActionSequence.TotalCost ?? 0;
        }

        public List<Coordinate> GetMovePath(Coordinate destination)
        {
            if (Player == null)
            {
                return [];
            }

            if (!Context.MapState.IsWalkable(destination))
            {
                return [];
            }

            Pathfinder pathfinder = new(Context.MapState);
            return pathfinder.FindPath(Player.PlannedPosition, destination);
        }

        public int CalculateMovementApCost(Player player, IReadOnlyList<Coordinate> path)
        {
            if (path.Count <= 1)
            {
                return 0;
            }

            int diagonalCount = GetPlannedDiagonalMoves(player);
            int totalCost = 0;
            for (int i = 1; i < path.Count; i++)
            {
                Coordinate from = path[i - 1];
                Coordinate to = path[i];
                if (IsDiagonalStep(from, to))
                {
                    totalCost += GetDiagonalStepCost(diagonalCount);
                    diagonalCount++;
                }
                else
                {
                    totalCost += ActionCosts.Movement;
                }
            }

            return totalCost;
        }

        private int GetMovementApCost(Player player, Move move)
        {
            if (move is Move.Diagonal)
            {
                int diagonalCount = GetPlannedDiagonalMoves(player);
                return GetDiagonalStepCost(diagonalCount);
            }

            return ActionCosts.Movement;
        }

        private void RegisterQueuedMove(Player player, Move move)
        {
            if (move is Move.Diagonal)
            {
                plannedDiagonalMoves[player] = GetPlannedDiagonalMoves(player) + 1;
            }
        }

        private int GetPlannedDiagonalMoves(Player player)
        {
            return plannedDiagonalMoves.TryGetValue(player, out int value) ? value : 0;
        }

        private static int GetDiagonalStepCost(int diagonalCount)
        {
            return diagonalCount % 2 == 0 ? 2 : 1;
        }

        private static bool IsDiagonalStep(Coordinate from, Coordinate to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            return dx == 1 && dy == 1;
        }

        public bool TryUseSkillAction(SkillActionDefinition action, SkillActionTarget target)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (Player.IsSkillLocked && action.Id != SkillActionId.Inspire)
            {
                Events.RaiseGameMessage("You are too drained to use more skills today.");
                return false;
            }

            if (IsCombatActive())
            {
                bool queued = TryQueuePlayerAction(action.Name, action.ApCost, () => skillActionExecutor.Execute(action, target, Player!));
                if (!queued)
                {
                    Events.RaiseGameMessage("Not enough AP to queue that skill.");
                }
                return queued;
            }

            return PerformPlayerAction(() => skillActionExecutor.Execute(action, target, Player!), action.ApCost, false, GameStateType.Exploration, GameStateType.Combat);
        }

        public void RefreshFrame()
        {
            if (!isRunning || Player == null)
            {
                return;
            }

            RenderFrame();
        }

        public void CommitPlayerActions()
        {
            if (!isRunning || Player == null)
            {
                return;
            }

            EndPlayerTurn();
        }

        public IReadOnlyList<string> GetPlannedActionDescriptions()
        {
            if (!IsCombatActive())
            {
                return Array.Empty<string>();
            }

            List<string> descriptions = [];
            for (int i = plannedActionCursor; i < plannedActions.Count; i++)
            {
                PlannedActionEntry entry = plannedActions[i];
                descriptions.Add($"{entry.Player.Name} - {entry.Action.Description}");
            }

            return descriptions;
        }

        public bool ToggleCombatMode()
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            bool hasVisibleEnemies = HasVisibleEnemies();
            if (manualCombatMode || context.StateMachine.CurrentState == GameStateType.Combat)
            {
                if (hasVisibleEnemies)
                {
                    return false;
                }

                manualCombatMode = false;
                context.StateMachine.TryChangeState(GameStateType.Exploration);
                ClearPlayerActions();
                foreach (Player player in controlledPlayers)
                {
                    player.ResetTurn(false, player == Player);
                }
                RenderFrame();
                return true;
            }

            manualCombatMode = true;
            context.StateMachine.TryChangeState(GameStateType.Combat);
            ClearPlayerActions();
            foreach (Player player in controlledPlayers)
            {
                player.ResetTurn(true, player == Player);
            }
            RenderFrame();
            return true;
        }

        public void SetOverlayCells(IEnumerable<(Coordinate Position, Cell Cell)> cells)
        {
            overlayCells.Clear();
            overlayCells.AddRange(cells);
            RenderFrame();
        }

        public void ClearOverlayCells()
        {
            if (overlayCells.Count == 0)
            {
                return;
            }

            overlayCells.Clear();
            RenderFrame();
        }

        public bool SwitchControlledPlayer(int direction)
        {
            if (Player == null || controlledPlayers.Count <= 1)
            {
                return false;
            }

            int nextIndex = activePlayerIndex + direction;
            if (nextIndex < 0)
            {
                nextIndex = controlledPlayers.Count - 1;
            }
            else if (nextIndex >= controlledPlayers.Count)
            {
                nextIndex = 0;
            }

            if (nextIndex == activePlayerIndex)
            {
                return false;
            }

            activePlayerIndex = nextIndex;
            Player = controlledPlayers[activePlayerIndex];
            context.SetActivePlayer(Player);
            UpdateActivePlayer(Player);
            Player.UpdateFOV();
            RenderFrame();
            return true;
        }

        public bool IsCombatActive()
        {
            if (manualCombatMode)
            {
                return true;
            }

            if (context.StateMachine.CurrentState == GameStateType.Combat)
            {
                return true;
            }

            return HasVisibleEnemies();
        }

        public bool TryMovePlayer(Move move, params GameStateType[] allowedStates)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (!IsStateAllowed(allowedStates))
            {
                return false;
            }

            Coordinate oldPosition = Player.Position;
            Coordinate target = Player.Position.Move(move.ToVec());
            Enemy? enemy = context.EnemyManager.GetEnemyAt(target.X, target.Y);
            if (enemy != null)
            {
                return PerformPlayerAction(
                    () => Player.Attack(enemy),
                    Player.EquippedWeapon.ApCost,
                    false,
                    allowedStates);
            }

            bool moved = PerformPlayerAction(() => Player.Move(move), ActionCosts.Movement, false, allowedStates);

            // 检查移动触发器
            if (moved)
            {
                context.TriggerManager.OnPlayerMoved(oldPosition.X, oldPosition.Y, Player.X, Player.Y, context.TurnManager.GetTurnNumber());
            }

            return moved;
        }

        public bool TryStartDialogue(Npc npc)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (IsCombatActive())
            {
                return false;
            }

            if (!context.StateMachine.TryChangeState(GameStateType.Dialogue))
            {
                return false;
            }

            bool started = context.DialogueManager.TryStartDialogue(npc);
            if (!started)
            {
                context.StateMachine.TryChangeState(GameStateType.Exploration);
            }

            return started;
        }

        public void EndDialogue()
        {
            context.StateMachine.TryChangeState(GameStateType.Exploration);
        }

        public void Stop()
        {
            isRunning = false;
            manualCombatMode = false;
            ClearPlayerActions();
            ClearOverlayCells();
            context.StateMachine.TryChangeState(GameStateType.GameOver);
        }

        private void EndPlayerTurn()
        {
            if (Player == null)
            {
                return;
            }

            bool useAp = context.StateMachine.CurrentState == GameStateType.Combat;
            if (useAp)
            {
                ExecutePlannedPlayerActions();
                foreach (Player player in controlledPlayers)
                {
                    player.ActionSequence.Clear();
                    player.EndTurn();
                    player.ResetPlannedPosition();
                }
                plannedActions.Clear();
                plannedActionCursor = 0;
                plannedDiagonalMoves.Clear();
                Events.RaiseActionSequenceChanged();
            }
            else
            {
                foreach (Player player in controlledPlayers)
                {
                    player.ActionSequence.Clear();
                    player.ResetPlannedPosition();
                }
                plannedActions.Clear();
                plannedActionCursor = 0;
                plannedDiagonalMoves.Clear();
                Events.RaiseActionSequenceChanged();
            }
            context.TurnManager.EndPlayerTurn();
            world.Update(Player, useAp, _ => AnimateStep());
            context.TurnManager.EndEnemyTurn();

            // 检查触发器
            context.TriggerManager.OnTurnEnd(context.TurnManager.GetTurnNumber());

            if (CheckGameConditions())
            {
                RenderFrame();
                return;
            }
            UpdateCombatState();
            bool nextUseAp = context.StateMachine.CurrentState == GameStateType.Combat;
            foreach (Player player in controlledPlayers)
            {
                player.ResetTurn(nextUseAp, player == Player);
            }
            context.TurnManager.BeginPlayerTurn();
            RenderFrame();
        }

        private void ExecutePlannedPlayerActions()
        {
            HashSet<Player> exhaustedPlayers = [];
            while (plannedActionCursor < plannedActions.Count)
            {
                PlannedActionEntry entry = plannedActions[plannedActionCursor];
                plannedActionCursor++;

                if (exhaustedPlayers.Contains(entry.Player))
                {
                    Events.RaiseActionSequenceChanged();
                    continue;
                }

                if (!entry.Player.Stats.TrySpendAp(entry.Action.ApCost))
                {
                    exhaustedPlayers.Add(entry.Player);
                    Events.RaiseActionSequenceChanged();
                    continue;
                }

                entry.Action.Execute();
                Events.RaiseActionSequenceChanged();
                AnimateStep();
            }
        }

        private bool CheckGameConditions()
        {
            if (Player == null)
            {
                return false;
            }

            (bool isMet, bool isWin) = conditionEvaluator.CheckGameConditions(controlledPlayers);
            if (isMet)
            {
                Events.RaiseGameMessage(isWin
                    ? "Victory! Press Ctrl+Q to quit."
                    : "Defeat! Press Ctrl+Q to quit.");
                Stop();
                return true;
            }

            return false;
        }

        private void RenderFrame()
        {
            if (world == null || Player == null)
            {
                return;
            }

            IDrawingContext drawingContext = context.DrawingContext;

            drawingContext.Viewport.UpdateViewport(Player.X, Player.Y, context.MapState.Width, context.MapState.Height);
            drawingContext.Clear();

            world.Draw(drawingContext);
            foreach ((Coordinate position, Cell cell) in overlayCells)
            {
                drawingContext.DrawAt((position.X, position.Y), cell);
            }
            DrawPlannedDestinations(drawingContext);
            foreach (Player player in controlledPlayers)
            {
                if (player != Player)
                {
                    (player as IGameActor).Draw(drawingContext);
                }
            }
            (Player as IGameActor).Draw(drawingContext);

            drawingContext.Render();
        }

        private void DrawPlannedDestinations(IDrawingContext drawingContext)
        {
            foreach (Player player in controlledPlayers)
            {
                if (player.PlannedPosition.Equals(player.Position))
                {
                    continue;
                }

                Coordinate position = player.PlannedPosition;
                drawingContext.DrawAt((position.X, position.Y), new Cell
                {
                    Character = GUISettings.PlannedDestinationGlyph,
                    Attribute = ColorPresets.PlannedDestination
                });
            }
        }

        private void AnimateStep()
        {
            RenderFrame();
            if (applicationHost == null || !applicationHost.IsInitialized)
            {
                return;
            }

            applicationHost.LayoutAndDraw(true);
            int delayMs = GUISettings.ActionStepDelayMs;
            if (delayMs > 0)
            {
                Thread.Sleep(delayMs);
            }
        }

        private void UpdateActivePlayer(Player activePlayer)
        {
            foreach (Player player in controlledPlayers)
            {
                player.SetActive(player == activePlayer);
            }
        }

        private static void AddStartingItems(Items.ItemCatalog itemCatalog, Player player, IEnumerable<string> itemIds)
        {
            foreach (string itemId in itemIds)
            {
                if (itemCatalog.TryCreateItem(itemId, out Items.Item item))
                {
                    player.AddItem(item);
                }
            }
        }

        private static void AddMapItems(GameContext context, IEnumerable<MapItemSpawn> items)
        {
            foreach (MapItemSpawn spawn in items)
            {
                if (spawn.Quantity <= 0)
                {
                    continue;
                }

                for (int i = 0; i < spawn.Quantity; i++)
                {
                    if (!context.ItemCatalog.TryCreateItem(spawn.ItemId, out Items.Item item))
                    {
                        continue;
                    }

                    if (!context.MapState.IsWalkable(spawn.Position))
                    {
                        throw new InvalidDataException($"Item spawn {spawn.ItemId} is not walkable at {spawn.Position}.");
                    }

                    item.X = spawn.Position.X;
                    item.Y = spawn.Position.Y;
                    context.ItemManager.AddItem(item);
                }
            }
        }

        private bool IsStateAllowed(GameStateType[] allowedStates)
        {
            if (allowedStates.Length == 0)
            {
                return true;
            }

            GameStateType currentState = context.StateMachine.CurrentState;
            foreach (GameStateType state in allowedStates)
            {
                if (state == currentState)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateCombatState()
        {
            if (Player == null)
            {
                return;
            }

            bool hasVisibleEnemies = HasVisibleEnemies();
            if (hasVisibleEnemies)
            {
                context.StateMachine.TryChangeState(GameStateType.Combat);
            }
            else if (!manualCombatMode && context.StateMachine.CurrentState == GameStateType.Combat)
            {
                context.StateMachine.TryChangeState(GameStateType.Exploration);
            }
        }

        private bool HasVisibleEnemies()
        {
            if (Player == null)
            {
                return false;
            }

            return context.EnemyManager.Enemies.Any(enemy =>
                context.MapState.IsVisible(enemy.X, enemy.Y)
                && enemy.CanDetectPlayer(Player)
                && context.FactionManager.WillAttackPlayer(enemy.Faction));
        }

    }
}
