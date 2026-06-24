using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class MapEditorViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IProjectService _projectService;

    // 多标签页支持
    [ObservableProperty]
    private ObservableCollection<MapTabViewModel> _openMaps = [];

    [ObservableProperty]
    private MapTabViewModel? _selectedMap;

    // 当前工具（全局）
    [ObservableProperty]
    private string _currentTool = "Paint";

    [ObservableProperty]
    private TileType? _selectedTileType;

    [ObservableProperty]
    private char _currentBrush = '.';

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private ObservableCollection<string> _mapFiles = [];

    // 当前选择的实体类型（用于放置模式）
    [ObservableProperty]
    private MapEntityType? _selectedEntityType;

    // 实体属性面板
    [ObservableProperty]
    private MapEntity? _editingEntity;

    // Available tile types
    public ObservableCollection<TileType> AvailableTiles { get; } =
    [
        new('.', "Floor", "Walkable floor"),
        new('#', "Wall", "Solid wall"),
        new('+', "Door", "Closed door"),
        new('-', "Open Door", "Open door"),
        new('~', "Water", "Water"),
        new('^', "Lava/Hill", "Lava or hill"),
        new('*', "Forest", "Forest"),
        new('H', "House", "House wall"),
        new('S', "Shop", "Shop floor"),
        new('>', "Stairs Down", "Stairs down"),
        new('<', "Stairs Up", "Stairs up"),
        new('E', "Enemy", "Enemy spawn"),
        new('C', "NPC", "NPC spawn"),
        new('P', "Player", "Player spawn"),
        new('I', "Item", "Item spawn"),
        new(' ', "Empty", "Empty space"),
    ];

    public MapEditorViewModel(IDataService dataService, IProjectService projectService)
    {
        _dataService = dataService;
        _projectService = projectService;
    }

    partial void OnSelectedTileTypeChanged(TileType? value)
    {
        if (value != null)
        {
            CurrentBrush = value.Character;
        }
    }

    [RelayCommand]
    private void SetTool(string? tool)
    {
        if (tool != null)
        {
            CurrentTool = tool;
            StatusMessage = $"Tool: {tool}";
        }
    }

    [RelayCommand]
    private void NewMap()
    {
        var tab = new MapTabViewModel("New Map", 40, 25);
        OpenMaps.Add(tab);
        SelectedMap = tab;
        StatusMessage = "Created new map";
    }

    [RelayCommand]
    public async Task LoadMapFilesAsync()
    {
        string? mapsDir = _projectService.GetMapsDirectory();
        if (mapsDir == null || !Directory.Exists(mapsDir))
        {
            StatusMessage = "No maps directory found";
            return;
        }

        var files = Directory.GetFiles(mapsDir, "*.txt");
        MapFiles = new ObservableCollection<string>(files.Select(Path.GetFileName)!);
        StatusMessage = $"Found {MapFiles.Count} map files";
    }

    [RelayCommand]
    private async Task OpenMapFile(string? fileName)
    {
        if (fileName == null) return;

        // 检查是否已经打开
        var existing = OpenMaps.FirstOrDefault(t => t.FileName == fileName);
        if (existing != null)
        {
            SelectedMap = existing;
            StatusMessage = $"Switched to: {fileName}";
            return;
        }

        // 打开新地图
        string? mapsDir = _projectService.GetMapsDirectory();
        if (mapsDir == null) return;

        string filePath = Path.Combine(mapsDir, fileName);
        if (!File.Exists(filePath))
        {
            StatusMessage = $"File not found: {filePath}";
            return;
        }

        try
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0) return;

            int height = lines.Length;
            int width = lines.Max(l => l.Length);
            string name = Path.GetFileNameWithoutExtension(fileName);

            var tab = new MapTabViewModel(name, width, height, fileName);

            for (int y = 0; y < height; y++)
            {
                string line = lines[y];
                for (int x = 0; x < width; x++)
                {
                    tab.MapData[x, y] = x < line.Length ? line[x] : ' ';
                }
            }

            OpenMaps.Add(tab);
            SelectedMap = tab;
            StatusMessage = $"Opened: {name} ({width}x{height})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CloseMap(MapTabViewModel? tab)
    {
        if (tab == null) return;

        OpenMaps.Remove(tab);
        if (SelectedMap == tab)
        {
            SelectedMap = OpenMaps.LastOrDefault();
        }
        StatusMessage = $"Closed: {tab.Name}";
    }

    [RelayCommand]
    private async Task SaveCurrentMap()
    {
        if (SelectedMap == null)
        {
            StatusMessage = "No map selected";
            return;
        }

        string? mapsDir = _projectService.GetMapsDirectory();
        if (mapsDir == null)
        {
            StatusMessage = "No maps directory";
            return;
        }

        try
        {
            Directory.CreateDirectory(mapsDir);
            string fileName = $"{SelectedMap.Name}.txt";
            string filePath = Path.Combine(mapsDir, fileName);

            var lines = new List<string>();
            for (int y = 0; y < SelectedMap.MapHeight; y++)
            {
                var line = new char[SelectedMap.MapWidth];
                for (int x = 0; x < SelectedMap.MapWidth; x++)
                {
                    line[x] = SelectedMap.MapData[x, y];
                }
                lines.Add(new string(line));
            }

            await File.WriteAllLinesAsync(filePath, lines);

            SelectedMap.FileName = fileName;
            SelectedMap.IsDirty = false;

            await LoadMapFilesAsync();
            StatusMessage = $"Saved: {fileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GenerateRandomMap()
    {
        if (SelectedMap == null) return;

        SelectedMap.GenerateRandomMap();
        StatusMessage = "Generated random map";
    }

    public void HandleTileAction(int x, int y)
    {
        if (SelectedMap == null) return;

        switch (CurrentTool)
        {
            case "Paint":
                SelectedMap.SetTileAt(x, y, CurrentBrush);
                break;
            case "Fill":
                SelectedMap.FloodFill(x, y, CurrentBrush);
                break;
            case "PlaceEntity":
                PlaceEntity(x, y);
                break;
            case "SelectEntity":
                SelectEntityAt(x, y);
                break;
        }
    }

    private void PlaceEntity(int x, int y)
    {
        if (SelectedMap == null || SelectedEntityType == null) return;

        MapEntity entity = SelectedEntityType.Value switch
        {
            MapEntityType.Enemy => new EnemyEntity { X = x, Y = y, Id = $"enemy_{SelectedMap.Entities.Count}", Name = "New Enemy" },
            MapEntityType.Npc => new NpcEntity { X = x, Y = y, Id = $"npc_{SelectedMap.Entities.Count}", Name = "New NPC" },
            MapEntityType.Player => new PlayerSpawnEntity { X = x, Y = y, Id = $"player_{SelectedMap.Entities.Count}", Name = "Player Spawn" },
            MapEntityType.Item => new ItemEntity { X = x, Y = y, Id = $"item_{SelectedMap.Entities.Count}", Name = "New Item" },
            MapEntityType.Trigger => new TriggerEntity { X = x, Y = y, Id = $"trigger_{SelectedMap.Entities.Count}", Name = "New Trigger" },
            _ => throw new ArgumentException($"Unknown entity type: {SelectedEntityType}")
        };

        SelectedMap.AddEntity(entity);
        EditingEntity = entity;
        StatusMessage = $"Placed {entity.EntityType} at ({x},{y})";
    }

    private void SelectEntityAt(int x, int y)
    {
        if (SelectedMap == null) return;

        var entity = SelectedMap.GetEntityAt(x, y);
        SelectedMap.SelectedEntity = entity;
        EditingEntity = entity;

        if (entity != null)
        {
            StatusMessage = $"Selected {entity.EntityType}: {entity.Name}";
        }
    }

    [RelayCommand]
    private void DeleteSelectedEntity()
    {
        if (SelectedMap?.SelectedEntity == null) return;

        var entity = SelectedMap.SelectedEntity;
        SelectedMap.RemoveEntity(entity);
        EditingEntity = null;
        StatusMessage = $"Deleted {entity.EntityType}: {entity.Name}";
    }
}

// 单个地图标签页的 ViewModel
public partial class MapTabViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string? _fileName;

    [ObservableProperty]
    private int _mapWidth;

    [ObservableProperty]
    private int _mapHeight;

    [ObservableProperty]
    private int _cursorX;

    [ObservableProperty]
    private int _cursorY;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private ObservableCollection<MapEntity> _entities = [];

    [ObservableProperty]
    private MapEntity? _selectedEntity;

    public char[,] MapData { get; private set; }

    public MapTabViewModel(string name, int width, int height, string? fileName = null)
    {
        _name = name;
        _fileName = fileName;
        _mapWidth = width;
        _mapHeight = height;
        MapData = new char[width, height];

        // 初始化为地板
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                MapData[x, y] = '.';
            }
        }
    }

    public void SetTileAt(int x, int y, char character)
    {
        if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
        {
            MapData[x, y] = character;
            IsDirty = true;
            OnPropertyChanged(nameof(MapData));
        }
    }

    public char GetTileAt(int x, int y)
    {
        if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
        {
            return MapData[x, y];
        }
        return ' ';
    }

    public void FloodFill(int startX, int startY, char newChar)
    {
        if (startX < 0 || startX >= MapWidth || startY < 0 || startY >= MapHeight) return;

        char originalChar = MapData[startX, startY];
        if (originalChar == newChar) return;

        var stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) continue;
            if (MapData[x, y] != originalChar) continue;

            MapData[x, y] = newChar;

            stack.Push((x - 1, y));
            stack.Push((x + 1, y));
            stack.Push((x, y - 1));
            stack.Push((x, y + 1));
        }

        IsDirty = true;
        OnPropertyChanged(nameof(MapData));
    }

    public void GenerateRandomMap()
    {
        var random = new Random();

        // Fill with walls
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                MapData[x, y] = '#';
            }
        }

        // Generate rooms
        int roomCount = random.Next(4, 8);
        var rooms = new List<(int x, int y, int w, int h)>();

        for (int i = 0; i < roomCount; i++)
        {
            int w = random.Next(4, 10);
            int h = random.Next(4, 8);
            int x = random.Next(1, MapWidth - w - 1);
            int y = random.Next(1, MapHeight - h - 1);

            bool overlap = false;
            foreach (var room in rooms)
            {
                if (x < room.x + room.w + 2 && x + w + 2 > room.x &&
                    y < room.y + room.h + 2 && y + h + 2 > room.y)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                rooms.Add((x, y, w, h));
                for (int ry = y; ry < y + h; ry++)
                {
                    for (int rx = x; rx < x + w; rx++)
                    {
                        MapData[rx, ry] = '.';
                    }
                }
            }
        }

        // Connect rooms
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var (x1, y1, w1, h1) = rooms[i];
            var (x2, y2, w2, h2) = rooms[i + 1];

            int cx1 = x1 + w1 / 2;
            int cy1 = y1 + h1 / 2;
            int cx2 = x2 + w2 / 2;
            int cy2 = y2 + h2 / 2;

            for (int x = Math.Min(cx1, cx2); x <= Math.Max(cx1, cx2); x++)
            {
                MapData[x, cy1] = '.';
            }
            for (int y = Math.Min(cy1, cy2); y <= Math.Max(cy1, cy2); y++)
            {
                MapData[cx2, y] = '.';
            }
        }

        // Add doors
        foreach (var (x, y, w, h) in rooms)
        {
            if (y > 0 && MapData[x + w / 2, y - 1] == '.')
                MapData[x + w / 2, y - 1] = '+';
            if (y + h < MapHeight && MapData[x + w / 2, y + h] == '.')
                MapData[x + w / 2, y + h] = '+';
        }

        IsDirty = true;
        OnPropertyChanged(nameof(MapData));
    }

    // 实体管理方法
    public void AddEntity(MapEntity entity)
    {
        Entities.Add(entity);
        IsDirty = true;
    }

    public void RemoveEntity(MapEntity entity)
    {
        Entities.Remove(entity);
        if (SelectedEntity == entity)
        {
            SelectedEntity = null;
        }
        IsDirty = true;
    }

    public MapEntity? GetEntityAt(int x, int y)
    {
        return Entities.FirstOrDefault(e => e.X == x && e.Y == y);
    }

    // 序列化实体数据
    public string SerializeEntities()
    {
        var data = Entities.Select(e => new
        {
            type = e.EntityType.ToString(),
            x = e.X,
            y = e.Y,
            id = e.Id,
            name = e.Name,
            props = e switch
            {
                EnemyEntity enemy => new { enemyTypeId = enemy.EnemyTypeId, patrolRoute = enemy.PatrolRoute, isStatic = enemy.IsStatic },
                NpcEntity npc => new { dialogueId = npc.DialogueId, faction = npc.Faction, schedule = npc.Schedule },
                PlayerSpawnEntity player => new { playerIndex = player.PlayerIndex },
                ItemEntity item => new { itemId = item.ItemId, quantity = item.Quantity, isHidden = item.IsHidden },
                TriggerEntity trigger => new { triggerType = trigger.TriggerType, action = trigger.Action, actionParameters = trigger.ActionParameters, isOneTime = trigger.IsOneTime, condition = trigger.Condition },
                _ => (object)new { }
            }
        });

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }
}

public record TileType(char Character, string Name, string Description);
