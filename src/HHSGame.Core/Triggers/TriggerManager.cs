using HHSGame.Core.Scripting;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Triggers
{
    /// <summary>
    /// 触发器管理器
    /// </summary>
    public sealed class TriggerManager
    {
        private readonly ILogger<TriggerManager> logger;
        private readonly ScriptIntegration? scriptIntegration;
        private readonly Dictionary<string, MapTrigger> triggers = new();
        private readonly List<MapTrigger> activeTriggers = [];

        // 事件
        public event EventHandler<TriggerEventArgs>? TriggerActivated;
        public event EventHandler<string>? ShowMessage;
        public event EventHandler<(string questId, bool start)>? QuestEvent;

        public TriggerManager(ILogger<TriggerManager> logger, ScriptIntegration? scriptIntegration = null)
        {
            this.logger = logger;
            this.scriptIntegration = scriptIntegration;
        }

        // 加载触发器定义
        public void LoadTriggers(IEnumerable<MapTrigger> triggerDefinitions)
        {
            triggers.Clear();
            activeTriggers.Clear();

            foreach (var trigger in triggerDefinitions)
            {
                triggers[trigger.Id] = trigger;
                if (trigger.IsEnabled)
                {
                    activeTriggers.Add(trigger);
                }
            }

            logger.LogInformation("Loaded {Count} triggers", triggers.Count);
        }

        // 获取所有触发器
        public IReadOnlyList<MapTrigger> GetAllTriggers()
        {
            return triggers.Values.ToList();
        }

        // 获取指定位置的触发器
        public IEnumerable<MapTrigger> GetTriggersAt(int x, int y)
        {
            return activeTriggers.Where(t => t.IsEnabled && t.IsPlayerInRange(x, y));
        }

        // 处理玩家移动
        public void OnPlayerMoved(int fromX, int fromY, int toX, int toY, int currentTurn)
        {
            // 检查离开区域的触发器
            foreach (var trigger in activeTriggers.Where(t => t.Type == TriggerType.OnExit))
            {
                if (trigger.IsPlayerInRange(fromX, fromY) && !trigger.IsPlayerInRange(toX, toY))
                {
                    ExecuteTrigger(trigger, currentTurn);
                }
            }

            // 检查进入区域的触发器
            foreach (var trigger in activeTriggers.Where(t => t.Type == TriggerType.OnEnter))
            {
                if (!trigger.IsPlayerInRange(fromX, fromY) && trigger.IsPlayerInRange(toX, toY))
                {
                    ExecuteTrigger(trigger, currentTurn);
                }
            }
        }

        // 处理回合结束
        public void OnTurnEnd(int currentTurn)
        {
            foreach (var trigger in activeTriggers.Where(t => t.Type == TriggerType.OnTurnEnd))
            {
                if (!trigger.IsOnCooldown(currentTurn))
                {
                    ExecuteTrigger(trigger, currentTurn);
                }
            }
        }

        // 处理敌人死亡
        public void OnEnemyKilled(string enemyId, int x, int y, int currentTurn)
        {
            foreach (var trigger in activeTriggers.Where(t => t.Type == TriggerType.OnEnemyKilled))
            {
                if (trigger.IsPlayerInRange(x, y) || trigger.ActionParameter == enemyId)
                {
                    ExecuteTrigger(trigger, currentTurn);
                }
            }
        }

        // 处理任务事件
        public void OnQuestEvent(string questId, bool isStart, int currentTurn)
        {
            var triggerType = isStart ? TriggerType.OnQuestStart : TriggerType.OnQuestComplete;
            foreach (var trigger in activeTriggers.Where(t => t.Type == triggerType))
            {
                if (trigger.ActionParameter == questId)
                {
                    ExecuteTrigger(trigger, currentTurn);
                }
            }
        }

        // 处理交互
        public void OnInteract(int x, int y, int currentTurn)
        {
            foreach (var trigger in activeTriggers.Where(t => t.Type == TriggerType.OnInteract))
            {
                if (trigger.IsPlayerInRange(x, y))
                {
                    ExecuteTrigger(trigger, currentTurn);
                }
            }
        }

        // 执行触发器
        private void ExecuteTrigger(MapTrigger trigger, int currentTurn)
        {
            if (!trigger.IsEnabled || trigger.IsOnCooldown(currentTurn))
            {
                return;
            }

            // 检查 Lua 条件
            if (scriptIntegration != null && !scriptIntegration.CheckTriggerCondition(trigger))
            {
                logger.LogDebug("Trigger condition not met: {Id}", trigger.Id);
                return;
            }

            logger.LogDebug("Executing trigger: {Id} ({Name})", trigger.Id, trigger.Name);

            // 使用 ScriptIntegration 执行动作（内置 + Lua）
            if (scriptIntegration != null)
            {
                scriptIntegration.ExecuteTriggerAction(trigger);
            }
            else if (trigger.BuiltInAction != BuiltInAction.None)
            {
                ExecuteBuiltInAction(trigger);
            }

            // 标记触发
            trigger.MarkTriggered(currentTurn);

            // 触发事件
            TriggerActivated?.Invoke(this, new TriggerEventArgs(trigger));
        }

        // 执行内置动作
        private void ExecuteBuiltInAction(MapTrigger trigger)
        {
            switch (trigger.BuiltInAction)
            {
                case BuiltInAction.ShowMessage:
                    if (!string.IsNullOrEmpty(trigger.ActionParameter))
                    {
                        ShowMessage?.Invoke(this, trigger.ActionParameter);
                    }
                    break;

                case BuiltInAction.StartQuest:
                    if (!string.IsNullOrEmpty(trigger.ActionParameter))
                    {
                        QuestEvent?.Invoke(this, (trigger.ActionParameter, true));
                    }
                    break;

                case BuiltInAction.CompleteQuest:
                    if (!string.IsNullOrEmpty(trigger.ActionParameter))
                    {
                        QuestEvent?.Invoke(this, (trigger.ActionParameter, false));
                    }
                    break;

                case BuiltInAction.SpawnEnemy:
                case BuiltInAction.SpawnItem:
                case BuiltInAction.ModifyReputation:
                case BuiltInAction.Teleport:
                case BuiltInAction.ChangeMap:
                case BuiltInAction.PlaySound:
                    // 这些需要通过 Lua 脚本或更复杂的参数处理
                    logger.LogWarning("Built-in action {Action} not yet implemented", trigger.BuiltInAction);
                    break;
            }
        }

        // 启用/禁用触发器
        public void SetTriggerEnabled(string triggerId, bool enabled)
        {
            if (triggers.TryGetValue(triggerId, out var trigger))
            {
                trigger.IsEnabled = enabled;
                if (enabled && !activeTriggers.Contains(trigger))
                {
                    activeTriggers.Add(trigger);
                }
                else if (!enabled)
                {
                    activeTriggers.Remove(trigger);
                }
            }
        }

        // 注册新触发器
        public void RegisterTrigger(MapTrigger trigger)
        {
            triggers[trigger.Id] = trigger;
            if (trigger.IsEnabled)
            {
                activeTriggers.Add(trigger);
            }
        }

        // 移除触发器
        public void UnregisterTrigger(string triggerId)
        {
            if (triggers.TryGetValue(triggerId, out var trigger))
            {
                triggers.Remove(triggerId);
                activeTriggers.Remove(trigger);
            }
        }
    }

    /// <summary>
    /// 触发器事件参数
    /// </summary>
    public sealed class TriggerEventArgs : EventArgs
    {
        public MapTrigger Trigger { get; }

        public TriggerEventArgs(MapTrigger trigger)
        {
            Trigger = trigger;
        }
    }
}
