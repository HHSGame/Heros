using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public CatalogEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    partial void OnWeaponsChanged(ObservableCollection<WeaponDefinition> value)
    {
        OnPropertyChanged(nameof(FilteredWeapons));
    }

    partial void OnArmorsChanged(ObservableCollection<ArmorDefinition> value)
    {
        OnPropertyChanged(nameof(FilteredArmors));
    }

    partial void OnItemsChanged(ObservableCollection<ItemDefinition> value)
    {
        OnPropertyChanged(nameof(FilteredItems));
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredWeapons));
        OnPropertyChanged(nameof(FilteredArmors));
        OnPropertyChanged(nameof(FilteredItems));
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
            // Load weapons
            string weaponsPath = Path.Combine(catalogsDir, "weapons.json");
            if (File.Exists(weaponsPath))
            {
                var weapons = await LoadCatalogAsync<WeaponDefinition>(weaponsPath, "weapons");
                Weapons = new ObservableCollection<WeaponDefinition>(weapons);
            }

            // Load armors
            string armorsPath = Path.Combine(catalogsDir, "armors.json");
            if (File.Exists(armorsPath))
            {
                var armors = await LoadCatalogAsync<ArmorDefinition>(armorsPath, "armors");
                Armors = new ObservableCollection<ArmorDefinition>(armors);
            }

            // Load items
            string itemsPath = Path.Combine(catalogsDir, "items.json");
            if (File.Exists(itemsPath))
            {
                var items = await LoadCatalogAsync<ItemDefinition>(itemsPath, "items");
                Items = new ObservableCollection<ItemDefinition>(items);
            }

            StatusMessage = $"Loaded {Weapons.Count} weapons, {Armors.Count} armors, {Items.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            Console.WriteLine($"[CatalogEditor] Exception: {ex}");
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
                Console.WriteLine($"[CatalogEditor] Loaded {result?.Count ?? 0} items from {filePath}");
                return result ?? [];
            }
            else
            {
                // Try to deserialize as direct array
                var result = JsonSerializer.Deserialize<List<T>>(json, options);
                Console.WriteLine($"[CatalogEditor] Loaded {result?.Count ?? 0} items from {filePath} (direct array)");
                return result ?? [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CatalogEditor] Error loading {filePath}: {ex.Message}");
            return [];
        }
    }

    [RelayCommand]
    private void AddWeapon()
    {
        var weapon = new WeaponDefinition(
            Id: $"weapon_{Weapons.Count + 1}",
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
        StatusMessage = $"Added weapon: {weapon.Id}";
    }

    [RelayCommand]
    private void RemoveWeapon()
    {
        if (SelectedWeapon != null)
        {
            string id = SelectedWeapon.Id;
            Weapons.Remove(SelectedWeapon);
            SelectedWeapon = null;
            StatusMessage = $"Removed weapon: {id}";
        }
    }

    [RelayCommand]
    private void AddArmor()
    {
        var armor = new ArmorDefinition(
            Id: $"armor_{Armors.Count + 1}",
            Name: "New Armor",
            Rarity: ItemRarity.Common,
            Value: 0,
            Weight: 1.0f,
            ArmorValue: 5);

        Armors.Add(armor);
        SelectedArmor = armor;
        StatusMessage = $"Added armor: {armor.Id}";
    }

    [RelayCommand]
    private void RemoveArmor()
    {
        if (SelectedArmor != null)
        {
            string id = SelectedArmor.Id;
            Armors.Remove(SelectedArmor);
            SelectedArmor = null;
            StatusMessage = $"Removed armor: {id}";
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var item = new ItemDefinition(
            Id: $"item_{Items.Count + 1}",
            Kind: "HealthPotion",
            Name: "New Item",
            Rarity: ItemRarity.Common,
            Value: 0,
            Weight: 0.5f,
            HealAmount: 10);

        Items.Add(item);
        SelectedItem = item;
        StatusMessage = $"Added item: {item.Id}";
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem != null)
        {
            string id = SelectedItem.Id;
            Items.Remove(SelectedItem);
            SelectedItem = null;
            StatusMessage = $"Removed item: {id}";
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
            await SaveCatalogAsync(Path.Combine(catalogsDir, "weapons.json"), "weapons", Weapons.ToList());
            await SaveCatalogAsync(Path.Combine(catalogsDir, "armors.json"), "armors", Armors.ToList());
            await SaveCatalogAsync(Path.Combine(catalogsDir, "items.json"), "items", Items.ToList());

            StatusMessage = $"Saved {Weapons.Count} weapons, {Armors.Count} armors, {Items.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    private async Task SaveCatalogAsync<T>(string filePath, string arrayPropertyName, List<T> data)
    {
        var wrapper = new Dictionary<string, List<T>> { { arrayPropertyName, data } };
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(wrapper, options);
        await File.WriteAllTextAsync(filePath, json);
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
