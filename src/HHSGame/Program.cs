using Microsoft.Extensions.DependencyInjection;
using HHSGame.UI;
using HHSGame.Core;

using Serilog;
using System.Globalization;
using Terminal.Gui.App;

Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("hss.log", formatProvider: CultureInfo.CurrentCulture)
                .CreateLogger();
StartTerminalGUI();

void StartTerminalGUI()
{
    Application.Init();

    ServiceProvider services = SetupServices();

    using (GameUI gameUI = services.GetRequiredService<GameUI>())
    using (Terminal.Gui.Views.Toplevel top = gameUI.Start())
    {
        Application.Run(top);
        Application.Shutdown();
    }

    services.Dispose();
}

ServiceProvider SetupServices()
{
    ServiceCollection serviceCollection = new();
    serviceCollection.AddLogging(loggingBuiler =>
        loggingBuiler.AddSerilog(dispose: true));
    return serviceCollection
        .AddHHSGameCore()
        .AddHHSGameUI()
        .BuildServiceProvider();
}
