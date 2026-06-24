using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Enemies;
using HHSGame.Core.Rendering;
using HHSGame.Core.Stats;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class EnemyEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<EnemyDefinition> _enemies = [];

    [ObservableProperty]
    private EnemyDefinition? _selectedEnemy;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public EnemyEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    partial void OnEnemiesChanged(ObservableCollection<EnemyDefinition> value)
    {
        OnPropertyChanged(nameof(FilteredEnemies));
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredEnemies));
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        StatusMessage = $"Loading from: {catalogsDir ?? "null"}";

        if (catalogsDir == null)
        {
            StatusMessage = "Error: No project loaded. Please open a project first.";
            return;
        }

        if (!Directory.Exists(catalogsDir))
        {
            StatusMessage = $"Error: Directory not found: {catalogsDir}";
            return;
        }

        try
        {
            string enemiesPath = Path.Combine(catalogsDir, "enemies.json");
            if (File.Exists(enemiesPath))
            {
                var enemies = await LoadCatalogAsync<EnemyDefinition>(enemiesPath, "enemies");
                Enemies = new ObservableCollection<EnemyDefinition>(enemies);
            }

            StatusMessage = $"Loaded {Enemies.Count} enemies";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading enemies: {ex.Message}";
            Console.WriteLine($"[EnemyEditor] Exception: {ex}");
        }
    }

    private async Task<List<T>> LoadCatalogAsync<T>(string filePath, string arrayPropertyName) where T : class
    {
        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            using JsonDocument doc = JsonDocument.Parse(json);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            if (doc.RootElement.TryGetProperty(arrayPropertyName, out JsonElement arrayElement))
            {
                var result = JsonSerializer.Deserialize<List<T>>(arrayElement.GetRawText(), options);
                Console.WriteLine($"[EnemyEditor] Loaded {result?.Count ?? 0} items from {filePath}");
                return result ?? [];
            }
            else
            {
                // Try to deserialize as direct array
                var result = JsonSerializer.Deserialize<List<T>>(json, options);
                Console.WriteLine($"[EnemyEditor] Loaded {result?.Count ?? 0} items from {filePath} (direct array)");
                return result ?? [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EnemyEditor] Error loading {filePath}: {ex.Message}");
            return [];
        }
    }

    [RelayCommand]
    private void AddEnemy()
    {
        var enemy = new EnemyDefinition(
            Id: $"enemy_{Enemies.Count + 1}",
            Name: "New Enemy",
            Faction: "Axis",
            Attributes: new Attributes { Strength = 10, Perception = 10, Agility = 10, Charisma = 10, Intelligence = 10 },
            Skills: new Skills(),
            WeaponId: "UnknownWeapon",
            ArmorId: "UnknownArmor",
            ExperienceValue: 10,
            Glyph: 'E',
            Attribute: ColorPresets.Enemies.Occupier,
            Loot: [],
            Abilities: []);

        Enemies.Add(enemy);
        SelectedEnemy = enemy;
        StatusMessage = $"Added enemy: {enemy.Id}";
    }

    [RelayCommand]
    private void RemoveEnemy()
    {
        if (SelectedEnemy != null)
        {
            string id = SelectedEnemy.Id;
            Enemies.Remove(SelectedEnemy);
            SelectedEnemy = null;
            StatusMessage = $"Removed enemy: {id}";
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

            // Save with wrapper object
            var wrapper = new Dictionary<string, List<EnemyDefinition>> { { "enemies", Enemies.ToList() } };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(wrapper, options);
            await File.WriteAllTextAsync(Path.Combine(catalogsDir, "enemies.json"), json);

            StatusMessage = $"Saved {Enemies.Count} enemies";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving enemies: {ex.Message}";
        }
    }

    public IEnumerable<EnemyDefinition> FilteredEnemies => string.IsNullOrWhiteSpace(SearchText)
        ? Enemies
        : Enemies.Where(e => e.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                              e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
}
