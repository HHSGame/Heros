using HHSGame.Core.Engine.Config;
using HHSGame.Core.Quests;

namespace HHSGame.Core
{
    /// <summary>
    /// Evaluates win/lose conditions for the game.
    /// Extracted from Game.cs to reduce its size and isolate condition logic.
    /// </summary>
    public sealed class ConditionEvaluator
    {
        public enum ConditionKind
        {
            PlayerDeath,
            AllEnemiesDefeated,
            TurnLimit,
            EnemyCountAtMost,
            EnemyCountAtLeast,
            HasItem,
            ReachMarker,
            CurrencyAtLeast,
            QuestStatus,
            QuestObjectivesComplete,
            QuestReadyToTurnIn,
            AchievementUnlocked
        }

        public sealed record ConditionSpec(ConditionKind Kind, int Value, string Param);

        private readonly GameContext context;
        private readonly IReadOnlyList<ConditionSpec> winConditions;
        private readonly IReadOnlyList<ConditionSpec> loseConditions;

        public ConditionEvaluator(GameContext context, ConditionConfig config)
        {
            this.context = context;
            winConditions = BuildConditions(config, isWin: true);
            loseConditions = BuildConditions(config, isWin: false);
        }

        /// <summary>
        /// Checks if any game-end condition is met. Returns true if the game should end.
        /// </summary>
        public (bool IsMet, bool IsWin) CheckGameConditions(IReadOnlyList<Player> players)
        {
            if (IsConditionMet(loseConditions, players))
            {
                return (true, false);
            }

            if (IsConditionMet(winConditions, players))
            {
                return (true, true);
            }

            return (false, false);
        }

        private bool IsConditionMet(IReadOnlyList<ConditionSpec> conditions, IReadOnlyList<Player> players)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return false;
            }

            foreach (ConditionSpec condition in conditions)
            {
                if (IsConditionMet(condition, players))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsConditionMet(ConditionSpec condition, IReadOnlyList<Player> players)
        {
            return condition.Kind switch
            {
                ConditionKind.PlayerDeath => AnyPlayerDead(players),
                ConditionKind.AllEnemiesDefeated => context.EnemyManager.Enemies.Count == 0,
                ConditionKind.TurnLimit => context.TurnManager.TurnCount >= condition.Value,
                ConditionKind.EnemyCountAtMost => context.EnemyManager.Enemies.Count <= condition.Value,
                ConditionKind.EnemyCountAtLeast => context.EnemyManager.Enemies.Count >= condition.Value,
                ConditionKind.HasItem => HasItem(condition.Param, condition.Value),
                ConditionKind.ReachMarker => IsAtMarker(condition.Param),
                ConditionKind.CurrencyAtLeast => context.InventoryManager.Currency >= condition.Value,
                ConditionKind.QuestStatus => IsQuestStatus(condition.Param, condition.Value),
                ConditionKind.QuestObjectivesComplete => context.QuestManager.AreQuestObjectivesComplete(condition.Param),
                ConditionKind.QuestReadyToTurnIn => context.QuestManager.CanCompleteQuest(condition.Param),
                ConditionKind.AchievementUnlocked => context.QuestManager.IsAchievementUnlocked(condition.Param),
                _ => false
            };
        }

        public static IReadOnlyList<ConditionSpec> BuildConditions(ConditionConfig config, bool isWin)
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

        public static ConditionSpec ParseCondition(string? id, int? value, string? param)
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
                ConditionKind.CurrencyAtLeast => value.GetValueOrDefault() > 0
                    ? new ConditionSpec(kind, value.GetValueOrDefault(), string.Empty)
                    : throw new InvalidDataException("CurrencyAtLeast requires a positive value."),
                ConditionKind.QuestStatus => BuildParamCondition(kind, value, param),
                ConditionKind.QuestObjectivesComplete => BuildParamCondition(kind, value, param),
                ConditionKind.QuestReadyToTurnIn => BuildParamCondition(kind, value, param),
                ConditionKind.AchievementUnlocked => BuildParamCondition(kind, value, param),
                ConditionKind.ReachMarker => value.GetValueOrDefault() == 0
                    ? BuildMarkerCondition(kind, param)
                    : throw new InvalidDataException("ReachMarker does not accept a value."),
                _ => (value.HasValue && value.Value != 0) || !string.IsNullOrWhiteSpace(param)
                    ? throw new InvalidDataException($"Condition '{kind}' does not accept a value or param.")
                    : new ConditionSpec(kind, 0, string.Empty)
            };
        }

        private static ConditionSpec BuildParamCondition(ConditionKind kind, int? value, string? param)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new InvalidDataException($"{kind} requires a param.");
            }

            return new ConditionSpec(kind, value.GetValueOrDefault(), param.Trim());
        }

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

        private static bool AnyPlayerDead(IReadOnlyList<Player> players)
        {
            foreach (Player player in players)
            {
                if (player.Stats.CurrentHp <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsQuestStatus(string questId, int statusValue)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return false;
            }
            if (!context.QuestManager.TryGetQuest(questId, out QuestState? quest) || quest == null)
            {
                return false;
            }
            return (int)quest.Status == statusValue;
        }

        private bool IsAtMarker(string marker)
        {
            Player? player = context.PlayerOrNull;
            if (player == null || string.IsNullOrWhiteSpace(marker))
            {
                return false;
            }

            char symbol = marker.Trim()[0];
            foreach ((Coordinate Position, char Symbol) entry in context.MapState.SpecialPositions)
            {
                if (entry.Symbol == symbol && entry.Position.Equals(player.Position))
                {
                    return true;
                }
            }

            return false;
        }
    }
}