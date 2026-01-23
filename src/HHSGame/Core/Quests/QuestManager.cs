using HHSGame.Core;
using HHSGame.Core.Items;

namespace HHSGame.Core.Quests
{
    public enum QuestStatus
    {
        Inactive,
        Active,
        Completed,
        Failed
    }

    public sealed class QuestState
    {
        private readonly int[] progress;

        public QuestState(QuestDefinition definition)
        {
            Definition = definition;
            progress = new int[definition.Objectives.Count];
        }

        public QuestDefinition Definition { get; }
        public QuestStatus Status { get; private set; } = QuestStatus.Inactive;
        public IReadOnlyList<int> Progress => progress;

        public bool IsCompleted => Status == QuestStatus.Completed;
        public bool IsActive => Status == QuestStatus.Active;

        public void Start()
        {
            if (Status == QuestStatus.Inactive)
            {
                Status = QuestStatus.Active;
            }
        }

        public void Complete()
        {
            if (Status == QuestStatus.Active)
            {
                Status = QuestStatus.Completed;
            }
        }

        public bool TryAdvanceObjective(QuestObjectiveKind kind, string targetId, int amount)
        {
            if (Status != QuestStatus.Active)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < Definition.Objectives.Count; i++)
            {
                QuestObjectiveDefinition objective = Definition.Objectives[i];
                if (objective.Kind != kind)
                {
                    continue;
                }

                if (!string.Equals(objective.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int current = progress[i];
                int next = Math.Min(objective.RequiredCount, current + Math.Max(1, amount));
                if (next != current)
                {
                    progress[i] = next;
                    changed = true;
                }
            }

            return changed;
        }

        public bool AreObjectivesComplete()
        {
            for (int i = 0; i < Definition.Objectives.Count; i++)
            {
                if (progress[i] < Definition.Objectives[i].RequiredCount)
                {
                    return false;
                }
            }

            return Definition.Objectives.Count > 0;
        }

        public IReadOnlyList<string> BuildObjectiveLines()
        {
            List<string> lines = [];
            for (int i = 0; i < Definition.Objectives.Count; i++)
            {
                QuestObjectiveDefinition objective = Definition.Objectives[i];
                int current = progress[i];
                lines.Add($"{objective.Kind} {objective.TargetId}: {current}/{objective.RequiredCount}");
            }

            return lines;
        }
    }

    public sealed class AchievementState
    {
        private readonly int[] progress;

        public AchievementState(AchievementDefinition definition)
        {
            Definition = definition;
            progress = new int[definition.Objectives.Count];
        }

        public AchievementDefinition Definition { get; }
        public bool IsUnlocked { get; private set; }
        public IReadOnlyList<int> Progress => progress;

        public bool TryAdvanceObjective(QuestObjectiveKind kind, string targetId, int amount)
        {
            if (IsUnlocked)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < Definition.Objectives.Count; i++)
            {
                QuestObjectiveDefinition objective = Definition.Objectives[i];
                if (objective.Kind != kind)
                {
                    continue;
                }

                if (!string.Equals(objective.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int current = progress[i];
                int next = Math.Min(objective.RequiredCount, current + Math.Max(1, amount));
                if (next != current)
                {
                    progress[i] = next;
                    changed = true;
                }
            }

            if (changed && AreObjectivesComplete())
            {
                IsUnlocked = true;
            }

            return changed;
        }

        private bool AreObjectivesComplete()
        {
            for (int i = 0; i < Definition.Objectives.Count; i++)
            {
                if (progress[i] < Definition.Objectives[i].RequiredCount)
                {
                    return false;
                }
            }

            return Definition.Objectives.Count > 0;
        }

        public IReadOnlyList<string> BuildObjectiveLines()
        {
            List<string> lines = [];
            for (int i = 0; i < Definition.Objectives.Count; i++)
            {
                QuestObjectiveDefinition objective = Definition.Objectives[i];
                int current = progress[i];
                lines.Add($"{objective.Kind} {objective.TargetId}: {current}/{objective.RequiredCount}");
            }

            return lines;
        }

        public void Unlock()
        {
            IsUnlocked = true;
        }
    }

    public sealed class QuestManager
    {
        private readonly PartyState partyState;
        private readonly InventoryManager inventoryManager;
        private readonly ItemCatalog itemCatalog;
        private readonly Dictionary<string, QuestState> quests = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AchievementState> achievements = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> readyForTurnIn = new(StringComparer.OrdinalIgnoreCase);

        public QuestManager(PartyState partyState, InventoryManager inventoryManager, ItemCatalog itemCatalog)
        {
            this.partyState = partyState;
            this.inventoryManager = inventoryManager;
            this.itemCatalog = itemCatalog;
        }

        public event EventHandler? QuestLogChanged;

        public IReadOnlyList<QuestState> Quests => quests.Values.ToList();
        public IReadOnlyList<AchievementState> Achievements => achievements.Values.ToList();

        public void LoadDefinitions(IEnumerable<QuestDefinition> questDefinitions, IEnumerable<AchievementDefinition> achievementDefinitions)
        {
            quests.Clear();
            achievements.Clear();
            readyForTurnIn.Clear();

            foreach (QuestDefinition quest in questDefinitions)
            {
                quests[quest.Id] = new QuestState(quest);
            }

            foreach (AchievementDefinition achievement in achievementDefinitions)
            {
                achievements[achievement.Id] = new AchievementState(achievement);
            }

            QuestLogChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool StartQuest(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestState? quest))
            {
                return false;
            }

            if (quest.Status != QuestStatus.Inactive)
            {
                return false;
            }

            quest.Start();
            Events.RaiseGameMessage($"Quest started: {quest.Definition.Name}");
            QuestLogChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool CompleteQuest(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestState? quest))
            {
                return false;
            }

            if (quest.Status != QuestStatus.Active)
            {
                return false;
            }

            if (!quest.AreObjectivesComplete())
            {
                return false;
            }

            quest.Complete();
            ApplyRewards(quest.Definition.Rewards);
            Events.RaiseGameMessage($"Quest completed: {quest.Definition.Name}");
            readyForTurnIn.Remove(questId);
            QuestLogChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool UnlockAchievement(string achievementId)
        {
            if (!achievements.TryGetValue(achievementId, out AchievementState? achievement))
            {
                return false;
            }

            if (achievement.IsUnlocked)
            {
                return false;
            }

            achievement.Unlock();
            Events.RaiseGameMessage($"Achievement unlocked: {achievement.Definition.Name}");
            QuestLogChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void NotifyEnemyDefeated(string enemyId)
        {
            AdvanceObjectives(QuestObjectiveKind.KillEnemy, enemyId, 1);
        }

        public void NotifyItemCollected(string itemId, int amount)
        {
            AdvanceObjectives(QuestObjectiveKind.CollectItem, itemId, amount);
        }

        public void NotifyMarkerReached(string marker)
        {
            AdvanceObjectives(QuestObjectiveKind.ReachMarker, marker, 1);
        }

        public void NotifyNpcTalked(string npcId)
        {
            AdvanceObjectives(QuestObjectiveKind.TalkToNpc, npcId, 1);
        }

        public bool TryGetQuest(string questId, out QuestState quest)
        {
            quest = null!;
            if (string.IsNullOrWhiteSpace(questId))
            {
                return false;
            }

            return quests.TryGetValue(questId.Trim(), out quest) && quest != null;
        }

        public bool IsQuestInStatus(string questId, QuestStatus status)
        {
            return TryGetQuest(questId, out QuestState quest) && quest.Status == status;
        }

        public bool AreQuestObjectivesComplete(string questId)
        {
            return TryGetQuest(questId, out QuestState quest) && quest.AreObjectivesComplete();
        }

        public bool CanCompleteQuest(string questId)
        {
            return TryGetQuest(questId, out QuestState quest)
                && quest.Status == QuestStatus.Active
                && quest.AreObjectivesComplete();
        }

        public void GiveItemReward(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Player? player = partyState.ActivePlayer;
            if (player == null)
            {
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                if (!itemCatalog.TryCreateItem(itemId, out Item reward))
                {
                    continue;
                }

                player.AddItem(reward);
                NotifyItemCollected(reward.Id, 1);
            }
        }

        public void GiveCurrencyReward(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            inventoryManager.AddCurrency(amount);
            Events.RaiseGameMessage($"Received {amount} credits.");
        }

        private void ApplyRewards(IReadOnlyList<QuestRewardDefinition> rewards)
        {
            foreach (QuestRewardDefinition reward in rewards)
            {
                switch (reward.Kind)
                {
                    case QuestRewardKind.Item:
                        GiveItemReward(reward.TargetId, reward.Amount <= 0 ? 1 : reward.Amount);
                        break;
                    case QuestRewardKind.Currency:
                        GiveCurrencyReward(reward.Amount);
                        break;
                    case QuestRewardKind.Experience:
                        partyState.ActivePlayer?.AddExperience(reward.Amount);
                        break;
                    default:
                        break;
                }
            }
        }

        private void AdvanceObjectives(QuestObjectiveKind kind, string targetId, int amount)
        {
            bool updated = false;
            foreach (QuestState quest in quests.Values)
            {
                if (quest.TryAdvanceObjective(kind, targetId, amount))
                {
                    updated = true;
                    if (quest.Status == QuestStatus.Active
                        && quest.AreObjectivesComplete()
                        && !readyForTurnIn.Contains(quest.Definition.Id))
                    {
                        readyForTurnIn.Add(quest.Definition.Id);
                        Events.RaiseGameMessage($"Quest ready to complete: {quest.Definition.Name}");
                    }
                }
            }

            foreach (AchievementState achievement in achievements.Values)
            {
                if (achievement.TryAdvanceObjective(kind, targetId, amount))
                {
                    updated = true;
                    if (achievement.IsUnlocked)
                    {
                        Events.RaiseGameMessage($"Achievement unlocked: {achievement.Definition.Name}");
                    }
                }
            }

            if (updated)
            {
                QuestLogChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
