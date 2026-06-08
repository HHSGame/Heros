using HHSGame.Core.Engine.Scripting;
using HHSGame.Core.Rendering;
using HHSGame.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Terminal.Gui.App;

namespace HHSGame.Core.Engine
{
    public sealed class GameHostOptions
    {
        public bool SkipWizard { get; init; }
        public ScriptedRunOptions? ScriptedRun { get; init; }
        public Action<ILoggingBuilder>? ConfigureLogging { get; init; }
    }

    public sealed record ScriptedRunOptions(string ScriptPath, int StepDelayMs);

    public sealed class GameEngineHost(GameEngine engine)
    {
        public void Run(GameHostOptions options)
        {
            Application.Init();

            ServiceProvider services = engine.BuildServiceProvider(
                new GameUiOptions { SkipWizard = options.SkipWizard },
                options.ConfigureLogging);

            using (GameUI gameUI = services.GetRequiredService<GameUI>())
            using (Terminal.Gui.Views.Toplevel top = gameUI.Start())
            {
                if (options.ScriptedRun != null)
                {
                    StartScriptedRun(services, options.ScriptedRun);
                }

                Application.Run(top);
                Application.Shutdown();
            }

            services.Dispose();
        }

        private static void StartScriptedRun(ServiceProvider services, ScriptedRunOptions options)
        {
            Game game = services.GetRequiredService<Game>();
            GameDataLoader dataLoader = new(NullLogger<GameDataLoader>.Instance);
            GameScriptLoader scriptLoader = new(dataLoader);
            GameScript script = scriptLoader.Load(options.ScriptPath);
            GameScriptRunner runner = new(script, new GameScriptInputAdapter(game));

            Application.AddTimeout(TimeSpan.FromMilliseconds(options.StepDelayMs), () =>
            {
                ScriptRunnerStatus status = runner.Step();
                if (status == ScriptRunnerStatus.Running)
                {
                    return true;
                }

                if (status == ScriptRunnerStatus.Failed)
                {
                    Events.RaiseGameMessage(runner.FailureReason ?? "Script failed.");
                }

                Application.Shutdown();
                return false;
            });
        }
    }
}
