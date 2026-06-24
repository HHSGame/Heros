# HHSGame 项目实现进度与改进计划

> 生成日期: 2026-05-09
> 基于 TODO.md、engine_refactoring.md、v4-wwii-retheme.md 及源码分析

---

## 一、当前实现进度总览

### 阶段零：二战重制（v4）— 约 70% 完成

| 子模块 | 状态 | 说明 |
|--------|------|------|
| 角色创建系统 | ✅ 完成 | CharacterCreationManager + 对话式 UI（身份→命名→骰子→职业） |
| 阵营系统 | ✅ 完成 | FactionManager（8阵营/3关系/默认中立/动态关系） |
| 伪装系统 | ✅ 框架完成 | TryDisguise/CheckDisguiseBlow/GetEffectivePlayerFaction |
| 二战武器/装备/物品数据 | ✅ 完成 | 17 武器 / 16 装备 / 6 物品 |
| 二战敌人数据（含阵营） | ✅ 完成 | 15 种敌人，每个有 Faction 字段 |
| 二战职业数据 | ✅ 完成 | 6 种职业（抵抗/占领军/公务员/平民/流亡/匪徒） |
| NPC + 对话树 | ✅ 完成 | 7 个法国风格 NPC，7 套完整对话 |
| 任务系统（3 幕 17 个） | ✅ 完成 | 第一幕 6 个 / 第二幕 7 个 / 第三幕 4 个 |
| 成就系统 | ✅ 完成 | 5 个成就 |
| 主菜单 | ✅ 完成 | MainMenuView（New Game/Continue/Load/Help/Quit） |
| 教程系统 | ✅ 完成 | 10 步中文教程 |
| 中央城区地图 | ✅ 完成 | city-center.txt（40×25） |
| 敌人 AI 阵营集成 | ✅ 完成 | DetermineState + IsCombatActive 检查阵营 |
| 声望系统集成 | 🔄 待完成 | ModifyReputation 已有，需接入对话/任务事件 |
| 更多区域地图 | 🔄 待完成 | 需添加指挥部/营地/前线/工业区等 |
| 伪装 UI 集成 | 🔄 待完成 | 需玩家可主动伪装的交互界面 |

### 阶段一：核心系统搭建 — 约 85% 完成

| 子模块 | 状态 | 说明 |
|--------|------|------|
| ASCII/Unicode 地图渲染 | ✅ 完成 | MapView + DrawingContext |
| 键盘输入 | ✅ 完成 | 统一通过 Application.KeyDown 分发 |
| 消息日志 | ✅ 完成 | EventLoggerView |
| 状态栏 | ✅ 完成 | StatusBarView（HP/AP/名字/位置/回合） |
| 回合制游戏循环 | ⚠️ 部分完成 | TurnManager 驱动，Phase-based 待优化 |
| 游戏状态管理 | ✅ 完成 | GameStateMachine（Exploration/Combat/Menu/Inventory/Dialogue） |
| 保存/加载系统 | ✅ 完成 | SaveManager + LoadManager + SaveLoadSlotDialog UI |
| 程序化地图生成 | ⚠️ 部分完成 | MapLoader 支持 .txt 文件，无算法生成 |
| 视野系统（Fog of War） | ✅ 完成 | MapState.IsExplored / IsVisible |
| 角色属性系统 | ✅ 完成 | CharacterStats + Attributes + Skills |
| 移动与碰撞 | ✅ 完成 | CollisionSystem + 方向+对角线移动 |
| 物品栏 | ✅ 完成 | InventoryManager + EquipmentManager |

### 阶段二：核心玩法 — 约 80% 完成

| 子模块 | 状态 | 说明 |
|--------|------|------|
| 回合制战斗 | ✅ 完成 | ActionSequence + AP 驱动 |
| 伤害/命中/暴击 | ✅ 完成 | CombatResolver |
| 怪物 AI（含阵营） | ✅ 完成 | Pathfinder + EnemyAbilitySystem + FactionManager |
| 经验/升级 | ✅ 完成 | Progression + SkillPoints |
| 战利品生成 | ✅ 完成 | EnemyLootSystem |
| 物品类型 | ✅ 完成 | 武器/防具/消耗品/杂物/任务物品 |
| 装备槽位 | ✅ 完成 | EquipmentManager（Head/Body/Legs/Accessory1/Accessory2/Weapon） |
| 物品属性 | ✅ 完成 | Damage/ApCost/Penetration/Armor |
| 物品稀有度 | ✅ 完成 | ItemRarity（Common/Uncommon/Rare/Epic/Legendary） |
| 技能系统 | ✅ 完成 | SkillActionWindow + 12 种技能动作 |
| 阵营系统 | ✅ 完成 | FactionManager + 敌人 AI 集成 |
| 伪装系统 | ⚠️ 框架完成 | 需 UI 集成和敌方识破逻辑 |

### 阶段三：世界交互与叙事 — 约 75% 完成

| 子模块 | 状态 | 说明 |
|--------|------|------|
| 怪物定义（含阵营） | ✅ 完成 | Config-driven，enemies.json + faction 字段 |
| NPC 定义（含阵营） | ✅ 完成 | NpcManager + 配置 + faction 字段 |
| 对话树 | ✅ 完成 | DialogueManager + 多分支 + 需求检定 + 阵营效果 |
| 任务系统（3 幕） | ✅ 完成 | QuestManager + 17 个任务 + 5 个成就 |
| 任务日志 | ✅ 完成 | QuestLogWindow |
| 任务奖励 | ✅ 完成 | GiveItem / GiveCurrency / StartQuest |
| 可互动对象 | ✅ 完成 | InteractableManager + IInteractable 接口 |
| 检查/使用功能 | ✅ 完成 | ExamineAt / InteractAt |
| 物品描述系统 | ⚠️ 部分完成 | Item.Description 存在，展示 UI 待完善 |
| 解谜系统 | ❌ 未实现 | — |

### 阶段四：高级功能 — 约 60% 完成

| 子模块 | 状态 | 说明 |
|--------|------|------|
| 音效/音乐 | ❌ 未实现 | 终端限制，可选 |
| 主菜单系统 | ✅ 完成 | MainMenuView（New Game/Continue/Load/Help/Quit） |
| 角色创建 | ✅ 完成 | CharacterCreationDialog（对话式 6 步） |
| 快捷键 | ✅ 完成 | 统一快捷键方案 |
| 故事内容 | ✅ 完成 | 3 幕剧情 + 17 个任务 + 7 个 NPC 对话 |
| 教程/新手引导 | ✅ 完成 | TutorialManager + 10 步中文教程 |
| Save/Load UI | ✅ 完成 | SaveLoadSlotDialog |
| 帮助窗口 | ✅ 完成 | HelpView |

### 阶段五：测试 — 约 50% 完成

| 子模块 | 状态 | 说明 |
|--------|------|------|
| 单元测试 | ✅ 完成 | 30+ 个测试文件，234 个测试全部通过 |
| 集成测试 | ✅ 完成 | GameIntegrationTests + ScriptRunner + SaveLoadTests |
| 阵营系统测试 | ⚠️ 待补充 | FactionManager 单元测试待写 |
| 角色创建测试 | ✅ 完成 | CharacterCreationManagerTests + DiceRollerTests |
| 性能优化 | ❌ 未分析 | — |
| 平衡性调整 | ❌ 未实现 | — |

---

## 二、Engine Refactoring 进度

| 里程碑 | 状态 | 说明 |
|--------|------|------|
| Config models + loader | ✅ 完成 | GameConfig + GameConfigLoader |
| Engine bootstrap | ✅ 完成 | GameEngineFactory → Program |
| CLI --config 支持 | ✅ 完成 | GameEngineLauncher |
| Scripted input runner | ✅ 完成 | GameScriptRunner + GameScriptInputAdapter |
| Catalog system | ✅ 完成 | 5 个 catalog JSON + GameCatalogLoader |
| Win/Lose conditions | ✅ 完成 | 12 种条件类型 |
| Multi-player spawns | ✅ 完成 | PartyState + ActivePlayer |
| NPC/Quest/Dialogue | ✅ 完成 | 完整对话树 + 任务 + 成就 |
| Skill actions | ✅ 完成 | 12 种技能动作 |
| Ranged targeting | ✅ 完成 | 射线/弧线弹道 + 射程覆盖 |
| Save/Load integration | ✅ 完成 | SaveManager + LoadManager + UI |
| Faction system | ✅ 完成 | FactionManager + 敌人 AI 集成 |
| Character creation | ✅ 完成 | 对话式角色创建 |
| Test scripted flow | ✅ 完成 | 234 测试全部通过 |

---

## 三、改进计划

### 已完成的阶段

#### 阶段 A：Save/Load 系统集成 ✅
- SaveManager + LoadManager + SaveLoadSlotDialog
- 5 个存档槽位

#### 阶段 E：UI/UX 优化 ✅
- MainMenuView 主菜单
- CharacterCreationDialog 对话式角色创建
- TutorialManager 教程系统
- HelpView 帮助窗口
- 中文化（对话/教程/角色创建/UI）

#### 阶段 D：世界交互系统 ✅
- InteractableManager + IInteractable
- DialogueManager + 完整对话树
- QuestManager + 17 个任务

### 待完成的阶段

#### 阶段 B：声望与阵营动态系统（优先级：高，~5h）
1. 接入 `ModifyReputation` 到对话选择和任务完成事件
2. 实现对话中的 `SetRelation` 效果（攻击某阵营 → 变敌对）
3. 伪装 UI 集成（玩家可主动切换伪装）
4. 敌方识破伪装的 AI 逻辑

#### 阶段 C：更多区域地图（优先级：中高，~8h）
1. 指挥部地图（北部）
2. 抵抗军营地地图（东部山区）
3. 匪徒占据区地图（西部工业区）
4. 盟军前线地图（南部）
5. 居民区地图
6. 教堂/中立区域地图

#### 阶段 F：测试、平衡与优化（优先级：持续进行，~10h）
1. FactionManager 单元测试
2. Enemy AI 阵营行为测试
3. 经验曲线平衡
4. 伤害公式平衡
5. 难度曲线调整
6. 性能优化（地图渲染/Pathfinder）

---

## 四、关键架构决策

1. **阵营系统优先于一切**：所有 NPC/Enemy 都有 Faction 字段，AI 行为受阵营关系约束
2. **默认中立**：除匪徒↔平民外，所有阵营关系默认中立，通过玩家行为触发敌对
3. **对话驱动剧情**：对话树通过 effects 改变阵营关系、声望、任务状态
4. **测试优先**：每个功能必须有对应测试，避免回归（234 测试全部通过）

---

## 五、风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 阵营关系复杂度过高 | 中 | 简化为 3 种关系（盟友/中立/敌对） |
| 伪装系统被滥用 | 低 | 声望阈值 + 识破机制 |
| 内容创作工作量大 | 中 | 先完成核心地图，后续迭代 |
| 敌人 AI 行为异常 | 高 | 阵营测试覆盖所有场景 |