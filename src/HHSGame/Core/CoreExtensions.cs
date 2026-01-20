
using Microsoft.Extensions.DependencyInjection;
using HHSGame.Core.Combat;
using HHSGame.Core.Map;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;

namespace HHSGame.Core
{
    public static class CoreExtensions
    {

        public static IServiceCollection AddHHSGameCore(this IServiceCollection serviceCollection)
        {
            return AddHHSGameCore(serviceCollection, new GameParameters { MapStyle = MapStyle.Cave, MapHeight = 500, MapWidth = 500 });
        }

        public static IServiceCollection AddHHSGameCore(this IServiceCollection serviceCollection, GameParameters parameters)
        {
            // Setup Initial Parameters;
            serviceCollection.AddSingleton((_) => new Random());
            serviceCollection.AddSingleton(parameters);

            // Setup Generators
            serviceCollection.AddSingleton<MapGenerator>();
            serviceCollection.AddSingleton<EnemyFactory>();
            serviceCollection.AddSingleton<ItemFactory>();

            // Setup Core System
            serviceCollection.AddSingleton<CollisionSystem>();
            serviceCollection.AddSingleton<Pathfinder>();
            serviceCollection.AddSingleton<MapState>();
            serviceCollection.AddSingleton<EnemyManager>();
            serviceCollection.AddSingleton<ItemManager>();
            serviceCollection.AddSingleton<SurroundingsManager>();
            serviceCollection.AddSingleton<InventoryManager>();
            serviceCollection.AddSingleton<TurnManager>();
            serviceCollection.AddSingleton<GameStateMachine>();

            // Setup GameContext
            serviceCollection.AddSingleton<GameContext>();
            serviceCollection.AddSingleton<GameWorld>();

            // Setup Game
            serviceCollection.AddSingleton<Game>();

            return serviceCollection;
        }
    }
}
