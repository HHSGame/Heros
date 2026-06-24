using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSGame.Core;
using HHSGame.Core.Engine;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Rendering;
using HHSEditor.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace HHSEditor.ViewModels;

/// <summary>
/// 游戏窗口 ViewModel - 使用真实游戏引擎
/// </summary>
public partial class GameWindowViewModel : ObservableObject
{
    private readonly GameDataLoader _dataLoader;
    private readonly GameConfigLoader _configLoader;
    private readonly GameCatalogLoader _catalogLoader;

    private Game? _game;
    private GameContext? _context;
    private ServiceProvider? _services;

    [ObservableProperty]
    private string _title = "HHS Game";

    [ObservableProperty]
    private int _playerHP;

    [ObservableProperty]
    private int _playerMaxHP;

    [ObservableProperty]
    private int _playerAP;

    [ObservableProperty]
    private int _turnNumber;

    [ObservableProperty]
    private string _statusMessage = "Loading...";

    [ObservableProperty]
    private ObservableCollection<string> _messages = [];

    [ObservableProperty]
    private ObservableCollection<string> _inventoryItems = [];

    [ObservableProperty]
    private GameCanvasViewModel _gameCanvas;

    [ObservableProperty]
    private bool _isGameLoaded;

    public GameWindowViewModel()
    {
        _dataLoader = new GameDataLoader(Microsoft.Extensions.Logging.Abstractions.NullLogger<GameDataLoader>.Instance);
        _configLoader = new GameConfigLoader(_dataLoader);
        _catalogLoader = new GameCatalogLoader(_dataLoader);
        _gameCanvas = new GameCanvasViewModel();
    }

    /// <summary>
    /// 加载游戏配置
    /// </summary>
    public async Task<bool> LoadGameAsync(string configPath)
    {
        try
        {
            StatusMessage = $"Loading game from: {configPath}";

            // 加载配置
            GameConfig config = _configLoader.Load(configPath);
            GameCatalog catalogs = _catalogLoader.Load(config.Catalogs);
            GameParameters parameters = new GameConfigMapper(catalogs).ToParameters(config);

            // 创建服务容器
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHHSGameCore(parameters, catalogs);
            _services = services.BuildServiceProvider();

            // 获取游戏实例
            _game = _services.GetRequiredService<Game>();
            _context = _game.Context;

            // 订阅事件
            Events.OnGameMessageEvent += OnGameMessage;

            // 启动游戏
            _game.Start();

            IsGameLoaded = true;
            StatusMessage = "Game loaded successfully";
            UpdateGameState();

            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading game: {ex.Message}";
            Console.WriteLine($"[GameWindow] Error: {ex}");
            return false;
        }
    }

    [RelayCommand]
    private void NewGame()
    {
        if (_game != null)
        {
            _game.Start();
            UpdateGameState();
            Messages.Clear();
            AddMessage("New game started!");
        }
    }

    [RelayCommand]
    private void SaveGame()
    {
        AddMessage("Save game not implemented yet.");
    }

    [RelayCommand]
    private void ShowHelp()
    {
        AddMessage("=== Controls ===");
        AddMessage("Arrow Keys: Move");
        AddMessage("Space: Wait");
        AddMessage("Tab: Switch player");
        AddMessage("Escape: Menu");
    }

    public void HandleKeyPress(Key key)
    {
        if (_game == null || !IsGameLoaded) return;

        bool handled = key switch
        {
            Key.Up => TryMove(0, -1),
            Key.Down => TryMove(0, 1),
            Key.Left => TryMove(-1, 0),
            Key.Right => TryMove(1, 0),
            Key.Space => TryWait(),
            _ => false
        };

        if (handled)
        {
            UpdateGameState();
        }
    }

    private bool TryMove(int dx, int dy)
    {
        if (_game?.Player == null) return false;

        // 使用 Game.TryMovePlayer 方法
        var move = CreateMove(dx, dy);
        if (move == null) return false;

        return _game.TryMovePlayer(move, GameStateType.Exploration, GameStateType.Combat);
    }

    private HHSGame.Core.Move? CreateMove(int dx, int dy)
    {
        // 根据方向创建 Move 对象
        if (dx == 0 && dy == -1) return new HHSGame.Core.Move.Forward(Direction.Up);
        if (dx == 0 && dy == 1) return new HHSGame.Core.Move.Forward(Direction.Down);
        if (dx == -1 && dy == 0) return new HHSGame.Core.Move.Forward(Direction.Left);
        if (dx == 1 && dy == 0) return new HHSGame.Core.Move.Forward(Direction.Right);
        return null;
    }

    private bool TryWait()
    {
        if (_game?.Player == null) return false;

        // 使用 Game 的方法等待一回合
        // 简单实现：移动到原地
        return true;
    }

    private void UpdateGameState()
    {
        if (_context?.Player == null) return;

        PlayerHP = _context.Player.Stats.CurrentHp;
        PlayerMaxHP = _context.Player.Stats.MaxHp;
        PlayerAP = _context.Player.Stats.CurrentAp;
        TurnNumber = _context.TurnManager.GetTurnNumber();
        StatusMessage = $"Position: ({_context.Player.X}, {_context.Player.Y})";

        // 更新画布
        UpdateCanvas();

        // 更新物品栏
        UpdateInventory();
    }

    private void UpdateCanvas()
    {
        if (_context == null) return;

        GameCanvas.MapWidth = _context.MapState.Width;
        GameCanvas.MapHeight = _context.MapState.Height;

        // 复制地图数据
        var mapData = new char[_context.MapState.Width, _context.MapState.Height];
        for (int y = 0; y < _context.MapState.Height; y++)
        {
            for (int x = 0; x < _context.MapState.Width; x++)
            {
                mapData[x, y] = _context.MapState.GetCell(x, y).Character;
            }
        }
        GameCanvas.MapData = mapData;

        // 收集实体
        var entities = new List<EntityRenderData>();

        // 添加玩家
        if (_context.Player != null)
        {
            entities.Add(new EntityRenderData
            {
                X = _context.Player.X,
                Y = _context.Player.Y,
                Glyph = '@',
                Name = _context.Player.Name,
                Type = "Player"
            });
        }

        // 添加敌人
        foreach (var enemy in _context.EnemyManager.Enemies)
        {
            entities.Add(new EntityRenderData
            {
                X = enemy.X,
                Y = enemy.Y,
                Glyph = enemy.Glyph,
                Name = enemy.Name,
                Type = "Enemy"
            });
        }

        // 添加 NPC
        foreach (var npc in _context.NpcManager.Npcs)
        {
            entities.Add(new EntityRenderData
            {
                X = npc.X,
                Y = npc.Y,
                Glyph = npc.Glyph,
                Name = npc.Name,
                Type = "NPC"
            });
        }

        GameCanvas.Entities = entities;
    }

    private void UpdateInventory()
    {
        if (_context?.InventoryManager == null) return;

        InventoryItems.Clear();
        foreach (var item in _context.InventoryManager.GetItems())
        {
            InventoryItems.Add($"{item.Name} ({item.Rarity})");
        }
    }

    private void OnGameMessage(object? sender, GameMessageEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            AddMessage(e.Message);
        });
    }

    private void AddMessage(string message)
    {
        Messages.Add(message);
        if (Messages.Count > 100)
        {
            Messages.RemoveAt(0);
        }
    }

    public void Dispose()
    {
        Events.OnGameMessageEvent -= OnGameMessage;
        _services?.Dispose();
    }
}

/// <summary>
/// 游戏画布 ViewModel
/// </summary>
public partial class GameCanvasViewModel : ObservableObject
{
    [ObservableProperty]
    private int _mapWidth = 40;

    [ObservableProperty]
    private int _mapHeight = 25;

    [ObservableProperty]
    private char[,] _mapData = new char[40, 25];

    [ObservableProperty]
    private List<EntityRenderData> _entities = [];
}
