using HHSGame.Core.Engine.Config;

namespace HHSGame.Core.Engine
{
    public sealed class GameConfigLoader(GameDataLoader dataLoader)
    {
        public const string DefaultConfigPath = "data/game.json";

        public GameConfig Load(string? path = null)
        {
            string resolvedPath = string.IsNullOrWhiteSpace(path) ? DefaultConfigPath : path;
            GameConfig? config = dataLoader.LoadData<GameConfig>(resolvedPath);
            if (config == null)
            {
                throw new InvalidDataException($"Game config is empty: {resolvedPath}");
            }

            Validate(config, resolvedPath);
            return config;
        }

        private static void Validate(GameConfig config, string path)
        {
            if (config.Map.Width <= 0 || config.Map.Height <= 0)
            {
                throw new InvalidDataException($"Map size must be positive in {path}.");
            }

            if (config.Map.UseCustomMap && string.IsNullOrWhiteSpace(config.Map.CustomMapPath))
            {
                throw new InvalidDataException($"CustomMapPath is required when UseCustomMap is true in {path}.");
            }
        }
    }
}
