using System.IO;
using HHSGame.Core.Combat;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.UI;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core
{
    public partial class Game(ILogger<Game> logger, GameContext context, GameWorld world, ClassCatalog classCatalog, GameConfig config)
    {
        public Player? Player { get; private set; }
        private bool isRunning;
        private readonly List<Player> controlledPlayers = [];
        private int activePlayerIndex;
        private readonly List<(Coordinate Position, Cell Cell)> overlayCells = [];
        private bool manualCombatMode;
        private readonly ClassCatalog classCatalog = classCatalog;
        private readonly IReadOnlyList<ConditionSpec> winConditions = BuildConditions(config.Conditions, isWin: true);
        private readonly IReadOnlyList<ConditionSpec> loseConditions = BuildConditions(config.Conditions, isWin: false);

        public GameContext Context => context;
        public int ControlledPlayerCount => controlledPlayers.Count;

        public void Start()
        {
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

            LogStartup(logger, "Intializing Context");
            context.InitializeContext(controlledPlayers, Player);
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

            Player.ActionSequence.Enqueue(new QueuedAction(description, apCost, action));
            Events.RaiseActionSequenceChanged();
            return true;
        }

        public void ClearPlayerActions()
        {
            foreach (Player player in controlledPlayers)
            {
                player.ActionSequence.Clear();
            }
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
            foreach (Player player in controlledPlayers)
            {
                foreach (QueuedAction action in player.ActionSequence.Snapshot())
                {
                    descriptions.Add($"{player.Name} - {action.Description}");
                }
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

            return PerformPlayerAction(() => Player.Move(move), ActionCosts.Movement, false, allowedStates);
        }

        public void Stop()
        {
            isRunning = false;
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
                foreach (Player player in controlledPlayers)
                {
                    player.ActionSequence.Execute(player.Stats, false, _ => Events.RaiseActionSequenceChanged());
                    player.ActionSequence.Clear();
                    player.EndTurn();
                }
                Events.RaiseActionSequenceChanged();
            }
            else
            {
                foreach (Player player in controlledPlayers)
                {
                    player.ActionSequence.Clear();
                }
                Events.RaiseActionSequenceChanged();
            }
            context.TurnManager.EndPlayerTurn();
            world.Update(Player, useAp);
            context.TurnManager.EndEnemyTurn();
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

        private bool CheckGameConditions()
        {
            if (Player == null)
            {
                return false;
            }

            if (IsConditionMet(loseConditions))
            {
                Stop();
                return true;
            }

            if (IsConditionMet(winConditions))
            {
                Stop();
                return true;
            }

            return false;
        }

        private bool IsConditionMet(IReadOnlyList<ConditionSpec> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            foreach (ConditionSpec condition in conditions)
            {
                if (IsConditionMet(condition))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsConditionMet(ConditionSpec condition)
        {
            return condition.Kind switch
            {
                ConditionKind.PlayerDeath => AnyPlayerDead(),
                ConditionKind.AllEnemiesDefeated => context.EnemyManager.Enemies.Count == 0,
                ConditionKind.TurnLimit => context.TurnManager.TurnCount >= condition.Value,
                ConditionKind.EnemyCountAtMost => context.EnemyManager.Enemies.Count <= condition.Value,
                ConditionKind.EnemyCountAtLeast => context.EnemyManager.Enemies.Count >= condition.Value,
                ConditionKind.HasItem => HasItem(condition.Param, condition.Value),
                ConditionKind.ReachMarker => IsAtMarker(condition.Param),
                _ => false
            };
        }

        private static List<ConditionSpec> BuildConditions(ConditionConfig config, bool isWin)
        {
            List<ConditionSpec> result = [];
            IReadOnlyList<string> conditionIds = isWin ? config.Win : config.Lose;
            IReadOnlyList<ConditionEntryConfig> entries = isWin ? config.WinEntries : config.LoseEntries;

            if (conditionIds != null)
            {
                foreach (string conditionId in conditionIds)
                {
                    if (string.IsNullOrWhiteSpace(conditionId))
                    {
                        continue;
                    }

                    result.Add(ParseCondition(conditionId, null, null));
                }
            }

            if (entries != null)
            {
                foreach (ConditionEntryConfig entry in entries)
                {
                    result.Add(ParseCondition(entry.Id, entry.Value, entry.Param));
                }
            }

            return result;
        }

        private static ConditionSpec ParseCondition(string? id, int? value, string? param)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidDataException("Condition id cannot be empty.");
            }

            if (!Enum.TryParse(id.Trim(), true, out ConditionKind kind))
            {
                throw new InvalidDataException($"Unknown condition '{id}'.");
            }

            return kind switch
            {
                ConditionKind.TurnLimit => value.GetValueOrDefault() > 0
                    ? new ConditionSpec(kind, value.GetValueOrDefault(), string.Empty)
                    : throw new InvalidDataException("TurnLimit condition requires a positive value."),
                ConditionKind.EnemyCountAtMost => value.GetValueOrDefault() >= 0
                    ? new ConditionSpec(kind, value.GetValueOrDefault(), string.Empty)
                    : throw new InvalidDataException("EnemyCountAtMost requires a non-negative value."),
                ConditionKind.EnemyCountAtLeast => value.GetValueOrDefault() >= 0
                    ? new ConditionSpec(kind, value.GetValueOrDefault(), string.Empty)
                    : throw new InvalidDataException("EnemyCountAtLeast requires a non-negative value."),
                ConditionKind.HasItem => BuildItemCondition(kind, value, param),
                ConditionKind.ReachMarker => value.GetValueOrDefault() == 0
                    ? BuildMarkerCondition(kind, param)
                    : throw new InvalidDataException("ReachMarker does not accept a value."),
                _ => (value.HasValue && value.Value != 0) || !string.IsNullOrWhiteSpace(param)
                    ? throw new InvalidDataException($"Condition '{kind}' does not accept a value or param.")
                    : new ConditionSpec(kind, 0, string.Empty)
            };
        }

        private enum ConditionKind
        {
            PlayerDeath,
            AllEnemiesDefeated,
            TurnLimit,
            EnemyCountAtMost,
            EnemyCountAtLeast,
            HasItem,
            ReachMarker
        }

        private sealed record ConditionSpec(ConditionKind Kind, int Value, string Param);

        private static ConditionSpec BuildMarkerCondition(ConditionKind kind, string? param)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new InvalidDataException("ReachMarker requires a marker param.");
            }

            return new ConditionSpec(kind, 0, param.Trim());
        }

        private static ConditionSpec BuildItemCondition(ConditionKind kind, int? value, string? param)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new InvalidDataException("HasItem requires an item id param.");
            }

            int minCount = value.GetValueOrDefault();
            if (minCount <= 0)
            {
                minCount = 1;
            }

            return new ConditionSpec(kind, minCount, param.Trim());
        }

        private bool HasItem(string itemId, int minCount)
        {
            if (Player == null)
            {
                return false;
            }

            int count = 0;
            foreach (Items.Item item in context.InventoryManager.GetItems())
            {
                if (string.Equals(item.Id, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    if (count >= minCount)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool AnyPlayerDead()
        {
            foreach (Player player in controlledPlayers)
            {
                if (player.Stats.CurrentHp <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAtMarker(string marker)
        {
            if (Player == null || string.IsNullOrWhiteSpace(marker))
            {
                return false;
            }

            char symbol = marker.Trim()[0];
            foreach ((Coordinate Position, char Symbol) entry in context.MapState.SpecialPositions)
            {
                if (entry.Symbol == symbol && entry.Position.Equals(Player.Position))
                {
                    return true;
                }
            }

            return false;
        }

        private void RenderFrame()
        {
            if (world == null || Player == null)
            {
                return;
            }

            UI.IDrawingContext drawingContext = context.DrawingContext;

            drawingContext.Viewport.UpdateViewport(Player.X, Player.Y, context.MapState.Width, context.MapState.Height);

            world.Draw(drawingContext);
            foreach ((Coordinate position, Cell cell) in overlayCells)
            {
                drawingContext.DrawAt((position.X, position.Y), cell);
            }
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
            return context.EnemyManager.Enemies.Any(enemy => context.MapState.IsVisible(enemy.X, enemy.Y));
        }

    }
}
