using System.Text;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Config;
using Microsoft.Extensions.Logging.Abstractions;

namespace HHSGame.Core
{
    [TestClass]
    public class GameConfigLoaderTests
    {
        [TestMethod]
        public void LoadGameConfigAppliesDefaults()
        {
            string json = "{\"map\":{\"width\":20,\"height\":10},\"player\":{}}";
            string path = WriteTempConfig(json);

            GameConfigLoader loader = CreateLoader();
            GameConfig config = loader.Load(path);

            Assert.AreEqual("Cave", config.Map.Style);
            Assert.AreEqual(20, config.Map.Width);
            Assert.AreEqual(10, config.Map.Height);
            Assert.IsTrue(config.Conditions.Lose.Contains("PlayerDeath"));
        }

        [TestMethod]
        public void LoadGameConfigThrowsWhenCustomMapPathMissing()
        {
            string json = "{\"map\":{\"useCustomMap\":true},\"player\":{}}";
            string path = WriteTempConfig(json);

            GameConfigLoader loader = CreateLoader();
            Assert.ThrowsException<InvalidDataException>(() => loader.Load(path));
        }

        private static GameConfigLoader CreateLoader()
        {
            GameDataLoader dataLoader = new(NullLogger<GameDataLoader>.Instance);
            return new GameConfigLoader(dataLoader);
        }

        private static string WriteTempConfig(string json)
        {
            string path = Path.Combine(Path.GetTempPath(), $"hhs_config_{Guid.NewGuid():N}.json");
            File.WriteAllText(path, json, Encoding.ASCII);
            return path;
        }
    }
}
