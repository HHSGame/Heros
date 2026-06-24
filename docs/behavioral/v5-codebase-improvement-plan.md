# HHSGame 代码库改进计划 (v5)

> 基于 2026-06-05 全面代码审查与架构分析

## 一、Vision / 背景动机

项目已完成 WWII 重主题（~70%）和核心系统搭建（~85%），但存在显著的技术债务阻碍后续开发效率和代码质量。本次改进聚焦于：
1. 消除 God Class / God Object 反模式
2. 修复层级依赖违规（UI ↔ Core 耦合）
3. 补全工程基础设施（CI/CD、代码规范）
4. 统一代码风格和类型系统

## 二、Scope / Non-Goals

### Scope
- 架构层面的代码重构
- 工程基础设施补全
- 代码质量与一致性改进
- 测试覆盖率提升

### Non-Goals
- 不涉及新功能开发
- 不涉及游戏内容扩充
- 不涉及 UI 视觉设计变更
- 不涉及数据格式迁移（现有 JSON 配置保持兼容）

## 三、问题清单与改进计划

### P0: 架构级重构（阻塞性问题）

#### P0-1: GameUI God Class 拆分
**现状**: `GameUI.cs` 约 2200 行，61 个私有方法，承担了所有输入处理、窗口管理、目标选择、存档/读档、教程集成等职责。

**问题**:
- 违反单一职责原则，难以维护和测试
- `HandleKeyEvent` 方法超过 300 行，深层嵌套 if-else 链
- 任何 UI 变更都需要修改这个巨型类

**改进方案**:
1. 引入 `IInputHandler` 接口，每个 `GameStateType` 对应一个输入处理器
2. 提取 `TargetSelector`（移动/攻击/对话/技能目标选择）
3. 提取 `WindowManager`（窗口可见性管理、状态恢复）
4. 提取 `SaveLoadUI`（存档/读档对话框逻辑）
5. `GameUI` 精简为协调器，委托给上述组件

**验收标准**:
- `GameUI.cs` 行数 < 500
- 每个提取的组件有独立的单元测试
- 所有现有功能行为不变（回归测试通过）

---

#### P0-2: GameContext God Object 解耦
**现状**: `GameContext` 持有 20+ 个服务引用，作为全局服务定位器使用。

**问题**:
- 任何代码通过 `GameContext` 可访问任何服务，形成隐式耦合
- 无法确定子系统的真实依赖关系
- 测试时需要构造完整的 `GameContext`

**改进方案**:
1. 按职责分组：`CombatContext`、`WorldContext`、`UIContext`
2. 子系统只接收实际需要的依赖（构造函数注入）
3. `GameContext` 精简为 `GameSession`，只持有游戏会话级别的状态
4. 使用接口隔离原则定义服务契约

**验收标准**:
- `GameContext` 依赖字段 < 10 个
- 每个子系统只注入必要的依赖
- 所有现有测试通过

---

#### P0-3: 静态 Events 事件总线改造
**现状**: `Events` 类使用静态 `EventHandler` 委托，全局可订阅/发布。

**问题**:
- 静态事件是内存泄漏源（订阅者不取消订阅会一直存活）
- 隐式全局耦合，无法追踪数据流
- 不可测试（无法 mock 或隔离）

**改进方案**:
1. 引入 `IEventBus` 接口（注入式）
2. 使用弱引用或显式订阅/取消订阅机制
3. 按领域分组事件：`CombatEvents`、`WorldEvents`、`UIEvents`
4. 逐步迁移现有订阅者

**验收标准**:
- `Events` 类不再使用静态事件
- 所有事件订阅通过 DI 注入的 `IEventBus`
- 无内存泄漏（可通过测试验证）

---

### P1: 层级依赖修复

#### P1-1: UI 类型泄漏到 Core 层
**现状**:
- `SaveManager.cs` 和 `LoadManager.cs` 导入 `using HHSGame.UI;`（使用 `Cell` 类型）
- `EnemyCatalog.cs` 导入 `using HHSGame.UI;`（使用 `ColorPresets`）
- `Player.Attribute` 属性返回 `ColorPresets` 颜色值

**问题**:
- Core 层不应依赖 UI 层，违反依赖倒置原则
- 难以独立测试 Core 逻辑
- 阻碍未来 UI 框架替换

**改进方案**:
1. 将 `Cell` 结构体移动到 `Core/DrawingContext` 或新建 `Core/Rendering/` 命名空间
2. 将 `ColorPresets` 移动到 `Core/Rendering/` 或使用接口抽象
3. `Player` 的颜色属性改为通过 `IGameActor` 接口的 `GetDisplayColor()` 方法
4. 更新所有 using 引用

**验收标准**:
- `Core/` 目录下无 `using HHSGame.UI;` 引用
- 编译通过，测试全部通过

---

#### P1-2: Game 类职责精简
**现状**: `Game.cs` 约 804 行，混合了游戏逻辑与渲染逻辑。

**问题**:
- `RenderFrame()`、`AnimateStep()`、`DrawPlannedDestinations()` 是渲染职责
- 直接依赖 `IDrawingContext`、`GUISettings`、`ColorPresets`、`Application`

**改进方案**:
1. 提取 `GameRenderer` 类处理所有渲染相关逻辑
2. `Game` 只保留游戏状态管理和回合协调
3. 渲染通过事件或回调触发

**验收标准**:
- `Game.cs` 行数 < 500
- 渲染逻辑完全独立于游戏逻辑

---

### P2: 代码质量改进

#### P2-1: 坐标类型统一
**现状**: 同时使用 `(int X, int Y)` 元组和 `Coordinate` 记录类型。

**问题**:
- 类型不一致导致代码可读性差
- 需要额外的转换逻辑

**改进方案**:
1. 统一使用 `Coordinate` 记录类型
2. 移除 `(int, int)` 元组的重载方法
3. 提供 `Coordinate.FromTuple()` 转换方法（兼容性）

**验收标准**:
- `MapState` 无 `(int, int)` 重载
- 所有坐标操作使用 `Coordinate` 类型

---

#### P2-2: 魔法数字提取为命名常量
**现状**: 代码中大量硬编码数字，缺乏语义。

**示例**:
- `random.Next(1, 21)` — d20 骰子
- `roll == 20` — 暴击阈值
- `100 * Math.Pow(level, 1.5)` — 经验公式
- `sqDistance <= 64` — 检测范围（8²）
- `-100, 100` — 声望边界

**改进方案**:
1. 在 `Constant.cs` 中提取游戏机制常量
2. 在各模块中提取局部常量
3. 使用 `const` 或 `static readonly` 并添加注释

**验收标准**:
- 无裸露的魔法数字
- 所有常量有清晰的命名和注释

---

#### P2-3: 命名空间一致性
**现状**: `Core/Skills/` 目录下的文件使用 `HHSGame.Core.SkillActions` 命名空间，与目录名不匹配。

**改进方案**:
1. 统一为 `HHSGame.Core.Skills`（匹配目录名）
2. 更新所有引用

**验收标准**:
- 目录名与命名空间一致
- 编译通过

---

#### P2-4: 重复代码消除
**现状**:
- `SaveManager` 和 `LoadManager` 有完全相同的 `GetSlotPath` 和 `SlotExists` 方法
- `InteractableObjects.cs` 中 4 个类重复相同的属性模式

**改进方案**:
1. 提取 `SavePathHelper` 工具类
2. 提取 `InteractableBase` 抽象基类

**验收标准**:
- 无重复的工具方法
- 所有测试通过

---

#### P2-5: 异常处理改进
**现状**: Save/Load 系统捕获所有 `Exception`，静默失败。

**改进方案**:
1. 捕获具体异常类型（`IOException`、`UnauthorizedAccessException`、`JsonException`）
2. 使用自定义异常类型 `GameConfigurationException`
3. 向 UI 层传播可操作的错误信息

**验收标准**:
- 无 `catch (Exception ex)` 模式
- 用户可见的错误提示

---

### P3: 工程基础设施

#### P3-1: CI/CD 流水线
**现状**: 无自动化构建和测试。

**改进方案**:
1. 创建 `.github/workflows/dotnet.yml`
2. 配置：build → test → (可选) publish
3. PR 触发和 main 分支保护

**验收标准**:
- PR 自动运行 build 和 test
- 测试失败时阻止合并

---

#### P3-2: 代码格式化强制
**现状**: `.editorconfig` 只有基础缩进配置，无 C# 特定规则。

**改进方案**:
1. 扩展 `.editorconfig` 添加 C# 命名和风格规则
2. 配置 `dotnet format` 作为 CI 步骤
3. 考虑添加 StyleCop 分析器

**验收标准**:
- `.editorconfig` 包含完整的 C# 规则
- CI 自动检查代码格式

---

#### P3-3: 构建配置完善
**现状**: `Directory.Build.props` 为空，主项目缺少 `LangVersion`。

**改进方案**:
1. 在 `Directory.Build.props` 中定义共享属性（`TreatWarningsAsErrors`、`LangVersion`）
2. 主项目添加 `<LangVersion>latest</LangVersion>`
3. 考虑启用 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`

**验收标准**:
- 两个项目共享统一的构建配置
- 编译无警告

---

### P3.5: 测试项目修复（阻塞 CI）

#### P3.5-1: 测试编译错误修复
**现状**: 5 个测试文件编译失败，原因是 `DialogueManager` 构造函数新增了 `FactionManager` 参数，但测试未同步更新。

**失败文件**:
- `SkillActionTests.cs:100`
- `ConditionEvaluatorTests.cs:221`
- `DialogueManagerTests.cs:187`
- `GameScriptRunnerTests.cs:87`
- `GameIntegrationTests.cs:485`

**改进方案**:
1. 更新上述测试文件，传入 `FactionManager` mock 或实例
2. 建立构造函数变更的测试同步检查机制

**验收标准**:
- `dotnet test` 编译通过且全部测试通过
- 测试项目无编译警告

---

### P4: 小改进项

#### P4-1: 代码错误修正
- 修复 `Game.cs` 第 58 行拼写错误："Intializing" → "Initializing"

#### P4-2: Random 实例统一
- `Enemy.cs` 使用注入的 `Random` 实例替代 `new Random()`

#### P4-3: 硬编码中文字符串国际化
- `InteractableObjects.cs` 中的中文字符串通过 `I18n` 系统处理

#### P4-4: 文件作用域命名空间
- 逐步迁移所有文件使用 `namespace X;` 语法

---

## 四、实施顺序

```
Sprint 0 (立即): P3.5-1 测试编译错误修复（阻塞开发）
Sprint 1 (Week 1): P1-1 UI 层依赖修复 + P2-3 命名空间统一 + P4-1 拼写修复
Sprint 2 (Week 2): P0-1 GameUI 拆分（第一阶段：提取 TargetSelector）
Sprint 3 (Week 3): P0-1 GameUI 拆分（第二阶段：提取 InputHandler）
Sprint 4 (Week 4): P0-2 GameContext 解耦 + P1-2 Game 精简
Sprint 5 (Week 5): P0-3 Events 改造 + P2-1 坐标统一
Sprint 6 (Week 6): P2-2 魔法数字 + P2-4 重复代码 + P2-5 异常处理
Sprint 7 (Week 7): P3 工程基础设施 + P4 小改进项
```

## 五、风险与回退方案

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| GameUI 拆分引入回归 | 高 | 每步拆分后运行完整测试套件 |
| GameContext 解耦影响范围大 | 中 | 渐进式重构，保持向后兼容 |
| Events 改造需要大量迁移 | 中 | 提供适配器层，逐步迁移 |
| CI/CD 配置环境差异 | 低 | 使用 GitHub Actions 官方 .NET 模板 |

## 六、验收标准（全局）

1. 所有现有测试通过（234 个测试）
2. 编译无警告（启用 TreatWarningsAsErrors）
3. Core 层无 UI 依赖
4. 无 God Class（单文件 < 500 行）
5. CI 自动运行 build + test
6. 代码格式统一

## 七、追溯矩阵

| 改进项 | 影响模块 | 验证方式 |
|--------|----------|----------|
| P0-1 GameUI 拆分 | UI/GameUI.cs, UI/Views/ | 单元测试 + 回归测试 |
| P0-2 GameContext 解耦 | Core/GameContext.cs, Core/Game.cs | 单元测试 + 集成测试 |
| P0-3 Events 改造 | Core/Event.cs, 所有订阅者 | 单元测试 + 内存测试 |
| P1-1 UI 层依赖修复 | Core/Save/, Core/Engine/ | 编译检查 + 测试 |
| P2-1 坐标统一 | Core/Map/, Core/BaseType.cs | 编译检查 + 测试 |
| P3-1 CI/CD | .github/workflows/ | PR 触发验证 |
