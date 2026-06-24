using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSGame.Core.Dialogue;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class DialogueEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<DialogueDefinition> _dialogues = [];

    [ObservableProperty]
    private DialogueDefinition? _selectedDialogue;

    [ObservableProperty]
    private DialogueNode? _selectedNode;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private List<DialogueGraphNode> _graphNodes = [];

    public DialogueEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    public ObservableCollection<DialogueNode> Nodes
    {
        get
        {
            if (SelectedDialogue == null) return [];
            return new ObservableCollection<DialogueNode>(SelectedDialogue.Nodes.Values);
        }
    }

    partial void OnSelectedDialogueChanged(DialogueDefinition? value)
    {
        OnPropertyChanged(nameof(Nodes));
        SelectedNode = null;
        UpdateGraphNodes();
    }

    private void UpdateGraphNodes()
    {
        if (SelectedDialogue == null)
        {
            GraphNodes = [];
            return;
        }

        GraphNodes = SelectedDialogue.Nodes.Values.Select(n => new DialogueGraphNode
        {
            Id = n.Id,
            Text = n.Text.Length > 50 ? n.Text[..50] + "..." : n.Text,
            Options = n.Options.Select(o => new DialogueGraphEdge
            {
                Text = o.Text.Length > 30 ? o.Text[..30] + "..." : o.Text,
                TargetNodeId = o.NextNodeId ?? ""
            }).ToList()
        }).ToList();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        string? dataDir = _projectService.CurrentProjectPath;
        Console.WriteLine($"[DialogueEditor] LoadDataAsync called. dataDir={dataDir}");

        if (dataDir == null)
        {
            StatusMessage = "Error: No project loaded";
            return;
        }

        try
        {
            // Try to load from game.json first
            string gameJsonPath = FindGameJsonPath(dataDir);
            Console.WriteLine($"[DialogueEditor] gameJsonPath={gameJsonPath}");

            if (gameJsonPath != null && File.Exists(gameJsonPath))
            {
                string json = await File.ReadAllTextAsync(gameJsonPath);
                Console.WriteLine($"[DialogueEditor] game.json length: {json.Length}");

                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("dialogues", out JsonElement dialoguesElement))
                {
                    Console.WriteLine($"[DialogueEditor] Found dialogues element, kind={dialoguesElement.ValueKind}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    try
                    {
                        // Deserialize as DTO first
                        var dtoList = JsonSerializer.Deserialize<List<DialogueDefinitionDto>>(dialoguesElement.GetRawText(), options);
                        Console.WriteLine($"[DialogueEditor] Deserialized {dtoList?.Count ?? 0} dialogue DTOs");

                        if (dtoList != null && dtoList.Count > 0)
                        {
                            // Convert DTOs to domain models
                            var dialogues = dtoList.Select(dto => dto.ToDefinition()).ToList();
                            Dialogues = new ObservableCollection<DialogueDefinition>(dialogues);
                            StatusMessage = $"Loaded {Dialogues.Count} dialogues from game.json";
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DialogueEditor] Deserialization error: {ex.Message}");
                        StatusMessage = $"Error deserializing dialogues: {ex.Message}";
                    }
                }
                else
                {
                    Console.WriteLine("[DialogueEditor] No 'dialogues' property found in game.json");
                }
            }

            StatusMessage = "No dialogues found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dialogues: {ex.Message}";
            Console.WriteLine($"[DialogueEditor] Exception: {ex}");
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
    private void SelectNode(string? nodeId)
    {
        if (SelectedDialogue == null || nodeId == null) return;

        if (SelectedDialogue.Nodes.TryGetValue(nodeId, out DialogueNode? node))
        {
            SelectedNode = node;
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
            string savePath = Path.Combine(dataDir, "data", "dialogues.json");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Dialogues.ToList(), options);
            await File.WriteAllTextAsync(savePath, json);

            StatusMessage = $"Saved {Dialogues.Count} dialogues";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    // Graph data for visualization
    public List<DialogueGraphNode> GetGraphNodes()
    {
        if (SelectedDialogue == null) return [];

        return SelectedDialogue.Nodes.Values.Select(n => new DialogueGraphNode
        {
            Id = n.Id,
            Text = n.Text.Length > 50 ? n.Text[..50] + "..." : n.Text,
            Options = n.Options.Select(o => new DialogueGraphEdge
            {
                Text = o.Text.Length > 30 ? o.Text[..30] + "..." : o.Text,
                TargetNodeId = o.NextNodeId ?? ""
            }).ToList()
        }).ToList();
    }
}

// DTO for JSON deserialization (nodes is array in JSON)
public class DialogueDefinitionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("startNodeId")]
    public string StartNodeId { get; set; } = "";

    [JsonPropertyName("nodes")]
    public List<DialogueNodeDto> Nodes { get; set; } = [];

    public DialogueDefinition ToDefinition()
    {
        var nodesDict = Nodes.ToDictionary(n => n.Id, n => n.ToNode());
        return new DialogueDefinition(Id, StartNodeId, nodesDict);
    }
}

public class DialogueNodeDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("options")]
    public List<DialogueOptionDto> Options { get; set; } = [];

    public DialogueNode ToNode()
    {
        return new DialogueNode(Id, Text, Options.Select(o => o.ToOption()).ToList());
    }
}

public class DialogueOptionDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("nextNodeId")]
    public string? NextNodeId { get; set; }

    [JsonPropertyName("requirements")]
    public List<DialogueRequirementDto>? Requirements { get; set; }

    [JsonPropertyName("effects")]
    public List<DialogueEffectDto>? Effects { get; set; }

    public DialogueOption ToOption()
    {
        var requirements = (Requirements ?? []).Select(r => r.ToRequirement()).ToList();
        var effects = (Effects ?? []).Select(e => e.ToEffect()).ToList();
        return new DialogueOption(Text, NextNodeId, requirements, effects);
    }
}

public class DialogueRequirementDto
{
    [JsonPropertyName("type")]
    public DialogueRequirementType Type { get; set; }

    [JsonPropertyName("skill")]
    public HHSGame.Core.Stats.SkillType? Skill { get; set; }

    [JsonPropertyName("attribute")]
    public HHSGame.Core.Stats.AttributeType? Attribute { get; set; }

    [JsonPropertyName("questId")]
    public string? QuestId { get; set; }

    [JsonPropertyName("questStatus")]
    public HHSGame.Core.Quests.QuestStatus? QuestStatus { get; set; }

    [JsonPropertyName("minimum")]
    public int Minimum { get; set; }

    public DialogueRequirement ToRequirement()
    {
        return new DialogueRequirement(Type, Skill, Attribute, QuestId, QuestStatus, Minimum);
    }
}

public class DialogueEffectDto
{
    [JsonPropertyName("type")]
    public DialogueEffectType Type { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    public DialogueEffect ToEffect()
    {
        return new DialogueEffect(Type, Target, Amount);
    }
}

public class DialogueGraphNode
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public List<DialogueGraphEdge> Options { get; set; } = [];
}

public class DialogueGraphEdge
{
    public string Text { get; set; } = "";
    public string TargetNodeId { get; set; } = "";
}
