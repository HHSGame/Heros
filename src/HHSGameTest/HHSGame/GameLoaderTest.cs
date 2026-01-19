
using Microsoft.Extensions.Logging.Abstractions;

namespace HHSGame.Core
{
    [TestClass]
    public class GameDataLoaderTest
    {
        private static GameDataLoader CreateLoader()
        {
            return new GameDataLoader(NullLogger<GameDataLoader>.Instance);
        }

        [TestMethod]
        public void LoadDataThrowsWhenMissing()
        {
            GameDataLoader loader = CreateLoader();
            string path = Path.Combine(Path.GetTempPath(), $"hhs_missing_{Guid.NewGuid()}.json");
            Assert.IsFalse(File.Exists(path));
            Assert.ThrowsException<FileNotFoundException>(() => loader.LoadData<object>(path));
        }

        [TestMethod]
        public void LoadBytesThrowsWhenMissing()
        {
            GameDataLoader loader = CreateLoader();
            string path = Path.Combine(Path.GetTempPath(), $"hhs_missing_{Guid.NewGuid()}.bin");
            Assert.IsFalse(File.Exists(path));
            Assert.ThrowsException<FileNotFoundException>(() => loader.LoadBytes(path));
        }
    }
}
