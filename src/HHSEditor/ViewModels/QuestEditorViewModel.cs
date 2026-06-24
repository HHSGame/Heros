using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSGame.Core.Quests;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class QuestEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<QuestDefinition> _quests = [];

    [ObservableProperty]
    private QuestDefinition? _selectedQuest;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private List<QuestGraphNode> _graphNodes = [];

    public QuestEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    partial void OnSelectedQuestChanged(QuestDefinition? value)
    {
        UpdateGraphNodes();
    }

    private void UpdateGraphNodes()
    {
        GraphNodes = Quests.Select(q => new QuestGraphNode
        {
            Id = q.Id,
            Name = q.Name,
            Description = q.Description.Length > 50 ? q.Description[..50] + "..." : q.Description,
            ObjectiveCount = q.Objectives.Count,
            RewardCount = q.Rewards.Count,
            IsSelected = q == SelectedQuest
        }).ToList();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        string? dataDir = _projectService.CurrentProjectPath;
        if (dataDir == null)
        {
            StatusMessage = "Error: No project loaded";
            return;
        }

        try
        {
            // Try to load from game.json first
            string gameJsonPath = FindGameJsonPath(dataDir);
            if (gameJsonPath != null && File.Exists(gameJsonPath))
            {
                string json = await File.ReadAllTextAsync(gameJsonPath);
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("quests", out JsonElement questsElement))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    };

                    var result = JsonSerializer.Deserialize<List<QuestDefinition>>(questsElement.GetRawText(), options);
                    if (result != null && result.Count > 0)
                    {
                        Quests = new ObservableCollection<QuestDefinition>(result);
                        StatusMessage = $"Loaded {Quests.Count} quests from game.json";
                        UpdateGraphNodes();
                        return;
                    }
                }
            }

            // Try separate files
            string[] possiblePaths =
            [
                Path.Combine(dataDir, "data", "quests.json"),
                Path.Combine(dataDir, "quests.json")
            ];

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    string json = await File.ReadAllTextAsync(path);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    };

                    var result = JsonSerializer.Deserialize<List<QuestDefinition>>(json, options);
                    if (result != null)
                    {
                        Quests = new ObservableCollection<QuestDefinition>(result);
                        StatusMessage = $"Loaded {Quests.Count} quests from {path}";
                        UpdateGraphNodes();
                        return;
                    }
                }
            }

            StatusMessage = "No quests found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading quests: {ex.Message}";
        }
    }

    private string? FindGameJsonPath(string dataDir)
    {
        string[] possiblePaths =
        [
            Path.Combine(dataDir, "game.json"),
            Path.Combine(dataDir, "data", "game.json")
        ];

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    [RelayCommand]
    private void AddQuest()
    {
        var quest = new QuestDefinition(
            Id: $"quest_{Quests.Count + 1}",
            Name: "New Quest",
            Description: "Quest description",
            Hint: "Quest hint",
            Objectives: [],
            Rewards: []);

        Quests.Add(quest);
        SelectedQuest = quest;
        StatusMessage = $"Added quest: {quest.Id}";
    }

    [RelayCommand]
    private void RemoveQuest()
    {
        if (SelectedQuest != null)
        {
            string id = SelectedQuest.Id;
            Quests.Remove(SelectedQuest);
            SelectedQuest = null;
            StatusMessage = $"Removed quest: {id}";
        }
    }

    [RelayCommand]
    public async Task SaveDataAsync()
    {
        string? dataDir = _projectService.CurrentProjectPath;
        if (dataDir == null)
        {
            StatusMessage = "Error: No project loaded";
            return;
        }

        try
        {
            string savePath = Path.Combine(dataDir, "data", "quests.json");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Quests.ToList(), options);
            await File.WriteAllTextAsync(savePath, json);

            StatusMessage = $"Saved {Quests.Count} quests";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    // Get dependencies for a quest
    public List<string> GetDependencies(string questId)
    {
        // For now, return empty - dependencies would need to be defined in the data model
        // This could be extended to parse dialogue effects that start quests
        return [];
    }

    // Get quests that depend on this quest
    public List<string> GetDependents(string questId)
    {
        // For now, return empty - would need to scan all dialogues for StartQuest effects
        return [];
    }
}

public class QuestGraphNode
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int ObjectiveCount { get; set; }
    public int RewardCount { get; set; }
    public bool IsSelected { get; set; }
}
