using HHSGame.Core;
using HHSGame.Core.Engine.Config;
using HHSGame.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Engine
{
    public sealed class GameEngineFactory(GameConfigLoader loader, GameCatalogLoader catalogLoader)
    {
        public GameEngine Create(string? configPath = null)
        {
            GameConfig config = loader.Load(configPath);
            GameCatalog catalogs = catalogLoader.Load(config.Catalogs);
            GameParameters parameters = new GameConfigMapper(catalogs).ToParameters(config);
            return new GameEngine(config, parameters, catalogs);
        }
    }

    public sealed class GameEngine(GameConfig config, GameParameters parameters, GameCatalog catalogs)
    {
        public GameConfig Config { get; } = config;
        public GameParameters Parameters { get; } = parameters;
        public GameCatalog Catalogs { get; } = catalogs;

        public void ConfigureServices(IServiceCollection services, GameUiOptions uiOptions)
        {
            services.AddSingleton(this);
            services.AddSingleton(Config);
            services
                .AddHHSGameCore(Parameters, Catalogs)
                .AddHHSGameUI(uiOptions);
        }

        public ServiceProvider BuildServiceProvider(GameUiOptions uiOptions, Action<ILoggingBuilder>? configureLogging = null)
        {
            ServiceCollection services = new();
            if (configureLogging != null)
            {
                services.AddLogging(configureLogging);
            }
            ConfigureServices(services, uiOptions);
            return services.BuildServiceProvider();
        }
    }
}
