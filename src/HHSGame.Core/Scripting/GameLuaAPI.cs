using HHSGame.Core.Items;
using HHSGame.Core.Quests;
using HHSGame.Core.Triggers;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Scripting
{
    /// <summary>
    /// 暴露给 Lua 的游戏 API 实现
    /// </summary>
    public sealed class GameLuaAPI : IGameLuaAPI
    {
        private readonly GameContext context;
        private readonly TriggerManager triggerManager;
        private readonly ILogger<GameLuaAPI> logger;

        public GameLuaAPI(GameContext context, TriggerManager triggerManager, ILogger<GameLuaAPI> logger)
        {
            this.context = context;
            this.triggerManager = triggerManager;
            this.logger = logger;
        }

        // 玩家
        public int GetPlayerX() => context.Player?.X ?? 0;
        public int GetPlayerY() => context.Player?.Y ?? 0;
        public int GetPlayerHP() => context.Player?.Stats.CurrentHp ?? 0;
        public int GetPlayerMaxHP() => context.Player?.Stats.MaxHp ?? 0;
        public int GetPlayerAP() => context.Player?.Stats.CurrentAp ?? 0;

        public void HealPlayer(int amount)
        {
            context.Player?.Stats.Heal(amount);
            Events.RaiseGameMessage($"Player healed for {amount} HP");
        }

        public void DamagePlayer(int amount)
        {
            context.Player?.TakeDamage(amount);
            Events.RaiseGameMessage($"Player took {amount} damage");
        }

        public void GiveItem(string itemId, int quantity)
        {
            // TODO: Implement when ItemCatalog has proper factory method
            logger.LogWarning("GiveItem not yet fully implemented");
        }

        public void RemoveItem(string itemId, int quantity)
        {
            // TODO: Implement
            logger.LogWarning("RemoveItem not yet fully implemented");
        }

        public bool HasItem(string itemId)
        {
            // TODO: Implement
            return false;
        }

        // 地图
        public void SetTile(int x, int y, char tile)
        {
            context.MapState.SetCell(x, y, new Rendering.Cell { Character = tile });
        }

        public char GetTile(int x, int y)
        {
            return context.MapState.GetCell(x, y).Character;
        }

        public void RevealArea(int x, int y, int radius)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int tx = x + dx;
                    int ty = y + dy;
                    if (context.MapState.IsInBounds(tx, ty))
                    {
                        context.MapState.MarkExplored(tx, ty);
                    }
                }
            }
        }

        public bool IsWalkable(int x, int y)
        {
            return context.MapState.IsWalkable(x, y);
        }

        public int GetMapWidth() => context.MapState.Width;
        public int GetMapHeight() => context.MapState.Height;

        // 敌人
        public void SpawnEnemy(string enemyId, int x, int y)
        {
            try
            {
                var enemy = context.EnemyFactory.CreateEnemy(enemyId, x, y);
                // TODO: Add enemy to EnemyManager
                Events.RaiseGameMessage($"Enemy spawned at ({x},{y})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to spawn enemy {EnemyId}", enemyId);
            }
        }

        public void RemoveEnemy(int x, int y)
        {
            // TODO: Implement
            logger.LogWarning("RemoveEnemy not yet fully implemented");
        }

        public int GetEnemyCount()
        {
            // TODO: Implement
            return 0;
        }

        public bool IsEnemyAt(int x, int y)
        {
            // TODO: Implement
            return false;
        }

        // NPC
        public void SpawnNpc(string npcId, int x, int y)
        {
            // TODO: Implement
            logger.LogWarning("SpawnNpc not yet fully implemented");
        }

        public void StartDialogue(string dialogueId)
        {
            Events.RaiseGameMessage($"Starting dialogue: {dialogueId}");
        }

        public bool IsNpcAt(int x, int y)
        {
            return context.NpcManager.GetNpcAt(x, y) != null;
        }

        // 任务
        public void StartQuest(string questId)
        {
            context.QuestManager.StartQuest(questId);
            triggerManager.OnQuestEvent(questId, true, context.TurnManager.GetTurnNumber());
        }

        public void CompleteQuest(string questId)
        {
            context.QuestManager.CompleteQuest(questId);
            triggerManager.OnQuestEvent(questId, false, context.TurnManager.GetTurnNumber());
        }

        public bool IsQuestActive(string questId)
        {
            return context.QuestManager.IsQuestInStatus(questId, QuestStatus.Active);
        }

        public bool IsQuestComplete(string questId)
        {
            return context.QuestManager.IsQuestInStatus(questId, QuestStatus.Completed);
        }

        public void SetQuestObjectiveProgress(string questId, int objectiveIndex, int value)
        {
            // TODO: Implement
            logger.LogWarning("SetQuestObjectiveProgress not yet implemented");
        }

        // 声望
        public int GetReputation(string faction)
        {
            var f = Factions.FactionManager.ParseFaction(faction);
            return context.FactionManager.GetReputation(f);
        }

        public void ModifyReputation(string faction, int amount)
        {
            var f = Factions.FactionManager.ParseFaction(faction);
            context.FactionManager.ModifyReputation(f, amount);
            Events.RaiseGameMessage($"Reputation with {faction} changed by {amount}");
        }

        // 消息
        public void ShowMessage(string message)
        {
            Events.RaiseGameMessage(message);
        }

        public void ShowNotification(string message)
        {
            Events.RaiseGameMessage($"[!] {message}");
        }

        // 地图切换
        public void ChangeMap(string mapId, int x, int y)
        {
            Events.RaiseGameMessage($"Changing map to {mapId} at ({x},{y})");
            // TODO: Implement actual map loading
        }

        // 触发器
        public void EnableTrigger(string triggerId)
        {
            triggerManager.SetTriggerEnabled(triggerId, true);
        }

        public void DisableTrigger(string triggerId)
        {
            triggerManager.SetTriggerEnabled(triggerId, false);
        }

        public void RegisterTrigger(string triggerId, string triggerType, string script)
        {
            var type = Enum.TryParse<TriggerType>(triggerType, true, out var t) ? t : TriggerType.OnEnter;
            var trigger = new MapTrigger
            {
                Id = triggerId,
                Name = triggerId,
                Type = type,
                ActionScript = script,
                IsEnabled = true
            };
            triggerManager.RegisterTrigger(trigger);
        }

        // 工具
        public int Random(int min, int max)
        {
            return context.Random.Next(min, max + 1);
        }

        public bool RollCheck(int difficulty)
        {
            int roll = context.Random.Next(1, 21);
            return roll <= difficulty;
        }

        public int GetTurnCount()
        {
            return context.TurnManager.GetTurnNumber();
        }

        public string GetLocalizedString(string key)
        {
            return Utils.I18n.T(key);
        }
    }
}
