using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

/// <summary>
/// 触发器编辑器 ViewModel
/// </summary>
public partial class TriggerEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<TriggerDefinition> _triggers = [];

    [ObservableProperty]
    private TriggerDefinition? _selectedTrigger;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _scriptCode = "";

    public TriggerEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    partial void OnSelectedTriggerChanged(TriggerDefinition? value)
    {
        if (value != null)
        {
            ScriptCode = value.ActionScript ?? "";
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        string? dataDir = _projectService.CurrentProjectPath;
        if (dataDir == null)
        {
            StatusMessage = "No project loaded";
            return;
        }

        try
        {
            // 尝试从 game.json 加载
            string gameJsonPath = FindGameJsonPath(dataDir);
            if (gameJsonPath != null && File.Exists(gameJsonPath))
            {
                string json = await File.ReadAllTextAsync(gameJsonPath);
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("triggers", out JsonElement triggersElement))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    };

                    var result = JsonSerializer.Deserialize<List<TriggerDefinition>>(triggersElement.GetRawText(), options);
                    if (result != null && result.Count > 0)
                    {
                        Triggers = new ObservableCollection<TriggerDefinition>(result);
                        StatusMessage = $"Loaded {Triggers.Count} triggers from game.json";
                        return;
                    }
                }
            }

            // 尝试单独文件
            string triggersPath = Path.Combine(dataDir, "data", "triggers.json");
            if (File.Exists(triggersPath))
            {
                string json = await File.ReadAllTextAsync(triggersPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var result = JsonSerializer.Deserialize<List<TriggerDefinition>>(json, options);
                if (result != null)
                {
                    Triggers = new ObservableCollection<TriggerDefinition>(result);
                    StatusMessage = $"Loaded {Triggers.Count} triggers";
                    return;
                }
            }

            StatusMessage = "No triggers found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading triggers: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddTrigger()
    {
        var trigger = new TriggerDefinition
        {
            Id = $"trigger_{Triggers.Count + 1}",
            Name = "New Trigger",
            X = 0,
            Y = 0,
            Width = 1,
            Height = 1,
            Type = "OnEnter",
            IsEnabled = true,
            IsOneTime = false
        };

        Triggers.Add(trigger);
        SelectedTrigger = trigger;
        StatusMessage = $"Added trigger: {trigger.Id}";
    }

    [RelayCommand]
    private void RemoveTrigger()
    {
        if (SelectedTrigger != null)
        {
            string id = SelectedTrigger.Id;
            Triggers.Remove(SelectedTrigger);
            SelectedTrigger = null;
            StatusMessage = $"Removed trigger: {id}";
        }
    }

    [RelayCommand]
    private void UpdateScript()
    {
        if (SelectedTrigger != null)
        {
            SelectedTrigger.ActionScript = ScriptCode;
            StatusMessage = $"Updated script for trigger: {SelectedTrigger.Id}";
        }
    }

    [RelayCommand]
    public async Task SaveDataAsync()
    {
        string? dataDir = _projectService.CurrentProjectPath;
        if (dataDir == null)
        {
            StatusMessage = "No project loaded";
            return;
        }

        try
        {
            // 保存到单独文件
            string savePath = Path.Combine(dataDir, "data", "triggers.json");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Triggers.ToList(), options);
            await File.WriteAllTextAsync(savePath, json);

            StatusMessage = $"Saved {Triggers.Count} triggers";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
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
}

/// <summary>
/// 触发器定义
/// </summary>
public class TriggerDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    public string Type { get; set; } = "OnEnter";
    public string? ConditionScript { get; set; }
    public string? ActionScript { get; set; }
    public string? BuiltInAction { get; set; }
    public string? ActionParameter { get; set; }
    public bool IsOneTime { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int CooldownTurns { get; set; }
}
