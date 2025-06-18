
using Microsoft.Extensions.DependencyInjection;
using HHSGame.UI.Views;
using HHSGame.UI.Windows;

namespace HHSGame.UI
{
    public static class UIExtensions
    {
        public static IServiceCollection AddHHSGameUI(this IServiceCollection serviceCollection)
        {
            // Setup UI Basics
            serviceCollection.AddSingleton<IDrawingContext, MapViewDrawingContext>();

            // Setup Views
            serviceCollection.AddSingleton<MapView>();
            serviceCollection.AddSingleton<InventoryListView>();
            serviceCollection.AddSingleton<EventLoggerView>();
            serviceCollection.AddSingleton<SurroundingsListView>();

            // Setup Windows
            serviceCollection.AddSingleton<MapWindow>();
            serviceCollection.AddSingleton<MessageWindow>();
            serviceCollection.AddSingleton<InventoryWindow>();
            serviceCollection.AddSingleton<SurroundingsWindow>();
            serviceCollection.AddSingleton<UtilityWindow>();

            // Setup Game UI
            serviceCollection.AddSingleton<GameUI>();

            return serviceCollection;
        }
    }
}