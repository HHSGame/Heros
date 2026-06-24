using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSEditor.Core.Services;
using System.ComponentModel;

namespace HHSEditor.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private string _title = "HHS Game Editor";

    [ObservableProperty]
    private string _statusMessage = "Ready - No project loaded";

    [ObservableProperty]
    private bool _isProjectLoaded;

    public string? CurrentProjectPath => _projectService.CurrentProjectPath;

    [ObservableProperty]
    private CatalogEditorViewModel _catalogEditor;

    [ObservableProperty]
    private EnemyEditorViewModel _enemyEditor;

    [ObservableProperty]
    private ClassEditorViewModel _classEditor;

    [ObservableProperty]
    private DialogueEditorViewModel _dialogueEditor;

    [ObservableProperty]
    private QuestEditorViewModel _questEditor;

    [ObservableProperty]
    private MapEditorViewModel _mapEditor;

    [ObservableProperty]
    private TriggerEditorViewModel _triggerEditor;

    [ObservableProperty]
    private GameTestViewModel _gameTest;

    public MainWindowViewModel()
    {
        _dataService = new JsonDataService();
        _projectService = new ProjectService();
        var gameLauncher = new GameLauncher(_projectService);

        _catalogEditor = new CatalogEditorViewModel(_dataService, _projectService);
        _enemyEditor = new EnemyEditorViewModel(_dataService, _projectService);
        _classEditor = new ClassEditorViewModel(_dataService, _projectService);
        _dialogueEditor = new DialogueEditorViewModel(_dataService, _projectService);
        _questEditor = new QuestEditorViewModel(_dataService, _projectService);
        _mapEditor = new MapEditorViewModel(_dataService, _projectService);
        _triggerEditor = new TriggerEditorViewModel(_dataService, _projectService);
        _gameTest = new GameTestViewModel(gameLauncher, _projectService);
    }

    public async Task CreateProjectFromPathAsync(string projectPath)
    {
        try
        {
            string projectName = Path.GetFileName(projectPath);

            bool success = await _projectService.CreateProjectAsync(projectPath, projectName);
            if (success)
            {
                IsProjectLoaded = true;
                StatusMessage = $"Project created: {projectPath}";
                Title = $"HHS Game Editor - {projectName}";
            }
            else
            {
                StatusMessage = "Failed to create project";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating project: {ex.Message}";
        }
    }

    public async Task OpenProjectFromPathAsync(string projectPath)
    {
        try
        {
            bool success = await _projectService.OpenProjectAsync(projectPath);
            if (success)
            {
                IsProjectLoaded = true;
                StatusMessage = $"Project opened: {projectPath}";
                Title = $"HHS Game Editor - {Path.GetFileName(projectPath)}";

                await LoadAllDataAsync();
            }
            else
            {
                StatusMessage = "Failed to open project - invalid project structure";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening project: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task OpenBuiltinGameDataAsync()
    {
        try
        {
            string currentDir = Directory.GetCurrentDirectory();

            string[] possiblePaths =
            [
                Path.Combine(currentDir, "data"),
                Path.Combine(currentDir, "..", "data"),
                "/Users/qliu23/workspace/fe/Slime/data"
            ];

            foreach (string path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    bool hasCatalogs = Directory.Exists(Path.Combine(fullPath, "catalogs"));
                    bool hasGameJson = File.Exists(Path.Combine(fullPath, "game.json"));

                    if (hasCatalogs || hasGameJson)
                    {
                        bool success = await _projectService.OpenProjectAsync(fullPath);
                        if (success)
                        {
                            IsProjectLoaded = true;
                            StatusMessage = $"Game data loaded: {fullPath}";
                            Title = "HHS Game Editor - Game Data";

                            await LoadAllDataAsync();
                            return;
                        }
                    }
                }
            }

            StatusMessage = "Could not find game data directory. Try Open Project instead.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading game data: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SaveAllAsync()
    {
        if (!IsProjectLoaded)
        {
            StatusMessage = "No project loaded";
            return;
        }

        try
        {
            await CatalogEditor.SaveDataAsync();
            await EnemyEditor.SaveDataAsync();
            await ClassEditor.SaveDataAsync();
            await DialogueEditor.SaveDataAsync();
            await QuestEditor.SaveDataAsync();
            await TriggerEditor.SaveDataAsync();
            StatusMessage = "All data saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    private async Task LoadAllDataAsync()
    {
        try
        {
            await CatalogEditor.LoadDataAsync();
            await EnemyEditor.LoadDataAsync();
            await ClassEditor.LoadDataAsync();
            await DialogueEditor.LoadDataAsync();
            await QuestEditor.LoadDataAsync();
            await TriggerEditor.LoadDataAsync();
            await MapEditor.LoadMapFilesAsync();
            StatusMessage = "All data loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
        }
    }
}
