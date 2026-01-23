using System.IO;
using System.Threading;
using HHSGame.Core.Combat;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.SkillActions;
using HHSGame.Core.Stats;
using HHSGame.UI;
using Terminal.Gui.App;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core
{
    public partial class Game(ILogger<Game> logger, GameContext context, GameWorld world, ClassCatalog classCatalog, GameConfig config)
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
                bool queued = TryQueuePlayerAction(action.Name, action.ApCost, () => ExecuteSkillAction(action, target));
                if (!queued)
                {
                    Events.RaiseGameMessage("Not enough AP to queue that skill.");
                }
                return queued;
            }

            return PerformPlayerAction(() => ExecuteSkillAction(action, target), action.ApCost, false, GameStateType.Exploration, GameStateType.Combat);
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

        private void ExecuteSkillAction(SkillActionDefinition action, SkillActionTarget target)
        {
            if (Player == null)
            {
                return;
            }

            switch (action.Id)
            {
                case SkillActionId.Inspect:
                    ExecuteInspect(Player);
                    break;
                case SkillActionId.Search:
                    ExecuteSearch(Player);
                    break;
                case SkillActionId.Sneak:
                    ExecuteSneak(Player);
                    break;
                case SkillActionId.Pickpocket:
                    ExecutePickpocket(Player, target.Npc);
                    break;
                case SkillActionId.Inspire:
                    ExecuteInspire(Player);
                    break;
                case SkillActionId.Shove:
                    ExecuteShove(Player, target.Enemy);
                    break;
                case SkillActionId.Leap:
                    ExecuteLeap(Player, target.Position);
                    break;
                case SkillActionId.Forage:
                    ExecuteForage(Player);
                    break;
                case SkillActionId.Aim:
                    ExecuteAim(Player);
                    break;
                case SkillActionId.SteadyMind:
                    ExecuteSteadyMind(Player);
                    break;
                case SkillActionId.PickLock:
                    ExecutePickLock(Player, target.Position);
                    break;
                case SkillActionId.Appraise:
                    ExecuteAppraise(Player);
                    break;
                case SkillActionId.Demoralize:
                    ExecuteDemoralize(Player, target.Enemy);
                    break;
                case SkillActionId.TreatWounds:
                    ExecuteTreatWounds(Player, target.Player ?? Player);
                    break;
                case SkillActionId.Bless:
                    ExecuteBless(Player);
                    break;
                default:
                    break;
            }
        }

        private void ExecuteInspect(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Awareness);
            int radius = Math.Clamp(1 + (skillValue / 25), 1, 4);
            List<HiddenFeature> revealed = context.MapState.RevealHiddenFeatures(player.Position, radius);
            if (revealed.Count == 0)
            {
                Events.RaiseGameMessage("No hidden features detected.");
                return;
            }

            Events.RaiseGameMessage($"Discovered {revealed.Count} hidden feature(s).");
            foreach (HiddenFeature feature in revealed)
            {
                if (!string.IsNullOrWhiteSpace(feature.Description))
                {
                    Events.RaiseGameMessage(feature.Description);
                }
            }
        }

        private void ExecuteSearch(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Knowledge);
            int radius = Math.Clamp(1 + (skillValue / 40), 1, 2);
            List<Item> revealed = context.ItemManager.RevealHiddenItems(player.Position, radius);
            List<Item> items = context.ItemManager.GetItemsInRadius(player.Position, radius, includeHidden: false);

            if (revealed.Count == 0 && items.Count == 0)
            {
                Events.RaiseGameMessage("You find nothing of interest.");
                return;
            }

            if (revealed.Count > 0)
            {
                string foundNames = string.Join(", ", revealed.Select(item => item.Name));
                Events.RaiseGameMessage($"Revealed hidden items: {foundNames}.");
            }

            if (items.Count > 0)
            {
                string itemNames = string.Join(", ", items.Select(item => item.Name));
                Events.RaiseGameMessage($"Items nearby: {itemNames}.");
            }
        }

        private void ExecuteSneak(Player player)
        {
            player.ToggleSneak();
            Events.RaiseGameMessage(player.IsSneaking ? "You move silently." : "You stop sneaking.");
        }

        private void ExecutePickpocket(Player player, Npc? npc)
        {
            if (npc == null)
            {
                Events.RaiseGameMessage("No target to pickpocket.");
                return;
            }

            if (!IsAdjacent(player.Position, npc.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Dexterity);
            int detection = npc.Stats.Attributes.Perception
                + npc.Stats.Skills.GetValue(SkillType.Awareness, npc.Stats.Attributes);
            SkillCheckResult attempt = SkillChecks.Roll(skillValue, detection + 10, context.Random);

            if (!attempt.Success)
            {
                SkillCheckResult detected = SkillChecks.Roll(detection, skillValue + 10, context.Random);
                Events.RaiseGameMessage(detected.Success ? "You were spotted!" : "You fail to steal anything.");
                return;
            }

            if (npc.Inventory.Count == 0)
            {
                Events.RaiseGameMessage("Nothing to steal.");
                return;
            }

            int index = context.Random.Next(0, npc.Inventory.Count);
            Item stolen = npc.Inventory.ObservableItems.ElementAt(index);
            npc.Inventory.RemoveItem(stolen);
            player.AddItem(stolen);
            Events.RaiseGameMessage($"Stole {stolen.Name} from {npc.Name}.");
        }

        private void ExecuteInspire(Player player)
        {
            if (context.PartyState.InspireActive)
            {
                Events.RaiseGameMessage("The party is already inspired.");
                return;
            }

            context.PartyState.ApplyInspire(player, 1, 1);
            player.SetSkillLocked(true);
            Events.RaiseGameMessage($"{player.Name} inspires the team. Party checks +1 today.");
        }

        private void ExecuteShove(Player player, Enemy? enemy)
        {
            if (enemy == null)
            {
                Events.RaiseGameMessage("No target to shove.");
                return;
            }

            if (!IsAdjacent(player.Position, enemy.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Melee);
            int defense = enemy.Stats.Attributes.Strength
                + enemy.Stats.Skills.GetValue(SkillType.Athletics, enemy.Stats.Attributes);
            SkillCheckResult check = SkillChecks.Roll(skillValue, defense + 10, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("Shove failed.");
                return;
            }

            int dx = Math.Sign(enemy.X - player.X);
            int dy = Math.Sign(enemy.Y - player.Y);
            Coordinate destination = enemy.Position.Target(dx, dy);
            if (!context.CollisionSystem.CanMoveTo(destination.X, destination.Y, enemy))
            {
                Events.RaiseGameMessage("No room to push the target.");
                return;
            }

            enemy.X = destination.X;
            enemy.Y = destination.Y;
            Events.RaiseGameMessage($"You shove {enemy.Name} back.");
        }

        private void ExecuteLeap(Player player, Coordinate? targetPosition)
        {
            if (targetPosition == null)
            {
                Events.RaiseGameMessage("No direction chosen.");
                return;
            }

            Coordinate target = targetPosition;
            int dx = target.X - player.X;
            int dy = target.Y - player.Y;
            if (!IsValidLeap(dx, dy))
            {
                Events.RaiseGameMessage("Invalid leap direction.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Athletics);
            SkillCheckResult check = SkillChecks.Roll(skillValue, 18, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("You fail to leap.");
                return;
            }

            Direction? direction = ToDirection(dx, dy);
            if (!direction.HasValue)
            {
                Events.RaiseGameMessage("Invalid leap direction.");
                return;
            }

            Move.Forward step = new(direction.Value);
            Coordinate start = player.Position;
            player.Move(step);
            if (player.Position.Equals(start))
            {
                Events.RaiseGameMessage("Leap blocked.");
                return;
            }

            Coordinate mid = player.Position;
            player.Move(step);
            if (player.Position.Equals(mid))
            {
                Events.RaiseGameMessage("Leap blocked.");
            }
        }

        private void ExecuteForage(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Survival);
            Cell cell = context.MapState.GetCell(player.X, player.Y);
            int difficulty = cell.Character is '*' or '.' ? 18 : 25;
            SkillCheckResult check = SkillChecks.Roll(skillValue, difficulty, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("You fail to find usable supplies.");
                return;
            }

            if (!context.ItemCatalog.TryCreateItem("HealthPotion", out Item item))
            {
                Events.RaiseGameMessage("You find nothing useful.");
                return;
            }

            player.AddItem(item);
            Events.RaiseGameMessage("You forage a health potion.");
        }

        private void ExecuteAim(Player player)
        {
            if (!WeaponRules.IsRanged(player.EquippedWeapon.WeaponType))
            {
                Events.RaiseGameMessage("You need a ranged weapon to aim.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Firearms);
            int bonus = Math.Max(1, skillValue / 20);
            player.SetRangedAccuracyBonus(bonus);
            Events.RaiseGameMessage($"You steady your aim (+{bonus} accuracy).");
        }

        private void ExecuteSteadyMind(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Resolution);
            int restore = Math.Max(1, skillValue / 10);
            int removed = player.RemoveEffects<StunEffect>();
            player.Stats.RestoreSanity(restore);

            string message = $"You regain {restore} sanity.";
            if (removed > 0)
            {
                message += " The stun wears off.";
            }
            Events.RaiseGameMessage(message);
        }

        private void ExecutePickLock(Player player, Coordinate? targetPosition)
        {
            if (targetPosition == null)
            {
                Events.RaiseGameMessage("No door selected.");
                return;
            }

            Coordinate target = targetPosition;
            if (!context.MapState.IsInBounds(target))
            {
                Events.RaiseGameMessage("No door selected.");
                return;
            }

            Cell cell = context.MapState.GetCell(target.X, target.Y);
            if (cell.Character != '+')
            {
                Events.RaiseGameMessage("That is not a locked door.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Mechanics);
            SkillCheckResult check = SkillChecks.Roll(skillValue, 20, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("Lockpicking failed.");
                return;
            }

            context.MapState.SetCell(target.X, target.Y, new Cell
            {
                Character = '.',
                Attribute = Map.TilePresets.GetTerrainColor('.')
            });
            Events.RaiseGameMessage("The lock clicks open.");
        }

        private void ExecuteAppraise(Player player)
        {
            List<Item> groundItems = context.ItemManager.GetItemsAt(player.X, player.Y, includeHidden: false);
            List<Item> inventoryItems = player.Inventory.GetItems().ToList();

            if (groundItems.Count == 0 && inventoryItems.Count == 0)
            {
                Events.RaiseGameMessage("You have nothing to appraise.");
                return;
            }

            if (groundItems.Count > 0)
            {
                string groundSummary = string.Join(", ", groundItems.Select(item => $"{item.Name}({item.Value})"));
                Events.RaiseGameMessage($"On the ground: {groundSummary}.");
            }

            if (inventoryItems.Count > 0)
            {
                string inventorySummary = string.Join(", ", inventoryItems.Select(item => $"{item.Name}({item.Value})"));
                Events.RaiseGameMessage($"Inventory: {inventorySummary}.");
            }
        }

        private void ExecuteDemoralize(Player player, Enemy? enemy)
        {
            if (enemy == null)
            {
                Events.RaiseGameMessage("No target to demoralize.");
                return;
            }

            if (!IsAdjacent(player.Position, enemy.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Persuasion);
            int defense = enemy.Stats.Skills.GetValue(SkillType.Resolution, enemy.Stats.Attributes)
                + enemy.Stats.Attributes.Charisma;
            SkillCheckResult check = SkillChecks.Roll(skillValue, defense + 10, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("Demoralize failed.");
                return;
            }

            enemy.SkipNextTurns(1);
            Events.RaiseGameMessage($"{enemy.Name} hesitates.");
        }

        private void ExecuteTreatWounds(Player player, Player target)
        {
            if (!IsAdjacent(player.Position, target.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Medicine);
            int heal = Math.Max(1, skillValue / 8);
            target.Heal(heal);
            Events.RaiseGameMessage($"{target.Name} recovers {heal} HP.");
        }

        private void ExecuteBless(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Religion);
            int restore = Math.Max(1, skillValue / 10);
            int affected = 0;
            bool removed = false;

            foreach (Player ally in context.PartyState.Players)
            {
                if (!IsAdjacent(player.Position, ally.Position))
                {
                    continue;
                }

                ally.Stats.RestoreSanity(restore);
                if (ally.RemoveEffects<PoisonEffect>() > 0)
                {
                    removed = true;
                }
                affected++;
            }

            if (affected == 0)
            {
                Events.RaiseGameMessage("No one nearby to bless.");
                return;
            }

            string message = $"Blessing restores {restore} sanity.";
            if (removed)
            {
                message += " A toxin fades.";
            }
            Events.RaiseGameMessage(message);
        }

        private int GetEffectiveSkillValue(Player player, SkillType skill)
        {
            return context.PartyState.GetEffectiveSkillValue(player, skill);
        }

        private static bool IsAdjacent(Coordinate origin, Coordinate target)
        {
            int dx = Math.Abs(origin.X - target.X);
            int dy = Math.Abs(origin.Y - target.Y);
            return dx <= 1 && dy <= 1;
        }

        private static bool IsValidLeap(int dx, int dy)
        {
            if (dx != 0 && dy != 0)
            {
                return false;
            }

            return Math.Abs(dx) == 2 || Math.Abs(dy) == 2;
        }

        private static Direction? ToDirection(int dx, int dy)
        {
            return (dx, dy) switch
            {
                (0, -2) => Direction.Up,
                (0, 2) => Direction.Down,
                (-2, 0) => Direction.Left,
                (2, 0) => Direction.Right,
                _ => null
            };
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

            if (IsConditionMet(loseConditions))
            {
                Events.RaiseGameMessage("Defeat! Press Ctrl+Q to quit.");
                Stop();
                return true;
            }

            if (IsConditionMet(winConditions))
            {
                Events.RaiseGameMessage("Victory! Press Ctrl+Q to quit.");
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

        private void DrawPlannedDestinations(UI.IDrawingContext drawingContext)
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
            if (!Application.Initialized)
            {
                return;
            }

            Application.LayoutAndDraw(true);
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
                && enemy.CanDetectPlayer(Player));
        }

    }
}
