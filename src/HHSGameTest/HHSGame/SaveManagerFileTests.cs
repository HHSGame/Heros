using System.Text.Json;
using HHSGame.Core;
using HHSGame.Core.Save;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class SaveManagerFileTests : IDisposable
    {
        private readonly string tempDir;

        public SaveManagerFileTests()
        {
            tempDir = Path.Combine(Path.GetTempPath(), $"hhs-save-test-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        private SaveManager CreateSaveManager()
        {
            return new SaveManager(NullLogger<SaveManager>.Instance, tempDir);
        }

        private LoadManager CreateLoadManager()
        {
            return new LoadManager(NullLogger<LoadManager>.Instance, tempDir);
        }

        // ── File naming ──────────────────────────────────────────────────

        [TestMethod]
        public void GetSlotPath_ReturnsCorrectName()
        {
            SaveManager manager = CreateSaveManager();

            string path = manager.GetSlotPath(1);
            Assert.AreEqual(Path.Combine(tempDir, "slot-1.json"), path);

            string path3 = manager.GetSlotPath(3);
            Assert.AreEqual(Path.Combine(tempDir, "slot-3.json"), path3);
        }

        [TestMethod]
        public void ListSlots_Returns3EmptySlots_WhenNoFilesExist()
        {
            SaveManager manager = CreateSaveManager();

            SaveSlotInfo[] slots = manager.ListSlots();
            Assert.AreEqual(3, slots.Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.IsFalse(slots[i].IsOccupied);
                Assert.AreEqual(i + 1, slots[i].SlotNumber);
            }
        }

        // ── State guards ─────────────────────────────────────────────────

        [TestMethod]
        public void CanSaveInCurrentState_Exploration_ReturnsTrue()
        {
            Assert.IsTrue(SaveManager.CanSaveInCurrentState(GameStateType.Exploration));
        }

        [TestMethod]
        public void CanSaveInCurrentState_Combat_ReturnsFalse()
        {
            Assert.IsFalse(SaveManager.CanSaveInCurrentState(GameStateType.Combat));
        }

        [TestMethod]
        public void CanSaveInCurrentState_Dialogue_ReturnsFalse()
        {
            Assert.IsFalse(SaveManager.CanSaveInCurrentState(GameStateType.Dialogue));
        }

        [TestMethod]
        public void CanLoadInCurrentState_Exploration_ReturnsTrue()
        {
            Assert.IsTrue(LoadManager.CanLoadInCurrentState(GameStateType.Exploration));
        }

        [TestMethod]
        public void CanLoadInCurrentState_GameOver_ReturnsTrue()
        {
            Assert.IsTrue(LoadManager.CanLoadInCurrentState(GameStateType.GameOver));
        }

        [TestMethod]
        public void CanLoadInCurrentState_Combat_ReturnsFalse()
        {
            Assert.IsFalse(LoadManager.CanLoadInCurrentState(GameStateType.Combat));
        }

        // ── Corrupted file handling ──────────────────────────────────────

        [TestMethod]
        public void LoadSlot_CorruptedJson_ReturnsNull()
        {
            LoadManager loader = CreateLoadManager();

            string slotPath = loader.GetSlotPath(1);
            File.WriteAllText(slotPath, "this is not json { broken");

            SaveGameData? result = loader.LoadSlot(1);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadSlot_IncompatibleVersion_ReturnsNull()
        {
            LoadManager loader = CreateLoadManager();

            string slotPath = loader.GetSlotPath(1);
            SaveGameData data = new()
            {
                Version = 999,
                SaveName = "Old save",
                SaveTime = DateTime.UtcNow,
                TurnNumber = 10
            };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(slotPath, json);

            SaveGameData? result = loader.LoadSlot(1);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadSlot_MissingSlot_ReturnsNull()
        {
            LoadManager loader = CreateLoadManager();

            SaveGameData? result = loader.LoadSlot(2);
            Assert.IsNull(result);
        }

        // ── Delete ───────────────────────────────────────────────────────

        [TestMethod]
        public void DeleteSlot_RemovesFile()
        {
            LoadManager loader = CreateLoadManager();

            string slotPath = loader.GetSlotPath(1);
            File.WriteAllText(slotPath, "{}");
            Assert.IsTrue(File.Exists(slotPath));

            bool deleted = loader.DeleteSlot(1);
            Assert.IsTrue(deleted);
            Assert.IsFalse(File.Exists(slotPath));
        }

        [TestMethod]
        public void DeleteSlot_NonExistent_ReturnsFalse()
        {
            LoadManager loader = CreateLoadManager();

            bool deleted = loader.DeleteSlot(99);
            Assert.IsFalse(deleted);
        }

        // ── ListSlots reads populated file ───────────────────────────────

        [TestMethod]
        public void ListSlots_ReadsOccupiedSlot()
        {
            SaveManager manager = CreateSaveManager();

            SaveGameData data = new()
            {
                Version = 1,
                SaveName = "TestHero - Turn 10",
                SaveTime = new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc),
                TurnNumber = 10,
                Players =
                [
                    new PlayerSaveData { Name = "TestHero", Level = 3 }
                ]
            };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(manager.GetSlotPath(2), json);

            SaveSlotInfo[] slots = manager.ListSlots();
            Assert.AreEqual(3, slots.Length);
            Assert.IsFalse(slots[0].IsOccupied);
            Assert.IsTrue(slots[1].IsOccupied);
            Assert.AreEqual("TestHero", slots[1].PlayerName);
            Assert.AreEqual(3, slots[1].PlayerLevel);
            Assert.AreEqual(10, slots[1].TurnNumber);
            Assert.IsFalse(slots[2].IsOccupied);
        }

        // ── Save name auto-generation ────────────────────────────────────

        [TestMethod]
        public void SaveGameData_DefaultVersion_IsOne()
        {
            SaveGameData data = new();
            Assert.AreEqual(1, data.Version);
            Assert.AreEqual(string.Empty, data.SaveName);
        }

        // ── File extension ───────────────────────────────────────────────

        [TestMethod]
        public void SlotPath_EndsWithJson()
        {
            SaveManager manager = CreateSaveManager();

            string path = manager.GetSlotPath(1);
            Assert.IsTrue(path.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        }
    }
}