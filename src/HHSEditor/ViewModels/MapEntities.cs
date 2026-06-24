using CommunityToolkit.Mvvm.ComponentModel;

namespace HHSEditor.ViewModels;

/// <summary>
/// 地图实体类型
/// </summary>
public enum MapEntityType
{
    Enemy,
    Npc,
    Player,
    Item,
    Trigger
}

/// <summary>
/// 地图实体基类
/// </summary>
public abstract partial class MapEntity : ObservableObject
{
    [ObservableProperty]
    private int _x;

    [ObservableProperty]
    private int _y;

    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _name = "";

    public abstract MapEntityType EntityType { get; }

    public abstract char DisplayChar { get; }

    public abstract string DisplayName { get; }
}

/// <summary>
/// 敌人实体
/// </summary>
public partial class EnemyEntity : MapEntity
{
    [ObservableProperty]
    private string _enemyTypeId = "";

    [ObservableProperty]
    private string _patrolRoute = "";

    [ObservableProperty]
    private bool _isStatic;

    public override MapEntityType EntityType => MapEntityType.Enemy;
    public override char DisplayChar => 'E';
    public override string DisplayName => $"Enemy: {Name}";
}

/// <summary>
/// NPC 实体
/// </summary>
public partial class NpcEntity : MapEntity
{
    [ObservableProperty]
    private string _dialogueId = "";

    [ObservableProperty]
    private string _faction = "";

    [ObservableProperty]
    private string _schedule = "";

    public override MapEntityType EntityType => MapEntityType.Npc;
    public override char DisplayChar => 'C';
    public override string DisplayName => $"NPC: {Name}";
}

/// <summary>
/// 玩家起点
/// </summary>
public partial class PlayerSpawnEntity : MapEntity
{
    [ObservableProperty]
    private int _playerIndex;

    public override MapEntityType EntityType => MapEntityType.Player;
    public override char DisplayChar => 'P';
    public override string DisplayName => $"Player {PlayerIndex}";
}

/// <summary>
/// 物品实体
/// </summary>
public partial class ItemEntity : MapEntity
{
    [ObservableProperty]
    private string _itemId = "";

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private bool _isHidden;

    public override MapEntityType EntityType => MapEntityType.Item;
    public override char DisplayChar => 'I';
    public override string DisplayName => $"Item: {Name}";
}

/// <summary>
/// 触发事件实体
/// </summary>
public partial class TriggerEntity : MapEntity
{
    [ObservableProperty]
    private string _triggerType = "OnEnter";

    [ObservableProperty]
    private string _action = "";

    [ObservableProperty]
    private string _actionParameters = "";

    [ObservableProperty]
    private bool _isOneTime = true;

    [ObservableProperty]
    private string _condition = "";

    public override MapEntityType EntityType => MapEntityType.Trigger;
    public override char DisplayChar => 'T';
    public override string DisplayName => $"Trigger: {Name}";
}

/// <summary>
/// 触发事件类型
/// </summary>
public static class TriggerTypes
{
    public static readonly string[] All =
    [
        "OnEnter",      // 玩家进入格子时触发
        "OnExit",       // 玩家离开格子时触发
        "OnInteract",   // 玩家交互时触发
        "OnTurnEnd",    // 回合结束时触发
        "OnEnemyDeath", // 敌人死亡时触发
        "OnQuestStart", // 任务开始时触发
        "OnQuestEnd",   // 任务结束时触发
    ];

    public static readonly string[] Actions =
    [
        "SpawnEnemy",       // 生成敌人
        "SpawnItem",        // 生成物品
        "ShowDialogue",     // 显示对话
        "StartQuest",       // 开始任务
        "CompleteQuest",    // 完成任务
        "ModifyReputation", // 修改声望
        "Teleport",         // 传送
        "Damage",           // 造成伤害
        "Heal",             // 治疗
        "ShowMessage",      // 显示消息
        "PlaySound",        // 播放音效
        "ChangeMap",        // 切换地图
    ];
}
