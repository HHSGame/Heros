namespace HHSGame.Core.Save
{
    /// <summary>
    /// 存档路径工具类，提供存档文件路径和槽位检查功能。
    /// </summary>
    public sealed class SavePathHelper
    {
        public static readonly string DefaultSaveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".hhs-game", "saves");

        private const string SaveFileExtension = ".json";

        private readonly string saveDirectory;

        public SavePathHelper(string? saveDirectory = null)
        {
            this.saveDirectory = saveDirectory ?? DefaultSaveDirectory;
            Directory.CreateDirectory(this.saveDirectory);
        }

        public string SaveDirectory => saveDirectory;

        public string GetSlotPath(int slot)
        {
            return Path.Combine(saveDirectory, $"slot-{slot}{SaveFileExtension}");
        }

        public bool SlotExists(int slot)
        {
            return File.Exists(GetSlotPath(slot));
        }
    }
}
