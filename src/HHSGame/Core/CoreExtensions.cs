
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

            // Setup Initial Parameters;
            serviceCollection.AddSingleton((_) => new Random());
            serviceCollection.AddSingleton((_) => new GameParameters { MapStyle = MapStyle.Cave, MapHeight = 500, MapWidth = 500 });

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

            // Setup GameContext
            serviceCollection.AddSingleton<GameContext>();
            serviceCollection.AddSingleton<GameWorld>();

            // Create Player  (which depends on GameWorld)
            serviceCollection.AddSingleton((provider) => provider.GetRequiredService<GameWorld>().NewPlayer(Classes.Classes.Warrior.ToClass()));

            // Setup Game
            serviceCollection.AddSingleton<Game>();

            return serviceCollection;
        }
    }
}
