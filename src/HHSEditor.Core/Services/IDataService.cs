namespace HHSEditor.Core.Services;

/// <summary>
/// 数据服务接口，负责读写游戏数据文件。
/// </summary>
public interface IDataService
{
    /// <summary>
    /// 从文件加载数据。
    /// </summary>
    Task<T?> LoadAsync<T>(string filePath, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 保存数据到文件。
    /// </summary>
    Task SaveAsync<T>(string filePath, T data, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 加载目录下所有匹配的文件。
    /// </summary>
    Task<IReadOnlyList<T>> LoadDirectoryAsync<T>(string directoryPath, string searchPattern = "*.json", CancellationToken ct = default) where T : class;

    /// <summary>
    /// 检查文件是否存在。
    /// </summary>
    bool FileExists(string filePath);

    /// <summary>
    /// 列出目录下的文件。
    /// </summary>
    IReadOnlyList<string> ListFiles(string directoryPath, string searchPattern = "*.json");
}
