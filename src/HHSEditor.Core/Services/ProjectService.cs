namespace HHSEditor.Core.Services;

/// <summary>
/// 项目服务实现。
/// </summary>
public sealed class ProjectService : IProjectService
{
    public string? CurrentProjectPath { get; private set; }

    public Task<bool> CreateProjectAsync(string projectPath, string projectName, CancellationToken ct = default)
    {
        try
        {
            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "catalogs"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "maps"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "scripts"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "story"));

            CurrentProjectPath = projectPath;
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> OpenProjectAsync(string projectPath, CancellationToken ct = default)
    {
        if (!Directory.Exists(projectPath))
        {
            return Task.FromResult(false);
        }

        CurrentProjectPath = projectPath;
        return Task.FromResult(true);
    }

    public Task SaveProjectAsync(CancellationToken ct = default)
    {
        // Project structure is already saved on disk
        return Task.CompletedTask;
    }

    public void CloseProject()
    {
        CurrentProjectPath = null;
    }

    public string? GetCatalogsDirectory()
    {
        return CurrentProjectPath != null
            ? Path.Combine(CurrentProjectPath, "data", "catalogs")
            : null;
    }

    public string? GetMapsDirectory()
    {
        return CurrentProjectPath != null
            ? Path.Combine(CurrentProjectPath, "data", "maps")
            : null;
    }

    public string? GetScriptsDirectory()
    {
        return CurrentProjectPath != null
            ? Path.Combine(CurrentProjectPath, "data", "scripts")
            : null;
    }
}
