using Microsoft.Extensions.DependencyInjection;
using HHSGame.UI;
using HHSGame.Core;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Scripting;

using Serilog;
using System.Globalization;
using Terminal.Gui.App;
using Microsoft.Extensions.Logging.Abstractions;

Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("hss.log", formatProvider: CultureInfo.CurrentCulture)
                .CreateLogger();
StartTerminalGUI(args);

void StartTerminalGUI(string[] args)
{
    Application.Init();

    ScriptedRunOptions? scriptOptions = GetScriptOptions(args);
    ServiceProvider services = SetupServices(args, scriptOptions != null);

    using (GameUI gameUI = services.GetRequiredService<GameUI>())
    using (Terminal.Gui.Views.Toplevel top = gameUI.Start())
    {
        if (scriptOptions != null)
        {
            StartScriptedRun(services, scriptOptions);
        }

        Application.Run(top);
        Application.Shutdown();
    }

    services.Dispose();
}

ServiceProvider SetupServices(string[] args, bool skipWizard)
{
    GameParameters parameters = LoadGameParameters(args);
    ServiceCollection serviceCollection = new();
    serviceCollection.AddLogging(loggingBuiler =>
        loggingBuiler.AddSerilog(dispose: true));
    return serviceCollection
        .AddHHSGameCore(parameters)
        .AddHHSGameUI(new GameUiOptions { SkipWizard = skipWizard })
        .BuildServiceProvider();
}

static GameParameters LoadGameParameters(string[] args)
{
    string? configPath = GetArgValue(args, "--config");
    GameDataLoader dataLoader = new(NullLogger<GameDataLoader>.Instance);
    GameConfigLoader configLoader = new(dataLoader);
    GameEngineFactory factory = new(configLoader);
    GameEngine engine = factory.Create(configPath);
    return engine.Parameters;
}

static string? GetArgValue(string[] args, string name)
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

static ScriptedRunOptions? GetScriptOptions(string[] args)
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

static void StartScriptedRun(ServiceProvider services, ScriptedRunOptions options)
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

sealed record ScriptedRunOptions(string ScriptPath, int StepDelayMs);
