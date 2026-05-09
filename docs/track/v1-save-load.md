# v1 — Save/Load System (存档与加载系统)

## 1. Vision / 背景动机

HHSGame 当前无持久化能力。玩家退出游戏后所有进度丢失，这是 TODO.md 中阶段一明确标记为未完成的核心功能。存档系统是 Roguelike 游戏的基础设施，直接影响可玩性。

### Scope
- 实现 `SaveManager` 服务，将游戏完整状态序列化为 JSON 文件
- 保存内容：玩家状态（属性、技能、位置、装备、背包）、地图状态（已探索区域、物品位置）、敌人状态、NPC 状态、任务进度、对话状态、回合数、活跃效果
- 实现 `LoadManager` 服务，从 JSON 文件反序列化并恢复游戏状态
- 支持多存档槽位（默认 3 个：slot-1.json, slot-2.json, slot-3.json）
- 保存到用户数据目录：`~/.hhs-game/saves/`
- UI 触发：`Ctrl+S` 保存，`Ctrl+L` 加载
- 引擎配置 `GameConfig` 支持存档路径覆盖
- 单元测试覆盖序列化/反序列化核心逻辑

### Non-Goals
- 不实现自动存档（Auto-Save）
- 不实现云存档同步
- 不实现存档压缩/加密
- 不实现跨版本存档兼容性迁移（v1 不考虑）
- 不实现存档预览/缩略图
- 不涉及联网功能

## 3. Data Model (Schema-first)

### SaveGameData（顶层存档结构）
```
SaveGameData {
  Version: int              // 存档格式版本，当前为 1
  SaveName: string          // 存档名称（自动生成："{玩家名} - 第{N}回合"）
  SavedAt: DateTime         // 保存时间（UTC）
  CurrentTurn: int          // 当前回合数
  ActivePlayerIndex: int    // 当前活跃玩家索引
  Players: PlayerSaveData[] // 所有玩家存档数据
  Enemies: EnemySaveData[]  // 敌人存档数据
  Npcs: NpcSaveData[]       // NPC 存档数据
  Map: MapSaveData          // 地图存档数据
  Quests: QuestSaveData[]   // 任务存档数据
  Items: MapItemSaveData[]  // 地面物品存档数据
  Events: string[]          // 事件日志（最近 N 条）
}
```

### PlayerSaveData
```
PlayerSaveData {
  Name: string
  Glyph: char
  X: int
  Y: int
  Attributes: { Strength, Perception, Agility, Charisma, Intelligence }
  Skills: Dictionary<SkillType, int>
  CurrentHp: int
  MaxHp: int
  CurrentSp: int
  MaxSp: int
  CurrentAp: int
  MaxAp: int
  Level: int
  Experience: int
  EquippedWeaponId: string?
  EquippedArmorId: string?
  Inventory: InventoryItemSaveData[]
  IsSneaking: bool
  ActiveEffects: ActiveEffectSaveData[]
}
```

### InventoryItemSaveData
```
InventoryItemSaveData {
  Id: string           // 物品 catalog ID
  Type: string         // "Weapon" | "Armor" | "Consumable" | "KeyItem"
  Quantity: int
}
```

### EnemySaveData
```
EnemySaveData {
  Id: string            // 敌人 catalog ID
  Name: string
  X: int
  Y: int
  CurrentHp: int
  MaxHp: int
  IsDead: bool
}
```

### NpcSaveData
```
NpcSaveData {
  Id: string
  Name: string
  X: int
  Y: int
  DialogueId: string
}
```

### MapSaveData
```
MapSaveData {
  Width: int
  Height: int
  Tiles: string          // Base64 编码的 tile 数据
  Explored: bool[,]      // 已探索区域标记（RLE 压缩）
  SpecialPositions: { Position: {X,Y}, Symbol: char }[]
}
```

### QuestSaveData
```
QuestSaveData {
  Id: string
  Status: string         // "Inactive" | "Active" | "Completed"
  ObjectiveProgress: { Target: string, Current: int, Required: int }[]
}
```

### MapItemSaveData
```
MapItemSaveData {
  Id: string             // 物品 catalog ID
  X: int
  Y: int
  Quantity: int
  IsHidden: bool
}
```

### ActiveEffectSaveData
```
ActiveEffectSaveData {
  Type: string           // "Stun" | "DamageOverTime" | etc.
  Duration: int
  Power: int
}
```

## 4. UX / 流程

### 保存流程
1. 玩家按下 `Ctrl+S`
2. 若当前不在 `Exploring` 状态，忽略按键
3. 显示存档槽位选择弹窗（3 个槽位，显示已有存档信息）
4. 选择槽位后，`SaveManager.Save()` 执行序列化
5. 写入 `~/.hhs-game/saves/slot-{N}.json`
6. 状态栏显示 "Game saved." 提示，持续 3 秒

### 加载流程
1. 玩家按下 `Ctrl+L`
2. 若当前不在 `Exploring` 或 `GameOver` 状态，忽略按键
3. 显示存档槽位选择弹窗（显示每个槽位的存档名称、保存时间、回合数）
4. 选择槽位后，`LoadManager.Load()` 执行反序列化
5. 验证存档版本兼容性（Version == 1）
6. 重建游戏状态：恢复玩家、敌人、NPC、地图、任务
7. 恢复 FOV，刷新 UI
8. 状态栏显示 "Game loaded." 提示

### 存档槽位选择 UI
- 使用 Terminal.Gui Dialog
- 每个槽位显示：槽位编号、存档名称、保存时间、回合数
- 空槽位显示 "--- Empty ---"
- 支持 `D` 锥删除已有存档（需确认）

## 5. 依赖与边界

### 依赖
- `System.Text.Json`（.NET 内置，无额外依赖）
- `System.IO`（文件系统操作）
- 现有系统：`GameContext`, `Player`, `EnemyManager`, `NpcManager`, `MapState`, `QuestManager`, `ItemManager`, `InventoryManager`, `TurnManager`, `PartyState`

### 边界
- 存档目录不存在时自动创建
- 存档文件损坏时显示错误提示，不崩溃
- 存档文件不存在时，加载按钮禁用
- 并发：同一时间只允许一个保存/加载操作

## 6. 验收标准（可二元判定）

| # | 验收条件 | 验证方式 |
|---|---------|---------|
| AC-1 | 按 `Ctrl+S` 在探索状态时，弹出存档槽位选择弹窗 | 手动测试 |
| AC-2 | 按 `Ctrl+S` 在非探索状态时，无响应 | 手动测试 |
| AC-3 | 选择槽位后，`~/.hhs-game/saves/slot-{N}.json` 文件存在且为合法 JSON | 单元测试 |
| AC-4 | 保存文件包含 `Version`, `SaveName`, `SavedAt`, `CurrentTurn` 字段 | 单元测试 |
| AC-5 | 保存文件包含所有玩家的 `Name`, `X`, `Y`, `Attributes`, `CurrentHp` | 单元测试 |
| AC-6 | 保存文件包含所有敌人的 `Id`, `X`, `Y`, `CurrentHp`, `IsDead` | 单元测试 |
| AC-7 | 保存文件包含地图 `Tiles` 和 `Explored` 数据 | 单元测试 |
| AC-8 | 保存文件包含任务进度 `ObjectiveProgress` | 单元测试 |
| AC-9 | 按 `Ctrl+L` 时弹出存档槽位选择弹窗，显示已有存档信息 | 手动测试 |
| AC-10 | 加载后玩家位置、生命值、背包与保存时一致 | 单元测试 |
| AC-11 | 加载后敌人位置、生命值与保存时一致 | 单元测试 |
| AC-12 | 加载后任务进度与保存时一致 | 单元测试 |
| AC-13 | 加载后地图已探索区域与保存时一致 | 单元测试 |
| AC-14 | 加载后 FOV 正确恢复 | 手动测试 |
| AC-15 | 存档文件损坏时显示错误提示，不崩溃 | 单元测试 |
| AC-16 | 存档版本不匹配时显示 "Save version not compatible" 提示 | 单元测试 |
| AC-17 | 空槽位加载按钮禁用或显示提示 | 手动测试 |
| AC-18 | 删除存档后文件不存在 | 单元测试 |
| AC-19 | `SaveManager` 和 `LoadManager` 有完整的单元测试覆盖 | 单元测试 |
| AC-20 | 加载存档后游戏可正常继续（再保存再加载一致） | 手动测试 |

## 7. 风险与回退方案

| 风险 | 影响 | 应对 |
|------|------|------|
| `MapState` 内部状态复杂，序列化困难 | 加载后地图不一致 | 只序列化必要状态（tiles + explored + specials），加载时重新初始化 |
| `Enemy` 和 `Player` 有循环依赖（通过 `GameContext`） | JSON 序列化死循环 | 使用 ID 引用而非对象引用，加载时重建关联 |
| `ActiveEffect` 多态序列化 | 反序列化丢失子类信息 | 使用类型鉴别器 `Type` 字段 |
| 存档文件过大 | 磁盘占用 | Map tiles 使用 RLE 压缩 |

## 8. 用例与边界条件

### 用例 1：保存并加载（主路径）
- **角色**：正在探索的玩家
- **前置条件**：游戏进行中，无战斗
- **触发**：按下 `Ctrl+S`，选择槽位 1
- **步骤**：保存 → 退出游戏 → 重新启动 → 按 `Ctrl+L` → 选择槽位 1
- **预期**：游戏恢复到保存时的状态，玩家可继续操作

### 用例 2：战斗中尝试保存（边界路径）
- **前置条件**：正在战斗中（`Planning` 状态）
- **触发**：按下 `Ctrl+S`
- **预期**：无响应或显示 "Cannot save during combat"

### 用例 3：加载损坏的存档（边界路径）
- **前置条件**：存档文件内容为非 JSON 文本
- **触发**：选择该槽位加载
- **预期**：显示 "Save file is corrupted" 错误提示，返回当前游戏

### 用例 4：覆盖已有存档（边界路径）
- **前置条件**：槽位 1 已有存档
- **触发**：保存到槽位 1
- **预期**：覆盖旧存档，显示确认提示或直接覆盖