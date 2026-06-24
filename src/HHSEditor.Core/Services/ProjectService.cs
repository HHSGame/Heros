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
            // Create project structure
            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(Path.Combine(projectPath, "data"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "catalogs"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "maps"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "scripts"));
            Directory.CreateDirectory(Path.Combine(projectPath, "data", "story"));

            // Create empty catalog files
            string catalogsDir = Path.Combine(projectPath, "data", "catalogs");
            File.WriteAllText(Path.Combine(catalogsDir, "weapons.json"), "[]");
            File.WriteAllText(Path.Combine(catalogsDir, "armors.json"), "[]");
            File.WriteAllText(Path.Combine(catalogsDir, "items.json"), "[]");
            File.WriteAllText(Path.Combine(catalogsDir, "enemies.json"), "[]");
            File.WriteAllText(Path.Combine(catalogsDir, "classes.json"), "[]");

            // Create game.json
            string gameJson = """
            {
                "config": {
                    "mapStyle": "UrbanStreet",
                    "mapWidth": 40,
                    "mapHeight": 25
                },
                "catalogs": {
                    "weapons": "catalogs/weapons.json",
                    "armors": "catalogs/armors.json",
                    "items": "catalogs/items.json",
                    "enemies": "catalogs/enemies.json",
                    "classes": "catalogs/classes.json"
                }
            }
            """;
            File.WriteAllText(Path.Combine(projectPath, "data", "game.json"), gameJson);

            CurrentProjectPath = projectPath;
            return Task.FromResult(true);
        }
        catch (Exception)
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

        // Check if this is a valid project directory
        // Accept if it has any of these:
        // - catalogs/ subdirectory
        // - data/catalogs/ subdirectory
        // - game.json file
        // - data/game.json file
        // - Or if it IS a catalogs directory (has .json files)

        bool hasCatalogs = Directory.Exists(Path.Combine(projectPath, "catalogs"));
        bool hasDataCatalogs = Directory.Exists(Path.Combine(projectPath, "data", "catalogs"));
        bool hasGameJson = File.Exists(Path.Combine(projectPath, "game.json"));
        bool hasDataGameJson = File.Exists(Path.Combine(projectPath, "data", "game.json"));

        // Check if this directory itself contains catalog files
        bool hasJsonFiles = Directory.GetFiles(projectPath, "*.json").Length > 0;

        if (hasCatalogs || hasDataCatalogs || hasGameJson || hasDataGameJson || hasJsonFiles)
        {
            CurrentProjectPath = projectPath;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
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
        if (CurrentProjectPath == null) return null;

        // Check different possible locations
        string[] possiblePaths =
        [
            Path.Combine(CurrentProjectPath, "data", "catalogs"),
            Path.Combine(CurrentProjectPath, "catalogs"),
            CurrentProjectPath // If the project IS the catalogs directory
        ];

        foreach (string path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        // If no catalogs directory exists, create one
        string defaultPath = Path.Combine(CurrentProjectPath, "data", "catalogs");
        Directory.CreateDirectory(defaultPath);
        return defaultPath;
    }

    public string? GetMapsDirectory()
    {
        if (CurrentProjectPath == null) return null;

        string[] possiblePaths =
        [
            Path.Combine(CurrentProjectPath, "data", "maps"),
            Path.Combine(CurrentProjectPath, "maps")
        ];

        foreach (string path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    public string? GetScriptsDirectory()
    {
        if (CurrentProjectPath == null) return null;

        string[] possiblePaths =
        [
            Path.Combine(CurrentProjectPath, "data", "scripts"),
            Path.Combine(CurrentProjectPath, "scripts")
        ];

        foreach (string path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
