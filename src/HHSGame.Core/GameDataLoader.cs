using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core
{
    public partial class GameDataLoader(ILogger<GameDataLoader> logger)
    {

        private JsonSerializerOptions Options { get; set; } = new(JsonSerializerDefaults.Web) { };

        public T? LoadData<T>(string path)
        {
            if (!Path.Exists(path))
            {
                throw new FileNotFoundException($"File cannot be found for type {typeof(T).Name}",
                    path);
            }

            using FileStream openStream = File.OpenRead(path);
            LogLoading(logger, typeof(T).Name, path);
            T? result = JsonSerializer.Deserialize<T>(openStream, Options);
            return result;
        }

        public byte[] LoadBytes(string path)
        {
            if (!Path.Exists(path))
            {
                throw new FileNotFoundException($"File cannot be found when reading bytes",
                    path);
            }

            byte[] result = File.ReadAllBytes(path);
            LogLoading(logger, "byte array", path);

            return result;
        }

        [LoggerMessage(LogLevel.Information, "Loading {type} from {path}")]
        static partial void LogLoading(ILogger logger, string type, string path);

    }

}