using Microsoft.Extensions.DependencyInjection;
using HHSGame.UI;
using HHSGame.Core;
using HHSGame.Core.Engine;

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

    ServiceProvider services = SetupServices(args);

    using (GameUI gameUI = services.GetRequiredService<GameUI>())
    using (Terminal.Gui.Views.Toplevel top = gameUI.Start())
    {
        Application.Run(top);
        Application.Shutdown();
    }

    services.Dispose();
}

ServiceProvider SetupServices(string[] args)
{
    GameParameters parameters = LoadGameParameters(args);
    ServiceCollection serviceCollection = new();
    serviceCollection.AddLogging(loggingBuiler =>
        loggingBuiler.AddSerilog(dispose: true));
    return serviceCollection
        .AddHHSGameCore(parameters)
        .AddHHSGameUI()
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
