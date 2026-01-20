using HHSGame.Core.Engine.Config;

namespace HHSGame.Core.Engine
{
    public sealed class GameEngineFactory(GameConfigLoader loader)
    {
        public GameEngine Create(string? configPath = null)
        {
            GameConfig config = loader.Load(configPath);
            GameParameters parameters = GameConfigMapper.ToParameters(config);
            return new GameEngine(config, parameters);
        }
    }

    public sealed class GameEngine(GameConfig config, GameParameters parameters)
    {
        public GameConfig Config { get; } = config;
        public GameParameters Parameters { get; } = parameters;
    }
}
