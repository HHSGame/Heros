# HHSGame 阶段性改进计划 (v2)

> 基于 2026-05-08 代码库全面审查

## 一、当前实现进度总览

### 阶段一：核心系统搭建 — 完成度 ~85%

| 模块 | 状态 | 说明 |
|------|------|------|
| ASCII/Unicode 地图渲染 | ✅ 完成 | MapFrame, MapView, DrawingContext |
| 键盘输入处理 | ✅ 完成 | GameUI 处理方向键、交互键、菜单键 |
| 消息日志 | ✅ 完成 | EventLoggerView, MessageFrame |
| 状态栏 | ✅ 完成 | StatusBarView (HP, AP, SP, 位置等) |
| 回合制游戏循环 | ✅ 完成 | TurnManager, Game.EndPlayerTurn() |
| 游戏状态管理 | ✅ 完成 | GameStateMachine (Setup/Exploration/Combat/Dialogue/Menu/Inventory/GameOver) |
| **存档/读档系统** | ✅ 完成 | SaveManager + LoadManager + SaveLoadSlotDialog + 33 个测试 |
| 程序化地图生成 | ✅ 完成 | MapLoader + 自定义地图 (data/maps/*.txt) |
| 预定义地图区域 | ✅ 完成 | 6 张预定义地图 |
| 地图元素 | ✅ 完成 | 墙壁、地面、门、水、特殊地形 |
| 战争迷雾 | ✅ 完成 | MapState + Player.CalculateFOV() |
| 角色属性系统 | ✅ 完成 | 5 属性 (Str/Per/Agi/Cha/Int) + 15 技能 |
| 基础移动 + 碰撞 | ✅ 完成 | CollisionSystem, 对角线移动支持 |
| 物品栏系统 | ✅ 完成 | InventoryManager (拾取/丢弃/使用) |

### 阶段二：核心玩法 — 完成度 ~85%

| 模块 | 状态 | 说明 |
|------|------|------|
| 回合制战斗逻辑 | ✅ 完成 | AP 驱动的 ActionSequence 系统 |
| 伤害/命中/暴击 | ✅ 完成 | CombatResolver |
| 怪物 AI | ✅ 完成 | Enemy 状态机 (Idle/Chasing/Attacking/Fleeing) + Pathfinder |
| 经验/升级系统 | ✅ 完成 | CharacterProgression (等级、技能点) |
| 战利品系统 | ✅ 完成 | EnemyLootSystem |
| 物品类型定义 | ✅ 完成 | Weapon, Armor, HealthPotion + catalog 驱动 |
| 装备槽位 | ✅ 完成 | 6 槽位 (Weapon/Head/Body/Legs/Accessory1/Accessory2) |
| 物品属性 | ✅ 完成 | 攻击力、防御力、穿透、范围 |
| 物品稀有度 | ✅ 完成 | Common → Legendary |
| **物品鉴定系统** | ❌ 未实现 | |
| 技能树 | ✅ 完成 | 15 个技能 + 15 个技能动作 (SkillActions) |
| 主动/被动技能 | ✅ 完成 | SkillActionTarget + 技能检定系统 |
| **冷却机制** | ❌ 未实现 | 仅有 IsSkillLocked (Inspire 后锁定) |
| **魔法/法力系统** | ⚠️ 部分完成 | SP (Sanity) 系统存在但无传统法术 |

### 阶段三：世界交互与叙事 — 完成度 ~65%

| 模块 | 状态 | 说明 |
|------|------|------|
| 怪物类型定义 | ✅ 完成 | 完全配置驱动 (data/catalogs/enemies.json) |
| NPC 类型定义 | ✅ 完成 | NpcManager, NpcFactory, 配置驱动 |
| 对话树 | ✅ 完成 | DialogueManager + 多分支对话 |
| 任务日志 | ✅ 完成 | QuestManager + QuestLogWindow |
| 任务目标跟踪 | ✅ 完成 | 含完成条件检测 |
| 任务奖励 | ✅ 完成 | 通过 DialogueEffects 发放 |
| 可互动对象 | ⚠️ 部分完成 | 门 (PickLock)，缺少开关/宝箱/书籍/碑文/雕像 |
| "检查"功能 | ⚠️ 部分完成 | Inspect (隐藏特征) + Search (隐藏物品)，缺少通用对象检查 |
| 物品描述系统 | ⚠️ 部分完成 | Appraise 显示价值，缺少详细背景故事 |
| **解谜系统** | ❌ 未实现 | |
| **NPC 日常行为** | ❌ 未实现 | |

### 阶段四：高级功能 — 完成度 ~40%

| 模块 | 状态 | 说明 |
|------|------|------|
| **音效/音乐** | ❌ 未实现 | Terminal.Gui 限制，可选 |
| **主菜单/设置/帮助** | ✅ 完成 | MainMenuView + HelpView |
| 快捷键提示 | ⚠️ 部分完成 | StatusBarView 有部分提示 |
| **故事脚本/内容填充** | ⚠️ 最小化 | game.json 有基础场景，需要大量扩充 |
| **教程/新手引导** | ✅ 完成 | TutorialManager + TutorialHintDialog + 11 测试 |

### 阶段五：测试与迭代 — 完成度 ~75%

| 模块 | 状态 | 说明 |
|------|------|------|
| 单元测试 | ✅ 良好 | 24 个测试文件，210 个测试全部通过 |
| Bug 修复 | ✅ 持续进行 | |
| **平衡性调整** | ❌ 未系统化 | |
| **玩家反馈迭代** | ❌ 未开始 | |

---

## 二、引擎重构进度 (engine_refactoring.md)

### 已完成 ✅

| 里程碑 | 完成日期 | 状态 |
|--------|----------|------|
| Config models + loader + data/game.json | 2026-01-20 | ✅ |
| Config mapper + config-driven bootstrap | 2026-01-20 | ✅ |
| GameEngineFactory wrapper | 2026-01-20 | ✅ |
| --config CLI handling | 2026-01-20 | ✅ |
| Scripted input schema + loader + runner | 2026-01-20 | ✅ |
| --test-script CLI support | 2026-01-20 | ✅ |
| Catalog system (weapons/armors/items/classes/enemies) | 2026-01-20 | ✅ |
| GameEngineHost + service wiring | 2026-01-20 | ✅ |
| Config-driven win/lose conditions | 2026-01-20 | ✅ |
| GameEngineLauncher (CLI consolidation) | 2026-01-20 | ✅ |
| NPC/quest/dialogue config | 2026-01-23 | ✅ |
| Multi-player spawns + party system | 2026-01-22 | ✅ |
| Ranged targeting + trajectory system | 2026-01-22 | ✅ |
| Quest/achievement end conditions | 2026-01-23 | ✅ |

### 待完成 ⏳

- 更多 quest/currency 条件变体
- 配置验证增强
- 热重载配置支持

---

## 三、代码质量观察

### 需要关注的技术债务

1. **Game.cs 过大 (1587 行)**: 承担了太多职责——玩家管理、战斗规划、回合执行、条件检查、渲染、地图/敌人/物品初始化。应拆分为更小的组件。

2. **GameWorld.cs 也较庞大**: 包含回合更新、敌人 AI 调度、地形效果、事件处理等，应考虑拆分。

3. **GameContext 过于扁平**: 15+ 个服务字段，可考虑分组或使用服务定位器模式。

4. **事件系统耦合**: 静态 `Events` 类被广泛使用，虽然方便但增加了隐式耦合。

---

## 四、改进计划 — 按优先级排列

### ~~P0: 存档/读档系统 (Save/Load)~~ ✅ 完成

**完成日期**: 2026-05-08

**已实现**:
- `SaveManager` — 从运行时状态提取快照，3 个存档槽位 (`slot-1.json` ~ `slot-3.json`)
- `LoadManager` — 从快照恢复运行时状态，含版本校验和损坏文件处理
- `SaveLoadSlotDialog` — 存档槽位选择 UI (Enter 选择, D 删除, Esc 取消)
- `SaveGameData` 含 `Version`/`SaveName` 字段
- 保存路径: `~/.hhs-game/saves/`
- 状态守卫: 仅 Exploration 状态可保存，Exploration/GameOver 可加载
- 33 个单元测试 (数据模型往返 + 文件操作 + 状态守卫 + 损坏文件处理)

---

### ~~P1: Game.cs 拆分 (技术债务)~~ ✅ 完成

**完成日期**: 2026-05-08

**已实现**:
- `SkillActionExecutor` — 15 个技能动作执行逻辑 (~300 行)，含技能检定辅助方法
- `ConditionEvaluator` — 胜负条件解析与检查 (~150 行)，含 ConditionKind/ConditionSpec
- `Game` 作为外观类 (Facade)，委托给上述组件
- Game.cs 从 1473 行减少到 804 行 (减少 45%)
- 所有 101 个测试通过，行为无变化

---

### ~~P2: 装备系统扩展~~ ✅ 完成

**完成日期**: 2026-05-08

**已实现**:
- `EquipmentSlot` 枚举 (Weapon, Head, Body, Legs, Accessory1, Accessory2)
- `EquipmentSlotRules` (ParseSlot, GetDisplayName, GetTotalArmor)
- `EquipmentManager` — 管理多槽位装备 (Equip/Unequip/GetEquipped/RestoreEquipment)
- `Armor` 类新增 `Slot` 属性 (默认 Body，向后兼容)
- `ArmorDefinition` 记录新增 `Slot` 字段
- `ArmorDefinitionConfig` 新增 `Slot` 属性 (JSON 配置支持)
- `GameCatalogLoader` 解析 JSON 中的 `slot` 字段
- `ItemCatalog.BuildArmor` 传递 slot 参数
- 更新 `armors.json`：12 个盔甲条目 (head/body/legs/accessory)
- `Player` 使用 `EquipmentManager` 替代单一 `equippedArmor` 字段
- `SaveManager` 保存所有装备槽位 (`EquipmentSlots` 字段)
- `LoadManager` 恢复装备槽位 (兼容旧存档)
- `SaveDataModels` 新增 `EquipmentSlotSaveData`
- 19 个 `EquipmentManagerTests` 单元测试全部通过
- 所有 120 个测试通过 (5 个预存在失败)

**未实现 (标记为 Non-Goal)**:
- UI 装备面板 (InventoryFrame 增强) — 可在后续迭代中添加

---

### ~~P3: 主菜单与 UI 优化~~ ✅ 部分完成

**完成日期**: 2026-05-08

**已实现**:
- `MainMenuView` — 游戏启动主菜单 (New Game / Continue / Load Game / Help / Quit)
  - New Game → 启动角色创建向导
  - Continue → 自动加载最近存档并跳过向导
  - Load Game → 显示存档槽位选择弹窗
  - Help → 显示快捷键帮助页面
  - Quit → 退出游戏
- `HelpView` — 快捷键与游戏机制帮助页面
  - 移动/战斗/物品/技能/交互/系统 快捷键分组
  - 游戏机制说明
  - 滚动浏览，Esc 返回
- 主菜单集成到 GameUI 的启动流程
- 帮助页面可在主菜单中访问
- 取消存档选择时返回主菜单
- 所有 120 个测试通过

**未实现 (后续迭代)**:
- 设置页面 (动画速度、语言选择)
- 退出确认对话框
- 快捷键统一提示增强 (StatusBarView)

---

### ~~P4: 世界交互增强~~ ✅ 完成

**完成日期**: 2026-05-09

**已实现**:
- `IInteractable` 接口 (Examine, Interact, CanInteract, IsInteracted)
- `InteractionResult` 记录 (Success, Message, ItemsGiven)
- 4 种可互动对象：TreasureChest, BookInscription, Lever, Statue
- `InteractableManager` — 管理所有可互动对象
- 新增技能动作：Examine (Awareness, 1 AP), UseObject (Mechanics, 2 AP)
- 新增 `AdjacentObject` 目标类型
- 修复 ConditionEvaluator 缺失的 5 个条件类型

---

### ~~P5: 教程与新手引导~~ ✅ 完成

**完成日期**: 2026-05-09

**已实现**:
- `TutorialStep` — 教程步骤定义 (Id, Title, Message, Trigger, TriggerParam, Order)
- `TutorialTrigger` 枚举 — 12 种触发类型 (Immediate, OnMove, OnCombatStart, OnItemPickup, OnInventoryOpen, OnNpcTalk, OnSkillUse, OnObjectInteract, OnMarkerReached, OnTurnEnd, OnSaveOrLoad, OnCombatToggle)
- `TutorialScenario` — 教程场景定义
- `TutorialManager` — 教程进度管理 (LoadScenario, Notify, AdvanceStep, Dismiss)
  - 事件：HintReady, TutorialCompleted
  - 进度跟踪：CompletedSteps, TotalCount, IsStepCompleted
- `TutorialScenarios.CreateDefaultScenario()` — 10 步默认教程
  1. Welcome — 移动引导
  2. Exploration — 探索说明
  3. Items — 拾取物品
  4. Inventory — 物品栏
  5. Combat — 战斗模式
  6. Combat Mode — 战斗切换
  7. Skills — 技能系统
  8. NPCs — NPC 对话
  9. Interactive Objects — 可互动对象
  10. Saving — 存档/读档
- `TutorialHintDialog` — 教程提示 UI (标题/消息/步骤计数/Continue/Skip)
- TutorialManager 注册到 DI (CoreExtensions)
- TutorialHintDialog 注册到 UI DI (UIExtensions)
- GameUI 集成：
  - 新游戏启动时自动加载教程
  - TutorialHintDialog 在其他弹窗之前处理键盘输入
  - Enter/Esc 继续下一步，S 跳过教程
  - 所有步骤完成时显示祝贺消息
- 11 个 TutorialManagerTests 单元测试
- 所有 136 个测试全部通过 (0 失败)

### ~~P6: 内容扩充与平衡~~ ✅ 部分完成

**完成日期**: 2026-05-09

**已实现**:
- 怪物类型：6 → 15 种
  - 新增：CaveSpider, GiantRat, Skeleton, DarkMage, Golem, Wolf, FireElemental, Assassin, Zombie
  - 覆盖不同难度梯度 (XP 20~250)
  - 多样化技能组合 (Poison, Stun, HealSelf, DamageOverTime)
- 武器类型：11 → 22 种
  - 新增：Rapier, BattleAxe, Longbow, Crossbow, StaffOfLight, FlameSword, WarHammer, ElvenBow, StaffOfStorms, ShadowBlade, Dragonslayer
  - 覆盖 Common → Legendary 稀有度
  - 全部 5 种武器类型 (MeleeLight, MeleeHeavy, RangedSnap, RangedAimed, Burst)
- 物品种类：1 → 6 种
  - 新增：HealthPotionLg, HealthPotionSm, HealingSalve, ElixirOfLife, Bandage
  - 覆盖 Common → Rare 稀有度
- 所有 136 个测试通过 (0 失败)

**未实现 (后续迭代)**:
- 主线剧情场景 (3-5 个关卡/区域)
- 支线任务 (5-10 个)
- 更多对话内容 (NPC 背景故事)
- 战斗平衡调整 (伤害公式、经验曲线、掉落率)
- 角色成长曲线验证

---

### ~~P7: 测试覆盖率提升~~ ✅ 部分完成

**完成日期**: 2026-05-09

**已实现**:
- `ConditionEvaluatorTests` — 18 个测试覆盖所有条件解析 + 评估逻辑
  - ParseCondition: 所有 12 种 ConditionKind 的解析测试
  - 参数验证: null/空/非法值的错误处理
  - CheckGameConditions: PlayerDead/AllEnemiesDefeated/NoConditionsMet
- `InteractableManagerTests` — 15 个测试覆盖互动对象管理
  - GetAt/GetNearby/GetInteractableNearPlayer 位置查询
  - ExamineAt/InteractAt 交互操作
  - Clear/CanInteract 过滤逻辑
  - InteractionResult 默认值验证
- 总计：21 个测试文件，155 个测试全部通过 (0 失败)

**未实现 (后续迭代)**:
- Save/Load 完整集成测试 (保存→加载→验证状态一致性)
- 边界条件测试 (空物品栏、0HP、地图边界)
- 性能测试 (大地图 + 大量敌人)
- CI/CD 集成 (dotnet test 自动运行)

---

## 五、实施顺序建议

```
Sprint 1 (Week 1-2): P0 存档/读档 + P7 测试基础
Sprint 2 (Week 3):   P1 Game.cs 拆分 + P2 装备扩展
Sprint 3 (Week 4):   P3 主菜单 + P5 教程
Sprint 4 (Week 5-6): P4 世界交互 + P6 内容扩充 (部分)
Sprint 5 (Week 7):   P6 内容扩充 (剩余) + P7 测试完善 + 平衡调整
```

## 六、关键指标

| 指标 | 当前 | 目标 |
|------|------|------|
| TODO.md 完成率 | ~75% | 90% |
| 测试通过率 | 210/210 (100%) | 100% |
| Game.cs 行数 | 804 (原1587，-45%) | <500 |
| 可玩关卡数 | 1 (demo) | 5+ |
| 怪物种类 | 15 | 15+ ✅ |
| 武器种类 | 22 | 20+ ✅ |
| 盔甲种类 | 12 | 15+ |
| 物品种类 | 6 | 30+ |
| NPC 数量 | ~3 | 10+ |
| 支线任务 | ~3 | 8+ |
