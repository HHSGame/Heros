
using Microsoft.Extensions.DependencyInjection;
using HHSGame.UI.Views;

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
            serviceCollection.AddSingleton<EventLoggerView>();

            // Setup Windows
            serviceCollection.AddSingleton<MapFrame>();
            serviceCollection.AddSingleton<MessageFrame>();
            serviceCollection.AddSingleton<InventoryFrame>();
            serviceCollection.AddSingleton<SurroundingsFrame>();
            serviceCollection.AddSingleton<UtilityWindow>();

            // Setup Game UI
            serviceCollection.AddSingleton<GameUI>();

            return serviceCollection;
        }
    }
}