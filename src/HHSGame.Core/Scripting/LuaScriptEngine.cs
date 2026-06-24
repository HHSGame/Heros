using Lua;
using Lua.Standard;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Scripting
{
    /// <summary>
    /// 基于 LuaCSharp 的 Lua 脚本引擎实现
    /// </summary>
    public sealed class LuaScriptEngine : ILuaScriptEngine
    {
        private readonly ILogger<LuaScriptEngine> logger;
        private readonly LuaState state;
        private bool disposed;

        public LuaScriptEngine(ILogger<LuaScriptEngine> logger)
        {
            this.logger = logger;

            // 创建 Lua 状态
            state = LuaState.Create();

            // 加载标准库
            state.OpenStandardLibraries();
        }

        public void LoadScript(string name, string code)
        {
            try
            {
                RunAsync(state.DoStringAsync(code));
                logger.LogDebug("Loaded Lua script: {Name}", name);
            }
            catch (LuaCompileException ex)
            {
                logger.LogError(ex, "Syntax error in Lua script {Name}: {Message}", name, ex.Message);
                throw;
            }
            catch (LuaRuntimeException ex)
            {
                logger.LogError(ex, "Runtime error in Lua script {Name}: {Message}", name, ex.Message);
                throw;
            }
        }

        public void Execute(string script)
        {
            try
            {
                RunAsync(state.DoStringAsync(script));
            }
            catch (LuaRuntimeException ex)
            {
                logger.LogError(ex, "Lua runtime error: {Message}", ex.Message);
                throw;
            }
        }

        public T? Execute<T>(string script)
        {
            try
            {
                var results = RunAsync(state.DoStringAsync(script));
                if (results != null && results.Length > 0)
                {
                    return results[0].Read<T>();
                }
                return default;
            }
            catch (LuaRuntimeException ex)
            {
                logger.LogError(ex, "Lua runtime error: {Message}", ex.Message);
                throw;
            }
        }

        public T? CallFunction<T>(string functionName, params object[] args)
        {
            try
            {
                // 构建函数调用字符串
                var argsStr = string.Join(", ", args.Select(a => FormatLuaArg(a)));
                var script = $"return {functionName}({argsStr})";

                var results = RunAsync(state.DoStringAsync(script));
                if (results != null && results.Length > 0)
                {
                    return results[0].Read<T>();
                }
                return default;
            }
            catch (LuaRuntimeException ex)
            {
                logger.LogError(ex, "Lua error calling {FunctionName}: {Message}", functionName, ex.Message);
                throw;
            }
        }

        private static string FormatLuaArg(object arg)
        {
            return arg switch
            {
                string s => $"\"{s}\"",
                bool b => b ? "true" : "false",
                int or long or float or double => arg.ToString(),
                _ => $"\"{arg}\""
            };
        }

        public void SetGlobal(string name, object value)
        {
            state.Environment[name] = LuaValue.FromObject(value);
        }

        public T? GetGlobal<T>(string name)
        {
            var value = state.Environment[name];
            if (value.Type == LuaValueType.Nil)
            {
                return default;
            }
            return value.Read<T>();
        }

        public bool ValidateScript(string script, out string? error)
        {
            try
            {
                RunAsync(state.DoStringAsync($"return function() {script} end"));
                error = null;
                return true;
            }
            catch (LuaCompileException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public void Clear()
        {
            state.Environment.Clear();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
        }

        /// <summary>
        /// 同步执行异步方法
        /// </summary>
        private static T RunAsync<T>(ValueTask<T> task)
        {
            return task.AsTask().GetAwaiter().GetResult();
        }
    }
}
