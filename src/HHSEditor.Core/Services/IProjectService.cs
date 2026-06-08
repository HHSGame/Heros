namespace HHSEditor.Core.Services;

/// <summary>
/// 项目服务接口，管理编辑器项目。
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 当前项目路径。
    /// </summary>
    string? CurrentProjectPath { get; }

    /// <summary>
    /// 创建新项目。
    /// </summary>
    Task<bool> CreateProjectAsync(string projectPath, string projectName, CancellationToken ct = default);

    /// <summary>
    /// 打开项目。
    /// </summary>
    Task<bool> OpenProjectAsync(string projectPath, CancellationToken ct = default);

    /// <summary>
    /// 保存项目。
    /// </summary>
    Task SaveProjectAsync(CancellationToken ct = default);

    /// <summary>
    /// 关闭项目。
    /// </summary>
    void CloseProject();

    /// <summary>
    /// 获取项目数据目录。
    /// </summary>
    string? GetCatalogsDirectory();

    /// <summary>
    /// 获取地图目录。
    /// </summary>
    string? GetMapsDirectory();

    /// <summary>
    /// 获取脚本目录。
    /// </summary>
    string? GetScriptsDirectory();
}
