using HHSGame.Core.Dialogue;
using HHSGame.Core.Quests;
using HHSGame.Core.Triggers;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Scripting
{
    /// <summary>
    /// Lua 脚本与游戏系统的集成层
    /// </summary>
    public sealed class ScriptIntegration
    {
        private readonly ILuaScriptEngine luaEngine;
        private readonly GameLuaAPI gameAPI;
        private readonly ILogger<ScriptIntegration> logger;

        public ScriptIntegration(ILuaScriptEngine luaEngine, GameLuaAPI gameAPI, ILogger<ScriptIntegration> logger)
        {
            this.luaEngine = luaEngine;
            this.gameAPI = gameAPI;
            this.logger = logger;

            // 注册游戏 API 到 Lua
            RegisterGameAPI();
        }

        /// <summary>
        /// 注册游戏 API 到 Lua 环境
        /// </summary>
        private void RegisterGameAPI()
        {
            // 注册 game 命名空间
            luaEngine.SetGlobal("game", gameAPI);

            // 注册常用函数（简化调用）
            luaEngine.SetGlobal("print", new Action<string>(msg => gameAPI.ShowMessage(msg)));
            luaEngine.SetGlobal("random", new Func<int, int, int>((min, max) => gameAPI.Random(min, max)));
        }

        /// <summary>
        /// 执行 Lua 条件脚本
        /// </summary>
        public bool EvaluateCondition(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return true; // 无条件则默认通过
            }

            try
            {
                // 包装为函数并执行
                var wrappedScript = $"return (function() {script} end)()";
                var result = luaEngine.Execute<bool>(wrappedScript);
                return result == true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error evaluating Lua condition: {Script}", script);
                return false;
            }
        }

        /// <summary>
        /// 执行 Lua 动作脚本
        /// </summary>
        public void ExecuteAction(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            try
            {
                luaEngine.Execute(script);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing Lua action: {Script}", script);
            }
        }

        // ── 对话系统集成 ──────────────────────────────────────────────

        /// <summary>
        /// 检查对话选项的 Lua 条件
        /// </summary>
        public bool CheckDialogueCondition(DialogueRequirement requirement)
        {
            // 如果有 Lua 条件脚本，执行它
            if (!string.IsNullOrWhiteSpace(requirement.LuaCondition))
            {
                return EvaluateCondition(requirement.LuaCondition);
            }

            // 否则使用内置条件检查
            return true; // 由 DialogueManager 处理内置条件
        }

        /// <summary>
        /// 执行对话效果的 Lua 脚本
        /// </summary>
        public void ExecuteDialogueEffect(DialogueEffect effect)
        {
            if (!string.IsNullOrWhiteSpace(effect.LuaScript))
            {
                ExecuteAction(effect.LuaScript);
            }
        }

        // ── 任务系统集成 ──────────────────────────────────────────────

        /// <summary>
        /// 检查任务目标的 Lua 条件
        /// </summary>
        public bool CheckQuestObjectiveCondition(string conditionScript)
        {
            return EvaluateCondition(conditionScript);
        }

        /// <summary>
        /// 执行任务奖励的 Lua 脚本
        /// </summary>
        public void ExecuteQuestReward(string rewardScript)
        {
            ExecuteAction(rewardScript);
        }

        // ── 触发器系统集成 ──────────────────────────────────────────────

        /// <summary>
        /// 检查触发器条件
        /// </summary>
        public bool CheckTriggerCondition(MapTrigger trigger)
        {
            return EvaluateCondition(trigger.ConditionScript);
        }

        /// <summary>
        /// 执行触发器动作
        /// </summary>
        public void ExecuteTriggerAction(MapTrigger trigger)
        {
            // 先执行内置动作
            if (trigger.BuiltInAction != BuiltInAction.None)
            {
                ExecuteBuiltInAction(trigger);
            }

            // 再执行 Lua 脚本
            if (!string.IsNullOrWhiteSpace(trigger.ActionScript))
            {
                ExecuteAction(trigger.ActionScript);
            }
        }

        private void ExecuteBuiltInAction(MapTrigger trigger)
        {
            switch (trigger.BuiltInAction)
            {
                case BuiltInAction.ShowMessage:
                    gameAPI.ShowMessage(trigger.ActionParameter ?? "");
                    break;
                case BuiltInAction.StartQuest:
                    gameAPI.StartQuest(trigger.ActionParameter ?? "");
                    break;
                case BuiltInAction.CompleteQuest:
                    gameAPI.CompleteQuest(trigger.ActionParameter ?? "");
                    break;
                case BuiltInAction.ModifyReputation:
                    // 格式: "faction:amount"
                    if (!string.IsNullOrEmpty(trigger.ActionParameter))
                    {
                        var parts = trigger.ActionParameter.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int amount))
                        {
                            gameAPI.ModifyReputation(parts[0], amount);
                        }
                    }
                    break;
                case BuiltInAction.Teleport:
                    // 格式: "x,y"
                    if (!string.IsNullOrEmpty(trigger.ActionParameter))
                    {
                        var parts = trigger.ActionParameter.Split(',');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                        {
                            // TODO: Implement teleport
                            logger.LogWarning("Teleport not yet implemented");
                        }
                    }
                    break;
                case BuiltInAction.ChangeMap:
                    // 格式: "mapId:x,y"
                    if (!string.IsNullOrEmpty(trigger.ActionParameter))
                    {
                        var parts = trigger.ActionParameter.Split(':');
                        if (parts.Length == 2)
                        {
                            var coords = parts[1].Split(',');
                            if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                            {
                                gameAPI.ChangeMap(parts[0], x, y);
                            }
                        }
                    }
                    break;
            }
        }

        // ── NPC 行为集成 ──────────────────────────────────────────────

        /// <summary>
        /// 执行 NPC 行为脚本
        /// </summary>
        public void ExecuteNPCBehavior(string npcId, string behaviorScript)
        {
            // 设置当前 NPC 上下文
            luaEngine.SetGlobal("currentNpcId", npcId);
            ExecuteAction(behaviorScript);
        }

        /// <summary>
        /// 检查 NPC 条件
        /// </summary>
        public bool CheckNPCCondition(string npcId, string conditionScript)
        {
            luaEngine.SetGlobal("currentNpcId", npcId);
            return EvaluateCondition(conditionScript);
        }

        // ── 加载脚本 ──────────────────────────────────────────────

        /// <summary>
        /// 加载脚本文件
        /// </summary>
        public void LoadScriptFile(string name, string code)
        {
            luaEngine.LoadScript(name, code);
        }

        /// <summary>
        /// 加载脚本文件列表
        /// </summary>
        public void LoadScriptFiles(IEnumerable<(string name, string code)> scripts)
        {
            foreach (var (name, code) in scripts)
            {
                LoadScriptFile(name, code);
            }
        }
    }
}
