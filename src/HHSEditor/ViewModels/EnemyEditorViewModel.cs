using System.Collections.ObjectModel;
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
    private ObservableCollection<WeaponDefinition> _availableWeapons = [];

    [ObservableProperty]
    private ObservableCollection<ArmorDefinition> _availableArmors = [];

    public EnemyEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        if (catalogsDir == null) return;

        var enemies = await _dataService.LoadDirectoryAsync<EnemyDefinition>(
            Path.Combine(catalogsDir, "enemies.json"));
        var weapons = await _dataService.LoadDirectoryAsync<WeaponDefinition>(
            Path.Combine(catalogsDir, "weapons.json"));
        var armors = await _dataService.LoadDirectoryAsync<ArmorDefinition>(
            Path.Combine(catalogsDir, "armors.json"));

        Enemies = new ObservableCollection<EnemyDefinition>(enemies);
        AvailableWeapons = new ObservableCollection<WeaponDefinition>(weapons);
        AvailableArmors = new ObservableCollection<ArmorDefinition>(armors);
    }

    [RelayCommand]
    private void AddEnemy()
    {
        var enemy = new EnemyDefinition(
            Id: "NewEnemy",
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
    }

    [RelayCommand]
    private void RemoveEnemy()
    {
        if (SelectedEnemy != null)
        {
            Enemies.Remove(SelectedEnemy);
            SelectedEnemy = null;
        }
    }

    [RelayCommand]
    private async Task SaveDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        if (catalogsDir == null) return;

        await _dataService.SaveAsync(Path.Combine(catalogsDir, "enemies.json"), Enemies.ToList());
    }

    public IEnumerable<EnemyDefinition> FilteredEnemies => string.IsNullOrWhiteSpace(SearchText)
        ? Enemies
        : Enemies.Where(e => e.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                              e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
}
