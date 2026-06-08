# HHSGame 游戏编辑器规划 (v6)

> 创建日期：2026-06-05

## 一、Vision / 背景动机

当前游戏存在以下问题：
1. 剧情内容自动生成质量不稳定，需要人工编辑和审核工具
2. 游戏数据（武器、敌人、对话等）散布在多个 JSON 文件中，缺乏统一管理
3. 地图编辑依赖外部 HTML 工具，与游戏引擎脱节
4. 缺乏 Mod 支持，无法扩展游戏内容
5. 程序化生成规则硬编码，无法配置

**目标**：创建一个类似魔兽争霸地图编辑器的游戏编辑器，支持可视化编辑所有游戏数据、接入 LLM 自动生成内容、打包为可分发的数据包。

## 二、Scope / Non-Goals

### Scope
- 游戏编辑器（Avalonia UI）
- 共享 Core 逻辑
- 数据打包格式（.hhsbundle）
- LLM 集成
- 脚本系统（QuickJS/Lua）
- 引擎扩展（故事系统、Mod 系统、程序化生成规则）

### Non-Goals
- 不涉及游戏 UI 框架替换（保留 Terminal.Gui）
- 不涉及网络多人游戏
- 不涉及移动端部署

## 三、解决方案结构

```
Heros.sln
  src/
    HHSGame.Core/           -- 共享类库：所有 Core/* + Engine/* 代码
    HHSGame/                -- 游戏主程序：引用 HHSGame.Core，保留 UI/*
    HHSGameTest/            -- 测试项目：引用 HHSGame.Core
    HHSEditor/              -- Avalonia 桌面应用
    HHSEditor.Core/         -- 编辑器服务层：LLM、打包、项目管理
    HHSEditor.Controls/     -- Avalonia 自定义控件库
    HHSEditor.Plugins/      -- 插件接口定义
  data/                     -- 游戏数据
  editor-data/              -- 编辑器数据（Prompt 模板等）
```

**依赖关系**：
```
HHSGame.Core        (无 UI 依赖，net9.0 类库)
  ^           ^
  |           |
HHSGame      HHSEditor.Core  (编辑器服务，LLM，打包)
(Terminal.Gui)    ^
                  |
             HHSEditor  (Avalonia 桌面应用)
                  |
             HHSEditor.Controls  (Avalonia 自定义控件)
```

## 四、核心架构

### 4.1 Terminal.Gui 依赖解耦

当前 `Cell` 和 `ColorPresets` 依赖 `Terminal.Gui.Drawing.Attribute`。需要抽象为平台无关的类型：

```csharp
// HHSGame.Core.Rendering
public enum GameColor { Black, Red, Green, Yellow, Blue, Magenta, Cyan, White, ... }
public struct GameAttribute(GameColor Foreground, GameColor Background);
public struct Cell(char Character, GameAttribute Attribute);
```

各 UI 平台提供映射扩展：
- `HHSGame`: `GameAttribute.ToTerminalAttribute()`
- `HHSEditor`: `GameAttribute.ToAvaloniaColor()`

### 4.2 MVVM 模式

编辑器使用标准 Avalonia MVVM + CommunityToolkit.Mvvm：

```
Models:           复用 HHSGame.Core 类型
ViewModels:       EditorViewModelBase (ObservableObject)
Views:            Avalonia UserControls
Services:         IProjectService, IDataService, ILLMService, IBundlerService
```

### 4.3 数据流

```
[磁盘数据文件] → [IDataService.Load<T>] → [ViewModel ObservableCollection] → [Avalonia UI]
                                                                              ↓
[磁盘/Bundle] ← [IDataService.Save<T>] ← [ViewModel Commands] ← [用户编辑]
```

## 五、编辑器模块设计

每个编辑器采用三栏布局：列表（左）、编辑（中）、属性/预览（右）。

### 5.1 地图编辑器
- Canvas 网格渲染器（Avalonia DrawingContext / Skia）
- 图块调色板（字符 + 颜色）
- 工具：画笔/矩形/填充/门/楼梯/移动
- 程序化生成：城镇/洞穴/森林风格
- 实体叠加层：显示放置的敌人、NPC、物品
- 导入/导出 .txt 地图文件

### 5.2 物品/武器/盔甲目录编辑器
- DataGrid 内联编辑
- 搜索/过滤（按 id、name、rarity）
- 交叉引用验证（敌人引用的 weaponId 必须存在）
- 批量操作（导入 JSON、导出、复制）

### 5.3 敌人编辑器
- 属性滑块、技能等级
- 武器/盔甲引用选择器
- 能力编辑器（kind/chance/duration/amount）
- 掉落表编辑器
- 阵营选择器
- 字符和颜色预览

### 5.4 职业编辑器
- 属性点分配
- 技能分布编辑
- 初始装备选择器
- 派生属性预览（MaxHP、MaxSP、BaseAP、负重）

### 5.5 NPC/对话编辑器（节点图）
- 可视化节点图：节点 = 对话节点，边 = 选项连接
- 每个节点：id、文本、选项列表
- 每个选项：文本、nextNodeId、需求（技能/属性/任务）、效果（开始任务、给予物品、修改声望）
- 孤立节点检测、缺失引用检查
- LLM 集成："生成对话"按钮

### 5.6 任务/故事编辑器
- 任务定义：id、名称、描述、目标、奖励
- 目标编辑器（KillEnemy、CollectItem、ReachMarker、TalkToNpc）
- 奖励编辑器（物品、货币、经验）
- 成就编辑器
- 故事弧线视图：显示任务链和前置条件

### 5.7 阵营/故事弧线编辑器
- 幕/章节树结构
- 故事节拍关联任务、对话、事件
- 条件编辑器（任务状态、阵营声望、持有物品）
- 叙事文本编辑器 + LLM 集成
- 时间线视图

### 5.8 种族编辑器
- 属性修正滑块
- 能力选择器
- 描述/传说文本编辑器 + LLM 集成

### 5.9 脚本编辑器
- JSON 脚本编辑器（语法高亮）
- QuickJS/Lua 编辑器（AvaloniaEdit）
- 脚本运行/测试面板
- JSON 脚本单步调试器

## 六、数据格式与打包方案

### 6.1 新增数据文件

```
data/
  races.json             -- 种族定义
  story-arcs.json        -- 故事弧线定义
  procgen-rules.json     -- 程序化生成规则
  scripts/
    quickjs/             -- QuickJS 脚本
    lua/                 -- Lua 脚本
```

### 6.2 `.hhsbundle` 格式（ZIP 归档）

```
mymod.hhsbundle (ZIP)
  manifest.json          -- 元数据（id、name、version、author、dependencies）
  game.json              -- 覆盖游戏配置
  catalogs/              -- 目录数据
  maps/                  -- 地图文件
  scripts/               -- 脚本文件
  story/                 -- 故事数据
  resources/             -- 资源文件
```

### 6.3 引擎数据加载抽象

```csharp
public interface IDataSource
{
    string? ReadText(string path);
    byte[]? ReadBytes(string path);
    bool Exists(string path);
    IEnumerable<string> ListFiles(string directory, string pattern);
}

public sealed class FileSystemDataSource(string rootPath) : IDataSource { }
public sealed class BundleDataSource(Stream bundleStream) : IDataSource { }
public sealed class CompositeDataSource : IDataSource { }  // 合并多个数据源
```

## 七、LLM 集成架构

### 7.1 Provider 抽象

```csharp
public interface ILLMProvider
{
    string Name { get; }
    Task<LLMResponse> GenerateAsync(string prompt, LLMOptions options, CancellationToken ct = default);
    Task<LLMResponse> GenerateWithSystemAsync(string systemPrompt, string userPrompt, LLMOptions options, CancellationToken ct = default);
}
```

实现：OpenAIProvider（兼容本地 LLM）、OllamaProvider、AnthropicProvider

### 7.2 Prompt 模板系统

```
editor-data/prompts/
  dialogue-generation.json
  quest-description.json
  npc-backstory.json
  story-beat.json
  item-description.json
  enemy-lore.json
```

模板格式：
```json
{
  "id": "dialogue-generation",
  "systemPrompt": "You are a creative writer for a WWII-era roguelike game...",
  "userPromptTemplate": "Generate dialogue for NPC: {{npcName}}...",
  "variables": ["npcName", "faction", "role"],
  "outputFormat": "json"
}
```

### 7.3 集成点

- 对话编辑器：生成对话树
- 任务编辑器：生成任务描述
- 故事编辑器：生成故事节拍
- 物品/敌人编辑器：生成背景描述
- 地图编辑器：生成区域描述

## 八、脚本系统设计

### 8.1 技术选型：Jint（推荐）

- 纯 managed C# 实现，无原生依赖
- 完整 ES2023 支持
- 沙箱执行（默认无文件系统/网络访问）
- JSON 原生支持

### 8.2 游戏 API 暴露

```csharp
public interface IGameScriptAPI
{
    // 地图
    CellData GetCell(int x, int y);
    void SetCell(int x, int y, CellData cell);
    int MapWidth { get; }
    int MapHeight { get; }

    // 玩家
    PlayerData GetPlayer();
    void DamagePlayer(int amount);
    void HealPlayer(int amount);
    void GivePlayerItem(string itemId);

    // 敌人
    EnemyData[] GetEnemies();
    void SpawnEnemy(string enemyId, int x, int y);

    // 任务
    void StartQuest(string questId);
    void CompleteQuest(string questId);

    // 阵营
    int GetReputation(string faction);
    void ModifyReputation(string faction, int amount);

    // 事件
    void On(string eventName, Action<object> callback);
    void Emit(string eventName, object data);

    // 工具
    void ShowMessage(string text);
    int Random(int min, int max);
}
```

### 8.3 脚本示例

```javascript
export default {
  name: "custom-quest",
  hooks: {
    onEnemyKilled(enemyId, x, y) {
      if (enemyId === "BanditBoss") {
        api.StartQuest("bandit-revenge");
      }
    },
    onDialogueOption(dialogueId, nodeId, optionIndex) {
      if (dialogueId === "viktor-dialogue" && nodeId === "fight") {
        api.ModifyReputation("Bandits", -20);
      }
    }
  }
};
```

## 九、引擎扩展

### 9.1 故事系统

```csharp
public sealed record StoryArcDefinition(string Id, string Name, string Description, IReadOnlyList<StoryActDefinition> Acts);
public sealed record StoryActDefinition(string Id, string Name, string Description, IReadOnlyList<StoryBeatDefinition> Beats);
public sealed record StoryBeatDefinition(string Id, string Name, string NarrativeText, StoryTrigger Trigger, IReadOnlyList<StoryEffect> Effects);
public sealed record StoryTrigger(string Type, string Target, int Value);
public sealed record StoryEffect(string Type, string Target, Dictionary<string, string> Parameters);

public sealed class StoryManager
{
    public void LoadDefinitions(IReadOnlyList<StoryArcDefinition> arcs);
    public void CheckTriggers(GameContext context);
    public IReadOnlyList<StoryBeatDefinition> GetActiveBeats();
}
```

### 9.2 程序化生成规则

```csharp
public sealed class ProcGenRuleSet
{
    public string Style { get; init; }
    public int MinRooms { get; init; }
    public int MaxRooms { get; init; }
    public int MinRoomSize { get; init; }
    public int MaxRoomSize { get; init; }
    public char WallChar { get; init; }
    public char FloorChar { get; init; }
    public char DoorChar { get; init; }
    public List<TerrainPatchRule> TerrainPatches { get; init; }
    public List<SpawnRule> EnemySpawnRules { get; init; }
    public List<SpawnRule> ItemSpawnRules { get; init; }
    public List<FeatureRule> Features { get; init; }
}
```

### 9.3 Mod 系统

```csharp
public interface IGameMod
{
    string Id { get; }
    string Name { get; }
    Version Version { get; }
    void Initialize(IGameModContext context);
    void OnGameStart(Game game);
    void OnGameEnd(Game game);
}
```

### 9.4 NPC 背景系统

```csharp
public sealed class NpcBackgroundConfig
{
    public string Origin { get; init; }
    public string Occupation { get; init; }
    public string Backstory { get; init; }
    public string Motivation { get; init; }
    public string Secret { get; init; }
}

public sealed class NpcRelationshipConfig
{
    public string TargetNpcId { get; init; }
    public string Type { get; init; }  // "Ally", "Rival", "Family", "Enemy"
    public int Strength { get; init; }
    public string Description { get; init; }
}
```

## 十、实施路线图

```
Phase 1 (4-6 周): 基础架构
  - 创建 HHSGame.Core 类库，迁移 Core/* 代码
  - 抽象 Cell/ColorPresets，移除 Terminal.Gui 依赖
  - 更新 HHSGame 引用 HHSGame.Core
  - 创建 HHSEditor Avalonia 项目骨架
  - 创建 HHSEditor.Core 服务层

Phase 2 (4-6 周): 基础编辑器
  - 物品/武器/盔甲目录编辑器
  - 职业编辑器
  - 敌人编辑器
  - 地图编辑器（Canvas 渲染、工具、导入导出）
  - 交叉引用验证

Phase 3 (4-6 周): 高级编辑器
  - 对话节点图编辑器
  - 任务编辑器
  - NPC 编辑器（背景/关系）
  - 阵营编辑器
  - 种族编辑器
  - 故事弧线编辑器

Phase 4 (4-6 周): 引擎扩展
  - IDataSource 抽象 + CompositeDataSource
  - Bundle 格式实现（BundleBuilder + BundleLoader）
  - 程序化生成规则系统
  - 故事系统（StoryManager + 运行时钩子）
  - NPC 背景/关系运行时支持
  - Mod 加载系统

Phase 5 (3-4 周): 脚本系统
  - 集成 Jint
  - 定义 IGameScriptAPI 和实现
  - 脚本生命周期管理
  - 钩子系统（onTurnEnd、onEnemyKilled、onDialogue 等）
  - 编辑器中的脚本编辑器（语法高亮）

Phase 6 (2-3 周): LLM 集成
  - ILLMProvider 抽象
  - OpenAI 兼容 Provider 实现
  - Ollama Provider 实现
  - Prompt 模板系统
  - 对话/任务/故事编辑器集成
  - 结构化输出解析 + 验证

Phase 7 (3-4 周): 完善与插件系统
  - 插件加载（AssemblyLoadContext）
  - 插件管理器 UI
  - 撤销/重做系统
  - 项目保存/加载
  - 预览/测试模式（用编辑器数据启动游戏）
  - 文档
```

## 十一、关键文件清单

| 文件 | 影响 |
|------|------|
| `Core/Rendering/Cell.cs` | 必须移除 Terminal.Gui 依赖，改为 GameAttribute |
| `Core/Engine/GameEngineFactory.cs` | 扩展为接受 IDataSource |
| `Core/Engine/Config/GameConfig.cs` | 扩展种族、故事、ProcGen、脚本、Mod 配置 |
| `Core/CoreExtensions.cs` | 注册新服务（StoryManager、IScriptEngine、ModManager） |
| `Core/Game.cs` | 集成故事触发、脚本钩子、Mod 生命周期 |

## 十二、验收标准

1. 编辑器可独立运行，不依赖 Terminal.Gui
2. 所有游戏数据可通过编辑器可视化编辑
3. 编辑器可生成 .hhsbundle 数据包
4. HHS 引擎可加载 .hhsbundle 并正常运行
5. LLM 可生成对话、任务、故事内容并导入编辑器
6. 脚本系统可响应游戏事件并修改游戏状态
7. 程序化生成规则可通过 JSON 配置
8. 所有现有测试通过
