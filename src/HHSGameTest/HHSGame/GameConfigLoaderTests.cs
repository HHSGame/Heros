using System.Text;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Items;
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

            Assert.AreEqual("UrbanStreet", config.Map.Style);
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

            GameParameters parameters = CreateMapper().ToParameters(config);

            Assert.AreEqual(MapStyle.UrbanStreet, parameters.MapStyle);
            Assert.AreEqual(50, parameters.MapWidth);
            Assert.AreEqual(30, parameters.MapHeight);
            Assert.IsNotNull(parameters.PlayerClass);
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
                        Id = "Gangster",
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

            GameParameters parameters = CreateMapper().ToParameters(config);

            Assert.AreEqual(1, parameters.EnemySpawns.Count);
            Assert.AreEqual("Gangster", parameters.EnemySpawns[0].EnemyId);
            Assert.AreEqual(3, parameters.EnemySpawns[0].Position.X);
            Assert.AreEqual(4, parameters.EnemySpawns[0].Position.Y);
            Assert.AreEqual(1, parameters.EnemySpawns[0].Count);

            Assert.AreEqual(1, parameters.MapItems.Count);
            Assert.AreEqual("HealthPotion", parameters.MapItems[0].ItemId);
            Assert.AreEqual(6, parameters.MapItems[0].Position.X);
            Assert.AreEqual(7, parameters.MapItems[0].Position.Y);
            Assert.AreEqual(2, parameters.MapItems[0].Quantity);
        }

        [TestMethod]
        public void MapConfigToParametersMapsPlayers()
        {
            GameConfig config = new()
            {
                Map = new MapConfig(),
                Player = new PlayerConfig(),
                Players =
                [
                    new PlayerEntryConfig
                    {
                        Name = "Alpha",
                        Glyph = "A",
                        Class = "Warrior",
                        StartPosition = new CoordinateConfig { X = 1, Y = 2 },
                        StartingItems = ["HealthPotion"]
                    }
                ]
            };

            GameParameters parameters = CreateMapper().ToParameters(config);

            Assert.AreEqual(1, parameters.PlayerSpawns.Count);
            Assert.AreEqual("Alpha", parameters.PlayerSpawns[0].Name);
            Assert.AreEqual('A', parameters.PlayerSpawns[0].Glyph);
            Assert.AreEqual(1, parameters.PlayerSpawns[0].Position.X);
            Assert.AreEqual(2, parameters.PlayerSpawns[0].Position.Y);
            Assert.AreEqual(1, parameters.PlayerSpawns[0].StartingItems.Count);
        }

        private static GameConfigLoader CreateLoader()
        {
            GameDataLoader dataLoader = new(NullLogger<GameDataLoader>.Instance);
            return new GameConfigLoader(dataLoader);
        }

        private static GameConfigMapper CreateMapper()
        {
            ItemCatalog itemCatalog = new(
                new List<WeaponDefinition>
                {
                    new WeaponDefinition("UnknownWeapon", "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line)
                },
                new List<ArmorDefinition>
                {
                    new ArmorDefinition("UnknownArmor", "Unknown", ItemRarity.Common, 0, 0, 0)
                },
                new List<ItemDefinition>
                {
                    new ItemDefinition("HealthPotion", "HealthPotion", "Health Potion", ItemRarity.Common, 50, 0.5f, 10)
                });

            ClassCatalog classCatalog = new(
                new List<ClassDefinition>
                {
                    new ClassDefinition(
                        "Warrior",
                        "Warrior",
                        new Stats.Attributes { Strength = 7, Perception = 5, Agility = 5, Charisma = 4, Intelligence = 4 },
                        new Stats.Skills(),
                        "UnknownWeapon",
                        "UnknownArmor")
                },
                itemCatalog);

            EnemyCatalog enemyCatalog = new(new List<EnemyDefinition>
            {
                new EnemyDefinition(
                    "Gangster", "Gangster", "Neutral", new Stats.Attributes(),
                    new Stats.Skills(),
                    "UnknownWeapon",
                    "UnknownArmor",
                    0,
                    'g',
                    HHSGame.Core.Rendering.ColorPresets.Enemies.Occupier,
                    new List<EnemyLootEntry>(),
                    new List<EnemyAbilityDefinition>())
            });

            return new GameConfigMapper(new GameCatalog(itemCatalog, classCatalog, enemyCatalog));
        }


        private static string WriteTempConfig(string json)
        {
            string path = Path.Combine(Path.GetTempPath(), $"hhs_config_{Guid.NewGuid():N}.json");
            File.WriteAllText(path, json, Encoding.ASCII);
            return path;
        }
    }
}
