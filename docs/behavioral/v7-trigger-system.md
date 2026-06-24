# 触发事件系统与 Lua 脚本支持 (v7)

> 创建日期：2026-06-08

## 一、Vision / 背景动机

当前游戏缺乏通用的触发事件系统，无法实现：
- 玩家进入特定区域触发事件
- 敌人死亡后触发剧情
- 任务完成触发新内容
- 条件满足时解锁区域

需要引入 Lua 脚本引擎，让触发器可以执行复杂逻辑。

## 二、Scope / Non-Goals

### Scope
- 地图触发事件系统
- Lua 脚本引擎集成
- 触发器编辑器
- 脚本调试支持

### Non-Goals
- 不替换现有 JSON 脚本（保留用于测试）
- 不实现网络脚本执行
- 不实现可视化脚本编辑器（节点图）

## 三、数据模型

### 3.1 触发器定义

```csharp
// HHSGame.Core/Triggers/MapTrigger.cs
public sealed class MapTrigger
{
    public string Id { get; init; }
    public string Name { get; init; }
    public Coordinate Position { get; init; }
    public int Width { get; init; } = 1;
    public int Height { get; init; } = 1;

    // 触发条件
    public TriggerType Type { get; init; }
    public string? ConditionScript { get; init; }  // Lua 条件脚本

    // 触发动作
    public string? ActionScript { get; init; }     // Lua 动作脚本
    public TriggerAction? BuiltInAction { get; init; } // 内置动作

    // 属性
    public bool IsOneTime { get; init; }
    public bool IsEnabled { get; set; } = true;
    public int CooldownTurns { get; init; }
    public int LastTriggeredTurn { get; set; } = -1;
}

public enum TriggerType
{
    OnEnter,        // 玩家进入区域
    OnExit,         // 玩家离开区域
    OnInteract,     // 玩家交互（按 Enter）
    OnTurnEnd,      // 回合结束
    OnEnemyKilled,  // 敌人死亡
    OnItemPickup,   // 拾取物品
    OnQuestStart,   // 任务开始
    OnQuestComplete,// 任务完成
    OnDialogueEnd,  // 对话结束
    OnTimer         // 定时触发
}

public enum TriggerAction
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
```

### 3.2 脚本 API

```csharp
// HHSGame.Core/Scripting/ILuaScriptEngine.cs
public interface ILuaScriptEngine
{
    void LoadScript(string name, string code);
    bool Evaluate(string script, object? context = null);
    void Execute(string script, object? context = null);
    T? GetGlobal<T>(string name);
    void SetGlobal(string name, object value);
}

// 暴露给 Lua 的游戏 API
public interface IGameLuaAPI
{
    // 玩家
    PlayerData GetPlayer();
    void HealPlayer(int amount);
    void DamagePlayer(int amount);
    void GiveItem(string itemId, int quantity);

    // 地图
    void SetTile(int x, int y, char tile);
    char GetTile(int x, int y);
    void RevealArea(int x, int y, int radius);

    // 敌人
    void SpawnEnemy(string enemyId, int x, int y);
    void RemoveEnemy(int x, int y);
    int GetEnemyCount();

    // NPC
    void SpawnNpc(string npcId, int x, int y);
    void StartDialogue(string dialogueId);

    // 任务
    void StartQuest(string questId);
    void CompleteQuest(string questId);
    bool IsQuestActive(string questId);
    bool IsQuestComplete(string questId);

    // 声望
    int GetReputation(string faction);
    void ModifyReputation(string faction, int amount);

    // 消息
    void ShowMessage(string message);
    void ShowNotification(string message);

    // 地图
    void ChangeMap(string mapId, int x, int y);

    // 事件
    void RegisterTrigger(string triggerId);
    void UnregisterTrigger(string triggerId);

    // 工具
    int Random(int min, int max);
    bool RollCheck(int difficulty);
    string GetLocalizedString(string key);
}
```

## 四、实现计划

### Phase 1: 触发事件系统（1周）

1. 创建 `HHSGame.Core/Triggers/` 目录
2. 实现 `MapTrigger` 模型
3. 实现 `TriggerManager` 管理触发器
4. 集成到 `GameContext`
5. 在游戏循环中检查触发器

### Phase 2: Lua 引擎集成（1-2周）

1. 添加 NuGet 包：`NLua` 或 `MoonSharp`
2. 实现 `LuaScriptEngine`
3. 实现 `GameLuaAPI`
4. 注册到 DI 容器
5. 安全沙箱配置

### Phase 3: 编辑器集成（1周）

1. 触发器编辑器 UI
2. Lua 脚本编辑器（语法高亮）
3. 脚本测试面板
4. 保存/加载触发器数据

### Phase 4: 高级功能（1周）

1. 条件脚本支持
2. 脚本调试器
3. 脚本热重载
4. 性能优化（脚本缓存）

## 五、依赖

- NLua 或 MoonSharp（Lua 引擎）
- 现有游戏引擎的事件系统扩展
- 编辑器的脚本编辑器组件

## 六、验收标准

1. 触发器可以在地图上放置和配置
2. 玩家进入区域时触发器正确执行
3. Lua 脚本可以调用游戏 API
4. 条件脚本正确评估
5. 触发器可以保存/加载
6. 脚本错误不影响游戏稳定性

## 七、风险与回退

| 风险 | 影响 | 缓解 |
|------|------|------|
| Lua 引擎性能问题 | 中 | 限制脚本执行时间 |
| 脚本安全漏洞 | 高 | 沙箱限制，禁止文件/网络访问 |
| 脚本调试困难 | 中 | 提供日志和错误报告 |
| 复杂脚本难以维护 | 中 | 提供脚本模板和文档 |
