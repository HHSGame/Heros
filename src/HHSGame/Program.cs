using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using HHSGame.UI;
using HHSGame.Core;

using Serilog;
using System.Globalization;
using Terminal.Gui.App;

namespace HHSGame
{
    sealed class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("hss.log", formatProvider: CultureInfo.CurrentCulture)
                .CreateLogger();
            StartTerminalGUI();
        }

        static void StartTerminalGUI()
        {
            Application.Init();

            var services = SetupServices();

            using (var gameUI = services.GetRequiredService<GameUI>())
            using (var top = gameUI.Start())
            {
                Application.Run(top);
                Application.Shutdown();
            }

            services.Dispose();
        }

        static ServiceProvider SetupServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(loggingBuiler =>
                loggingBuiler.AddSerilog(dispose: true));
            return serviceCollection
                .AddHHSGameCore()
                .AddHHSGameUI()
                .BuildServiceProvider();
        }
    }
}
