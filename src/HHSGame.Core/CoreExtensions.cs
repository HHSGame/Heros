
using Microsoft.Extensions.DependencyInjection;
using HHSGame.Core.Combat;
using HHSGame.Core.CharacterCreation;
using HHSGame.Core.Factions;
using HHSGame.Core.Interactions;
using HHSGame.Core.Map;
using HHSGame.Core.Scripting;
using HHSGame.Core.Tutorial;
using HHSGame.Core.Triggers;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Save;

namespace HHSGame.Core
{
    public static class CoreExtensions
    {

        public static IServiceCollection AddHHSGameCore(this IServiceCollection serviceCollection, GameParameters parameters, Engine.GameCatalog catalogs)
        {
            // Setup Initial Parameters;
            serviceCollection.AddSingleton<IEventBus, EventBus>();
            serviceCollection.AddSingleton<CombatContext>();
            serviceCollection.AddSingleton<WorldContext>();
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
            serviceCollection.AddSingleton<NpcFactory>();
            serviceCollection.AddSingleton<NpcManager>();
            serviceCollection.AddSingleton<SurroundingsManager>();
            serviceCollection.AddSingleton<InventoryManager>();
            serviceCollection.AddSingleton<InteractableManager>();
            serviceCollection.AddSingleton<TutorialManager>();
            serviceCollection.AddSingleton<CharacterCreationManager>();
            serviceCollection.AddSingleton<FactionManager>();
            serviceCollection.AddSingleton<TurnManager>();
            serviceCollection.AddSingleton<GameStateMachine>();
            serviceCollection.AddSingleton<PartyState>();
            serviceCollection.AddSingleton<QuestManager>();
            serviceCollection.AddSingleton<DialogueManager>();

            // Triggers
            serviceCollection.AddSingleton<TriggerManager>();

            // Scripting
            serviceCollection.AddSingleton<ILuaScriptEngine, LuaScriptEngine>();
            serviceCollection.AddSingleton<GameLuaAPI>();
            serviceCollection.AddSingleton<ScriptIntegration>();

            // Save/Load
            serviceCollection.AddSingleton<SavePathHelper>();
            serviceCollection.AddSingleton<SaveManager>();
            serviceCollection.AddSingleton<LoadManager>();

            // Setup GameContext
            serviceCollection.AddSingleton<GameContext>();
            serviceCollection.AddSingleton<GameWorld>();

            // Setup Game
            serviceCollection.AddSingleton<Game>();

            return serviceCollection;
        }
    }
}
