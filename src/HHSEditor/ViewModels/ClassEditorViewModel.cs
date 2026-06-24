using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Stats;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class ClassEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<ClassDefinition> _classes = [];

    [ObservableProperty]
    private ClassDefinition? _selectedClass;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public ClassEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    partial void OnSelectedClassChanged(ClassDefinition? value)
    {
        OnPropertyChanged(nameof(SelectedAttributes));
        OnPropertyChanged(nameof(SelectedSkills));
    }

    public Attributes? SelectedAttributes => SelectedClass?.Attributes;
    public Skills? SelectedSkills => SelectedClass?.Skills;

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        if (catalogsDir == null)
        {
            StatusMessage = "Error: No project loaded";
            return;
        }

        try
        {
            string classesPath = Path.Combine(catalogsDir, "classes.json");
            if (File.Exists(classesPath))
            {
                string json = await File.ReadAllTextAsync(classesPath);
                using JsonDocument doc = JsonDocument.Parse(json);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                if (doc.RootElement.TryGetProperty("classes", out JsonElement arrayElement))
                {
                    var result = JsonSerializer.Deserialize<List<ClassDefinition>>(arrayElement.GetRawText(), options);
                    Classes = new ObservableCollection<ClassDefinition>(result ?? []);
                }
            }

            StatusMessage = $"Loaded {Classes.Count} classes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading classes: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddClass()
    {
        var classDef = new ClassDefinition(
            Id: $"class_{Classes.Count + 1}",
            Name: "New Class",
            Attributes: new Attributes { Strength = 10, Perception = 10, Agility = 10, Charisma = 10, Intelligence = 10 },
            Skills: new Skills(),
            WeaponId: "UnknownWeapon",
            ArmorId: "UnknownArmor");

        Classes.Add(classDef);
        SelectedClass = classDef;
        StatusMessage = $"Added class: {classDef.Id}";
    }

    [RelayCommand]
    private void RemoveClass()
    {
        if (SelectedClass != null)
        {
            string id = SelectedClass.Id;
            Classes.Remove(SelectedClass);
            SelectedClass = null;
            StatusMessage = $"Removed class: {id}";
        }
    }

    [RelayCommand]
    public async Task SaveDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        if (catalogsDir == null)
        {
            StatusMessage = "Error: No project loaded";
            return;
        }

        try
        {
            Directory.CreateDirectory(catalogsDir);

            var wrapper = new Dictionary<string, List<ClassDefinition>> { { "classes", Classes.ToList() } };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(wrapper, options);
            await File.WriteAllTextAsync(Path.Combine(catalogsDir, "classes.json"), json);

            StatusMessage = $"Saved {Classes.Count} classes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }
}
