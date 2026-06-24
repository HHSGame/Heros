namespace HHSGame.Core.Triggers
{
    /// <summary>
    /// 触发器类型
    /// </summary>
    public enum TriggerType
    {
        OnEnter,        // 玩家进入区域
        OnExit,         // 玩家离开区域
        OnInteract,     // 玩家交互
        OnTurnEnd,      // 回合结束
        OnEnemyKilled,  // 敌人死亡
        OnItemPickup,   // 拾取物品
        OnQuestStart,   // 任务开始
        OnQuestComplete,// 任务完成
        OnDialogueEnd,  // 对话结束
        OnTimer         // 定时触发
    }

    /// <summary>
    /// 内置触发动作
    /// </summary>
    public enum BuiltInAction
    {
        None,
        SpawnEnemy,
        SpawnItem,
        ShowMessage,
        StartQuest,
        CompleteQuest,
        ModifyReputation,
        Teleport,
        ChangeMap,
        PlaySound
    }

    /// <summary>
    /// 地图触发器定义
    /// </summary>
    public sealed class MapTrigger
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; } = 1;
        public int Height { get; init; } = 1;

        // 触发条件
        public TriggerType Type { get; init; }
        public string? ConditionScript { get; init; }  // Lua 条件脚本

        // 触发动作
        public string? ActionScript { get; init; }     // Lua 动作脚本
        public BuiltInAction BuiltInAction { get; init; }
        public string? ActionParameter { get; init; }  // 动作参数

        // 属性
        public bool IsOneTime { get; init; }
        public bool IsEnabled { get; set; } = true;
        public int CooldownTurns { get; init; }
        public int LastTriggeredTurn { get; set; } = -1;

        // 检查玩家是否在触发区域内
        public bool IsPlayerInRange(int playerX, int playerY)
        {
            return playerX >= X && playerX < X + Width
                && playerY >= Y && playerY < Y + Height;
        }

        // 检查冷却
        public bool IsOnCooldown(int currentTurn)
        {
            if (CooldownTurns <= 0) return false;
            return currentTurn - LastTriggeredTurn < CooldownTurns;
        }

        // 标记触发
        public void MarkTriggered(int currentTurn)
        {
            LastTriggeredTurn = currentTurn;
            if (IsOneTime)
            {
                IsEnabled = false;
            }
        }
    }

    /// <summary>
    /// 触发器序列化模型
    /// </summary>
    public sealed class MapTriggerData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public string Type { get; set; } = "OnEnter";
        public string? ConditionScript { get; set; }
        public string? ActionScript { get; set; }
        public string? BuiltInAction { get; set; }
        public string? ActionParameter { get; set; }
        public bool IsOneTime { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int CooldownTurns { get; set; }

        public MapTrigger ToTrigger()
        {
            return new MapTrigger
            {
                Id = Id,
                Name = Name,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                Type = Enum.TryParse<TriggerType>(Type, true, out var t) ? t : TriggerType.OnEnter,
                ConditionScript = ConditionScript,
                ActionScript = ActionScript,
                BuiltInAction = Enum.TryParse<Triggers.BuiltInAction>(BuiltInAction, true, out var a) ? a : Triggers.BuiltInAction.None,
                ActionParameter = ActionParameter,
                IsOneTime = IsOneTime,
                IsEnabled = IsEnabled,
                CooldownTurns = CooldownTurns
            };
        }

        public static MapTriggerData FromTrigger(MapTrigger trigger)
        {
            return new MapTriggerData
            {
                Id = trigger.Id,
                Name = trigger.Name,
                X = trigger.X,
                Y = trigger.Y,
                Width = trigger.Width,
                Height = trigger.Height,
                Type = trigger.Type.ToString(),
                ConditionScript = trigger.ConditionScript,
                ActionScript = trigger.ActionScript,
                BuiltInAction = trigger.BuiltInAction.ToString(),
                ActionParameter = trigger.ActionParameter,
                IsOneTime = trigger.IsOneTime,
                IsEnabled = trigger.IsEnabled,
                CooldownTurns = trigger.CooldownTurns
            };
        }
    }
}
