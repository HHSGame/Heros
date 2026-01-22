
using Microsoft.Extensions.DependencyInjection;
using HHSGame.Core.Combat;
using HHSGame.Core.Map;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;

namespace HHSGame.Core
{
    public static class CoreExtensions
    {

        public static IServiceCollection AddHHSGameCore(this IServiceCollection serviceCollection, GameParameters parameters, Engine.GameCatalog catalogs)
        {
            // Setup Initial Parameters;
            serviceCollection.AddSingleton((_) => new Random());
            serviceCollection.AddSingleton(parameters);
            serviceCollection.AddSingleton(catalogs);
            serviceCollection.AddSingleton(catalogs.ItemCatalog);
            serviceCollection.AddSingleton(catalogs.ClassCatalog);
            serviceCollection.AddSingleton(catalogs.EnemyCatalog);

            serviceCollection.AddSingleton<EnemyFactory>();

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
