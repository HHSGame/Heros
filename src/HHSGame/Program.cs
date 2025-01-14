using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using HHSGame.UI;
using HHSGame.Core;

namespace HHSGame
{
    sealed class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            var services = SetupServices();

            using (var gameUI = services.GetRequiredService<GameUI>())
            using (var top = gameUI.Start())
            {
                Application.Run(top);
                Application.Shutdown();
            }
        }


        static ServiceProvider SetupServices()
        {
            var serviceCollection = new ServiceCollection();

            return serviceCollection
                .AddHHSGameCore()
                .AddHHSGameUI()
                .BuildServiceProvider();
        }
    }
}
