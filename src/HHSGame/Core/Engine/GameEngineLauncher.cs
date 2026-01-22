using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HHSGame.Core.Engine
{
    public sealed class GameEngineLauncher
    {
        public static void Run(string[] args, Action<ILoggingBuilder>? configureLogging = null)
        {
            LaunchOptions options = ParseArgs(args);
            GameEngine engine = LoadEngine(options.ConfigPath);
            GameEngineHost host = new(engine);
            host.Run(new GameHostOptions
            {
                SkipWizard = options.SkipWizard,
                ScriptedRun = options.ScriptedRun,
                ConfigureLogging = configureLogging
            });
        }

        private static GameEngine LoadEngine(string? configPath)
        {
            GameDataLoader dataLoader = new(NullLogger<GameDataLoader>.Instance);
            GameConfigLoader configLoader = new(dataLoader);
            GameCatalogLoader catalogLoader = new(dataLoader);
            GameEngineFactory factory = new(configLoader, catalogLoader);
            return factory.Create(configPath);
        }

        private static LaunchOptions ParseArgs(string[] args)
        {
            string? configPath = GetArgValue(args, "--config");
            ScriptedRunOptions? scriptOptions = GetScriptOptions(args);
            bool skipWizard = scriptOptions != null || !string.IsNullOrWhiteSpace(configPath);
            return new LaunchOptions(configPath, scriptOptions, skipWizard);
        }

        private static string? GetArgValue(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private static ScriptedRunOptions? GetScriptOptions(string[] args)
        {
            string? scriptPath = GetArgValue(args, "--test-script");
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                return null;
            }

            int stepDelayMs = 100;
            string? delayValue = GetArgValue(args, "--test-step-ms");
            if (int.TryParse(delayValue, out int parsed) && parsed > 0)
            {
                stepDelayMs = parsed;
            }

            return new ScriptedRunOptions(scriptPath, stepDelayMs);
        }

        private sealed record LaunchOptions(
            string? ConfigPath,
            ScriptedRunOptions? ScriptedRun,
            bool SkipWizard);
    }
}
