# v3 — Game.cs Refactoring (Game.cs 拆分)

## 1. Vision / 背景动机

Game.cs 当前 1473 行，承担了太多职责：玩家管理、战斗规划、回合执行、条件检查、渲染、技能动作执行、地图/敌人/物品初始化。这使得代码难以理解、测试和维护。

## 2. Scope / Non-Goals

### Scope
- 将 Game.cs 拆分为职责单一的组件
- 保留 Game 作为外观类 (Facade)，委托给子组件
- 所有现有测试必须继续通过
- 不改变外部行为（纯重构）

### Non-Goals
- 不改变 GameContext 或 GameStateMachine
- 不改变 UI 层
- 不添加新功能

## 3. 拆分计划

### 3.1 SkillActionExecutor (~250 行)
提取所有技能动作执行逻辑：
- ExecuteSkillAction (switch)
- ExecuteInspect, ExecuteSearch, ExecuteSneak, ExecutePickpocket
- ExecuteInspire, ExecuteShove, ExecuteLeap, ExecuteForage
- ExecuteAim, ExecuteSteadyMind, ExecutePickLock
- ExecuteAppraise, ExecuteDemoralize, ExecuteTreatWounds, ExecuteBless
- GetEffectiveSkillValue, IsAdjacent (skill-specific helper)

### 3.2 ConditionEvaluator (~120 行)
提取胜负条件检查逻辑：
- CheckGameConditions
- IsConditionMet (2 个重载)
- BuildConditions, ParseCondition
- ConditionKind enum, ConditionSpec record
- BuildMarkerCondition, BuildItemCondition
- HasItem, AnyPlayerDead, IsAtMarker

### 3.3 GameInitializer (~100 行)
提取游戏初始化逻辑：
- Start (拆分后的主要初始化流程)
- CreatePlayersFromSpawns
- AddStartingItems, AddMapItems

### 3.4 保留在 Game.cs 中 (~1000 行)
紧耦合的状态管理保留在 Game 中：
- PerformPlayerAction, TryMovePlayer
- TryQueuePlayerAction, TryQueuePlayerMove, ClearPlayerActions
- GetRemainingPlannedAp, GetPlannedApCost, GetMovePath, CalculateMovementApCost
- CommitPlayerActions, EndPlayerTurn, ExecutePlannedPlayerActions
- RenderFrame, DrawPlannedDestinations, AnimateStep
- SwitchControlledPlayer, UpdateActivePlayer
- ToggleCombatMode, IsCombatActive, HasVisibleEnemies, UpdateCombatState
- SetOverlayCells, ClearOverlayCells
- GetPlannedActionDescriptions
- TryStartDialogue, EndDialogue, Stop
- 其他辅助方法

## 4. 验收标准

| # | 验收条件 | 验证方式 |
|---|---------|---------|
| AC-1 | Game.cs 行数减少到 < 1000 行 | 代码检查 |
| AC-2 | 所有现有测试通过 (101 pass) | dotnet test |
| AC-3 | 新提取的类有单元测试 | dotnet test |
| AC-4 | Game 编译通过，无编译错误 | dotnet build |
| AC-5 | 外部行为不变（纯重构） | 手动验证 |

## 5. 实施顺序

1. 提取 SkillActionExecutor
2. 提取 ConditionEvaluator  
3. 提取 GameInitializer
4. 为新类写单元测试
5. 验证所有测试通过