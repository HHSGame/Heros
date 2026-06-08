using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class CatalogEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<WeaponDefinition> _weapons = [];

    [ObservableProperty]
    private ObservableCollection<ArmorDefinition> _armors = [];

    [ObservableProperty]
    private ObservableCollection<ItemDefinition> _items = [];

    [ObservableProperty]
    private WeaponDefinition? _selectedWeapon;

    [ObservableProperty]
    private ArmorDefinition? _selectedArmor;

    [ObservableProperty]
    private ItemDefinition? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _selectedTabIndex;

    public CatalogEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        if (catalogsDir == null) return;

        var weapons = await _dataService.LoadDirectoryAsync<WeaponDefinition>(
            Path.Combine(catalogsDir, "weapons.json"));
        var armors = await _dataService.LoadDirectoryAsync<ArmorDefinition>(
            Path.Combine(catalogsDir, "armors.json"));
        var items = await _dataService.LoadDirectoryAsync<ItemDefinition>(
            Path.Combine(catalogsDir, "items.json"));

        Weapons = new ObservableCollection<WeaponDefinition>(weapons);
        Armors = new ObservableCollection<ArmorDefinition>(armors);
        Items = new ObservableCollection<ItemDefinition>(items);
    }

    [RelayCommand]
    private void AddWeapon()
    {
        var weapon = new WeaponDefinition(
            Id: "NewWeapon",
            Name: "New Weapon",
            Rarity: ItemRarity.Common,
            Value: 0,
            Weight: 1.0f,
            Damage: 10,
            Penetration: 0,
            Range: 1,
            WeaponType: WeaponType.MeleeLight,
            Trajectory: WeaponTrajectory.Line);

        Weapons.Add(weapon);
        SelectedWeapon = weapon;
    }

    [RelayCommand]
    private void RemoveWeapon()
    {
        if (SelectedWeapon != null)
        {
            Weapons.Remove(SelectedWeapon);
            SelectedWeapon = null;
        }
    }

    [RelayCommand]
    private void AddArmor()
    {
        var armor = new ArmorDefinition(
            Id: "NewArmor",
            Name: "New Armor",
            Rarity: ItemRarity.Common,
            Value: 0,
            Weight: 1.0f,
            ArmorValue: 5);

        Armors.Add(armor);
        SelectedArmor = armor;
    }

    [RelayCommand]
    private void RemoveArmor()
    {
        if (SelectedArmor != null)
        {
            Armors.Remove(SelectedArmor);
            SelectedArmor = null;
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var item = new ItemDefinition(
            Id: "NewItem",
            Kind: "HealthPotion",
            Name: "New Item",
            Rarity: ItemRarity.Common,
            Value: 0,
            Weight: 0.5f,
            HealAmount: 10);

        Items.Add(item);
        SelectedItem = item;
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem != null)
        {
            Items.Remove(SelectedItem);
            SelectedItem = null;
        }
    }

    [RelayCommand]
    private async Task SaveDataAsync()
    {
        string? catalogsDir = _projectService.GetCatalogsDirectory();
        if (catalogsDir == null) return;

        await _dataService.SaveAsync(Path.Combine(catalogsDir, "weapons.json"), Weapons.ToList());
        await _dataService.SaveAsync(Path.Combine(catalogsDir, "armors.json"), Armors.ToList());
        await _dataService.SaveAsync(Path.Combine(catalogsDir, "items.json"), Items.ToList());
    }

    public IEnumerable<WeaponDefinition> FilteredWeapons => string.IsNullOrWhiteSpace(SearchText)
        ? Weapons
        : Weapons.Where(w => w.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                              w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ArmorDefinition> FilteredArmors => string.IsNullOrWhiteSpace(SearchText)
        ? Armors
        : Armors.Where(a => a.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                             a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ItemDefinition> FilteredItems => string.IsNullOrWhiteSpace(SearchText)
        ? Items
        : Items.Where(i => i.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
}
