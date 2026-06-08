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

            foreach (EnemySpawnConfig enemy in config.Enemies)
            {
                if (string.IsNullOrWhiteSpace(enemy.Id))
                {
                    throw new InvalidDataException($"Enemy id is required in {path}.");
                }

                if (enemy.Position == null)
                {
                    throw new InvalidDataException($"Enemy position is required for {enemy.Id} in {path}.");
                }
            }

            foreach (ItemConfig item in config.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    throw new InvalidDataException($"Item id is required in {path}.");
                }

                if (item.Position == null)
                {
                    throw new InvalidDataException($"Item position is required for {item.Id} in {path}.");
                }
            }

            foreach (PlayerEntryConfig player in config.Players)
            {
                if (player.StartPosition == null)
                {
                    string name = string.IsNullOrWhiteSpace(player.Name) ? "player" : player.Name;
                    throw new InvalidDataException($"StartPosition is required for {name} in {path}.");
                }
            }

            foreach (NpcConfig npc in config.Npcs)
            {
                if (string.IsNullOrWhiteSpace(npc.Id))
                {
                    throw new InvalidDataException($"Npc id is required in {path}.");
                }

                if (npc.Position == null)
                {
                    throw new InvalidDataException($"Npc position is required for {npc.Id} in {path}.");
                }
            }
        }
    }
}
