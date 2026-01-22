using System.Text;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using Microsoft.Extensions.Logging.Abstractions;

namespace HHSGame.Core
{
    [TestClass]
    public class GameCatalogLoaderTests
    {
        [TestMethod]
        public void LoadCatalogParsesEnemyAbilities()
        {
            string weaponsPath = WriteTempFile("{\"weapons\":[{\"id\":\"UnknownWeapon\",\"name\":\"Unknown\",\"rarity\":\"Common\",\"value\":0,\"weight\":0,\"damage\":0,\"penetration\":0,\"range\":1,\"weaponType\":\"MeleeLight\"}]}");
            string armorsPath = WriteTempFile("{\"armors\":[{\"id\":\"UnknownArmor\",\"name\":\"Unknown\",\"rarity\":\"Common\",\"value\":0,\"weight\":0,\"armorValue\":0}]}");
            string itemsPath = WriteTempFile("{\"items\":[]}");
            string classesPath = WriteTempFile("{\"classes\":[]}");
            string enemiesPath = WriteTempFile("{\"enemies\":[{\"id\":\"TestEnemy\",\"weaponId\":\"UnknownWeapon\",\"armorId\":\"UnknownArmor\",\"abilities\":[{\"kind\":\"Poison\",\"chance\":100,\"duration\":2,\"amount\":3}]}]}");

            CatalogPathsConfig paths = new()
            {
                Weapons = weaponsPath,
                Armors = armorsPath,
                Items = itemsPath,
                Classes = classesPath,
                Enemies = enemiesPath
            };

            GameCatalogLoader loader = new(new GameDataLoader(NullLogger<GameDataLoader>.Instance));
            GameCatalog catalog = loader.Load(paths);
            Engine.Catalogs.EnemyDefinition enemy = catalog.EnemyCatalog.GetDefinition("TestEnemy");

            Assert.AreEqual(1, enemy.Abilities.Count);
            Assert.AreEqual(EnemyAbilityKind.Poison, enemy.Abilities[0].Kind);
            Assert.AreEqual(100, enemy.Abilities[0].Chance);
            Assert.AreEqual(2, enemy.Abilities[0].Duration);
            Assert.AreEqual(3, enemy.Abilities[0].Amount);
        }

        private static string WriteTempFile(string contents)
        {
            string path = Path.Combine(Path.GetTempPath(), $"hhs-catalog-{Guid.NewGuid():N}.json");
            File.WriteAllText(path, contents, Encoding.ASCII);
            return path;
        }
    }
}
