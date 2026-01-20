using System.Text;
using ClassCatalog = HHSGame.Core.Classes.Classes;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Map;
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

        [TestMethod]
        public void LoadGameConfigThrowsWhenEnemyPositionMissing()
        {
            string json = "{\"map\":{\"width\":10,\"height\":10},\"player\":{},\"enemies\":[{\"id\":\"Gangster\"}]}";
            string path = WriteTempConfig(json);

            GameConfigLoader loader = CreateLoader();
            Assert.ThrowsException<InvalidDataException>(() => loader.Load(path));
        }

        [TestMethod]
        public void MapConfigToParametersUsesDefaults()
        {
            GameConfig config = new()
            {
                Map = new MapConfig(),
                Player = new PlayerConfig()
            };

            GameParameters parameters = GameConfigMapper.ToParameters(config);

            Assert.AreEqual(MapStyle.Cave, parameters.MapStyle);
            Assert.AreEqual(50, parameters.MapWidth);
            Assert.AreEqual(30, parameters.MapHeight);
            Assert.AreEqual(ClassCatalog.Warrior, parameters.PlayerClass);
        }

        [TestMethod]
        public void MapConfigToParametersMapsSpawns()
        {
            GameConfig config = new()
            {
                Map = new MapConfig(),
                Player = new PlayerConfig(),
                Enemies =
                [
                    new EnemySpawnConfig
                    {
                        Id = EnemyType.Gangster.ToString(),
                        Position = new CoordinateConfig { X = 3, Y = 4 },
                        Count = 1
                    }
                ],
                Items =
                [
                    new ItemConfig
                    {
                        Id = "HealthPotion",
                        Position = new CoordinateConfig { X = 6, Y = 7 },
                        Quantity = 2
                    }
                ]
            };

            GameParameters parameters = GameConfigMapper.ToParameters(config);

            Assert.AreEqual(1, parameters.EnemySpawns.Count);
            Assert.AreEqual(EnemyType.Gangster, parameters.EnemySpawns[0].Type);
            Assert.AreEqual(3, parameters.EnemySpawns[0].Position.X);
            Assert.AreEqual(4, parameters.EnemySpawns[0].Position.Y);
            Assert.AreEqual(1, parameters.EnemySpawns[0].Count);

            Assert.AreEqual(1, parameters.MapItems.Count);
            Assert.AreEqual("HealthPotion", parameters.MapItems[0].ItemId);
            Assert.AreEqual(6, parameters.MapItems[0].Position.X);
            Assert.AreEqual(7, parameters.MapItems[0].Position.Y);
            Assert.AreEqual(2, parameters.MapItems[0].Quantity);
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
