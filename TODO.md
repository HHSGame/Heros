开发任务列表
为了实现上述游戏，我们将开发任务分解为几个核心阶段和模块。

# 阶段一：核心系统搭建 (Foundation Systems)

## 终端显示与用户输入模块 (Terminal Display & Input Module)

- [X] 实现基于 ASCII 或 Unicode 的地图和角色渲染。
- [X] 处理键盘输入（方向键、交互键、菜单键）。
- [X] 实现消息日志显示区域（用于战斗信息、事件通知）。
- [X] 状态栏显示（生命值、魔法值、经验值、当前位置、时间）。

## 游戏循环与状态管理 (Game Loop & State Management)

- [ ] 实现回合制游戏循环（玩家回合 -> 怪物回合 -> NPC 回合 -> 环境更新）。
  - 现状：TurnManager 已有基础结构，但非 Phase-based 设计。需重构为 PlayerPlanning → PlayerExecution → EnemyTurn → NpcTurn → EnvironmentUpdate → Cleanup 的标准流程。
- [ ] 管理游戏状态（探索、战斗、对话、菜单、库存）。
  - 现状：GameStateMachine 存在且支持 Exploration/Combat/Dialogue/Menu/Inventory/GameOver。部分状态（GameOver、Shop）未完全接入。
- [ ] 保存/加载游戏进度系统。
  - 现状：SaveManager 已有完整的 Capture/Save 逻辑，但缺少 LoadManager、UI 入口和 GameStateMachine 集成。

## 地图生成与管理 (Map Generation & Management)

- [X] 实现程序化生成地图算法（例如：洞穴生成、房间连接、地形生成）。
  - 现状：MapLoader 支持 .txt 文件加载，有基础的自动生成（empty map 或 file-based）。
- [ ] 支持预定义地图区域（用于重要地点如村庄、高塔）。
  - 现状：已有 6 个 .txt 地图文件，但无村庄/高塔等特殊区域。
- [X] 地图元素定义（墙壁、地面、门、水、特殊地形）。
  - 现状：TilePresets 定义了基本地形类型（墙/地/水/门/草/灌木等）。
- [X] 视野（Fog of War）系统，只显示玩家视野范围内的区域。
  - 现状：MapState.IsExplored/IsVisible 已实现。

## 基本角色系统 (Basic Character System)

- [X] 玩家角色定义（属性：力量、敏捷、体质、智力、感知；生命值、魔法值）。
  - 现状：CharacterStats + Attributes (Strength/Perception/Agility/Intelligence/Charisma) + Skills (15种) + HP/SP/AP。
- [X] 基础移动（上下左右）和碰撞检测。
  - 现状：CollisionSystem + 方向移动 + 对角线移动 + 1.5 AP 消耗。
- [X] 物品栏（Inventory）系统（拾取、丢弃、使用物品）。
  - 现状：InventoryManager + ObservableItems + AddItem/RemoveItem/GetItems。

# 阶段二：核心玩法实现 (Core Gameplay)

## 战斗系统 (Combat System)

- [X] 回合制战斗逻辑（攻击、防御、技能）。
  - 现状：ActionSequence + AP 驱动的行动规划，支持近战/远程/魔法/防御/巡逻。
- [X] 伤害计算、命中率、暴击系统。
  - 现状：CombatResolver 实现了伤害计算、命中率、护甲穿透、暴击判定。
- [X] 怪物 AI（基础攻击、寻路、追逐）。
  - 现状：EnemyAbilitySystem + Pathfinder + 四种状态（Idle/Chasing/Attacking/Fleeing）。
- [X] 经验值获取与升级系统（属性提升、技能点）。
  - 现状：Progression 类实现了经验值/等级/技能点分配。
- [X] 战利品（Loot）生成系统（怪物掉落、宝箱）。
  - 现状：EnemyLootSystem + config-driven 掉落。

## 物品与装备系统 (Items & Equipment System)

- [X] 定义物品类型（武器、防具、消耗品、任务物品、杂物）。
  - 现状：ItemType 枚举 + Item.cs 定义了完整物品系统。
- [ ] 装备槽位管理（武器、头部、身体、腿部、饰品等）。
  - 现状：仅支持武器槽位（EquippedWeapon），无防具/饰品装备槽位 UI。
- [X] 物品属性（攻击力、防御力、魔法加成、特殊效果）。
  - 现状：Damage/ApCost/Penetration/Armor/ApCost 属性。
- [ ] 物品稀有度与鉴定系统。
  - 现状：Item 无 Rarity 字段，无 IsIdentified 机制。

## 技能与魔法系统 (Skills & Magic System)

- [ ] 定义不同职业或派系的技能树。
  - 现状：Classes 定义了 6 种职业和各自的技能分布，但无技能树 UI。
- [X] 实现主动技能和被动技能。
  - 现状：SkillActions 支持 12 种主动技能动作（Stealth/Heal/Inspire/Track/Repair 等）。
- [ ] 魔法消耗与冷却机制。
  - 现状：SP 存在但技能不消耗 SP，无冷却系统。

# 阶段三：世界交互与叙事 (World Interaction & Narrative)

## 怪物与 NPC 系统 (Enemy & NPC System)

- [X] 定义怪物类型（名称、外观、属性、行为模式、掉落物）。
  - 现状：Config-driven enemies.json + EnemyAbilitySystem + EnemyLootSystem。
- [X] 定义 NPC 类型（名称、外观、立场、对话树）。
  - 现状：NpcManager + NpcFactory + config-driven NPC 定义。
- [ ] NPC 寻路和日常行为（可选）。
  - 现状：NPC 是静态的，无移动/日常行为。

## 对话与任务系统 (Dialogue & Quest System)

- [X] 实现对话树（多分支对话、选项影响）。
  - 现状：DialogueManager + DialogueDefinition/DialogueNode/DialogueOption，支持需求检定、效果触发。
- [X] 任务日志（记录主线和支线任务）。
  - 现状：QuestLogWindow + QuestManager。
- [ ] 任务目标跟踪与完成条件检测。
  - 现状：QuestObjectiveComplete 检查存在，但无实时进度 UI。
- [ ] 任务奖励发放。
  - 现状：GiveItem/GiveCurrency 已实现，无经验值奖励和装备奖励。

## 世界交互与背景故事 (World Interaction & Lore)

- [ ] 可互动对象（门、开关、宝箱、书籍、碑文、雕像）。
  - 现状：无 IInteractable 接口或互动对象框架。
- [ ] "检查"功能：玩家可以检查地图上的任何对象，获取其背景信息和传说。
  - 现状：无 inspect 命令。
- [ ] 物品描述系统：每个物品都有详细的背景和故事。
  - 现状：Item.Description 字段存在，但无详细展示 UI。
- [ ] 解谜系统：通过环境交互、物品使用、对话来推进。
  - 现状：无解谜系统。

# 阶段四：高级功能与完善 (Advanced Features & Refinement)

## 音效与音乐集成 (Sound Effects & Music Integration) (可选，但推荐)

- [ ] 播放背景音乐和环境音效。
- [ ] 播放战斗音效、交互音效。

## UI/UX 优化 (User Interface/Experience Optimization)

- [ ] 菜单系统（主菜单、设置、帮助）。
  - 现状：PlayerSetupWizard 用于角色创建，UtilityWindow 用于设置，无独立主菜单。
- [ ] 快捷键设置与提示。
  - 现状：有基础快捷键（方向键/Tab/Enter/T/Q 等），无自定义设置和帮助提示。
- [ ] 错误处理与用户友好提示。
  - 现状：基础错误处理已存在。

## 故事脚本与内容填充 (Story Scripting & Content Population)

- [ ] 根据"阿卡迪亚的挽歌"故事，编写所有对话、任务文本。
- [ ] 设计所有地图区域的布局和细节。
- [ ] 创建所有怪物、NPC 和物品的具体数值和描述。

## 教程与新手引导 (Tutorial & Onboarding)

- [ ] 引导玩家熟悉游戏机制和界面。
- [ ] 逐步介绍核心玩法。

# 阶段五：测试与迭代 (Testing & Iteration)

## Bug 修复与性能优化 (Bug Fixing & Performance Optimization)

- [ ] 识别并修复游戏中的错误。
- [ ] 优化游戏性能，确保流畅运行。

## 平衡性调整 (Balance Adjustments)

- [ ] 调整战斗难度、经验值获取、物品掉落率。
- [ ] 确保角色成长曲线合理。

## 玩家反馈与迭代 (Player Feedback & Iteration)

- [ ] 收集玩家反馈，进行后续改进。

---

# Engine Refactoring 状态

## 已完成 ✅
- Config models + loader (GameConfig + GameConfigLoader)
- Engine bootstrap (GameEngineFactory → Program)
- CLI --config 支持 (GameEngineLauncher)
- Scripted input runner (GameScriptRunner + GameScriptInputAdapter)
- Catalog system (5 个 catalog JSON + GameCatalogLoader)
- Win/Lose conditions (12 种条件类型)
- Multi-player spawns (PartyState + ActivePlayer)
- NPC/Quest/Dialogue 系统 (对话树 + 任务 + 成就)
- Skill actions (17 种技能动作，含 Examine/UseObject)
- Ranged targeting (射线/弧线弹道 + 射程覆盖)
- Combat planning UI (行动序列规划)
- Save/Load 系统 (SaveManager + LoadManager + SaveLoadSlotDialog)
- Game.cs 拆分 (SkillActionExecutor + ConditionEvaluator)
- 装备槽位系统 (6 槽位: Weapon/Head/Body/Legs/Accessory1/Accessory2)
- 主菜单 + 帮助页面 (MainMenuView + HelpView)
- 互动对象框架 (IInteractable + 4 种实现)
- 教程系统 (TutorialManager + TutorialHintDialog)
- 游戏内容扩充 (15 怪物 / 22 武器 / 12 盔甲 / 6 物品)

## 待开始 📋
- Phase-based TurnManager 重构
- 物品稀有度与鉴定系统
- 基础解谜系统
- 主线剧情场景
- 支线任务扩充
- 战斗平衡调整

# 改进计划优先级

| 优先级 | 阶段 | 预估工作量 | 依赖 |
|--------|------|------------|------|
| P0 最高 | A: Save/Load 系统集成 | ~10h | 无 |
| P1 高 | B: 回合制游戏循环规范化 | ~7h | 无 |
| P2 中高 | C: 物品与装备系统增强 | ~9h | B |
| P3 中 | D: 世界交互系统 | ~9h | A, B |
| P4 中 | E: UI/UX 优化与内容 | ~10h | A, B, C |
| P5 持续 | F: 测试、平衡与优化 | ~15h | 所有阶段 |
</parameter>
<task_progress>
- [x] Create improvement plan
- [ ] P0: Implement Save/Load system
  - [x] Found existing SaveManager in Core/Save/
  - [x] Identified missing APIs
  - [x] Deleted duplicate Core/Saves/ directory
  - [x] Updated TODO.md with current progress
  - [ ] Create SaveDataModels.cs in Core/Save/
  - [ ] Add missing helper methods (PartyState.Currency, DialogueManager APIs, etc.)
  - [ ] Fix existing SaveManager to use correct APIs
  - [ ] Create LoadManager in Core/Save/
  - [ ] Add Save/Load UI
  - [ ] Wire into GameStateMachine + keyboard shortcuts
  - [ ] Write unit tests
  - [ ] Build & verify
</task_progress>
</write_to_file>