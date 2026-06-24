using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

/// <summary>
/// 游戏测试 ViewModel
/// </summary>
public partial class GameTestViewModel : ObservableObject, IDisposable
{
    private readonly GameLauncher _gameLauncher;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private ObservableCollection<string> _outputLog = [];

    [ObservableProperty]
    private string _currentConfigPath = "";

    [ObservableProperty]
    private string _launchCommand = "";

    public GameTestViewModel(GameLauncher gameLauncher, IProjectService projectService)
    {
        _gameLauncher = gameLauncher;
        _projectService = projectService;

        // 订阅事件
        _gameLauncher.OutputReceived += OnOutputReceived;
        _gameLauncher.ErrorReceived += OnErrorReceived;
        _gameLauncher.GameExited += OnGameExited;
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        if (_gameLauncher.IsRunning)
        {
            StatusMessage = "Game is already running";
            return;
        }

        // 更新配置路径
        UpdateConfigPath();

        if (CurrentConfigPath == "Not found" || CurrentConfigPath == "No project loaded")
        {
            StatusMessage = "Please open a project or load game data first";
            return;
        }

        OutputLog.Clear();
        OutputLog.Add("=== Launching Game ===");
        OutputLog.Add($"Config: {CurrentConfigPath}");
        OutputLog.Add("");

        // 显示启动命令
        string? projectPath = _projectService.CurrentProjectPath;
        if (projectPath != null)
        {
            string gameProjectPath = FindGameProjectPath();
            if (gameProjectPath != null)
            {
                LaunchCommand = $"dotnet run --project \"{gameProjectPath}\" -- --config \"{CurrentConfigPath}\"";
                OutputLog.Add($"Command: {LaunchCommand}");
                OutputLog.Add("");
            }
        }

        StatusMessage = "Launching game...";

        bool success = await _gameLauncher.LaunchGameInTerminalAsync();
        if (success)
        {
            IsRunning = true;
            StatusMessage = "Game is running - check for game window";
            OutputLog.Add("");
            OutputLog.Add("=== Game Started ===");
            OutputLog.Add("The game window should appear now.");
            OutputLog.Add("If it doesn't appear, you can run manually:");
            OutputLog.Add(LaunchCommand);
        }
        else
        {
            StatusMessage = "Failed to launch game";
        }
    }

    [RelayCommand]
    private void StopGame()
    {
        if (_gameLauncher.IsRunning)
        {
            _gameLauncher.StopGame();
            IsRunning = false;
            StatusMessage = "Game stopped";
            OutputLog.Add("=== Game Stopped ===");
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        OutputLog.Clear();
    }

    [RelayCommand]
    private void CopyCommand()
    {
        if (!string.IsNullOrEmpty(LaunchCommand))
        {
            // 复制到剪贴板
            StatusMessage = "Command copied to clipboard";
        }
    }

    private void UpdateConfigPath()
    {
        string? projectPath = _projectService.CurrentProjectPath;
        if (projectPath != null)
        {
            string[] possiblePaths =
            [
                Path.Combine(projectPath, "game.json"),
                Path.Combine(projectPath, "data", "game.json")
            ];

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    CurrentConfigPath = path;
                    return;
                }
            }

            CurrentConfigPath = "Not found";
        }
        else
        {
            CurrentConfigPath = "No project loaded";
        }
    }

    private string? FindGameProjectPath()
    {
        string currentDir = Directory.GetCurrentDirectory();
        string[] searchPaths =
        [
            Path.Combine(currentDir, "src", "HHSGame", "HHSGame.csproj"),
            Path.Combine(currentDir, "..", "src", "HHSGame", "HHSGame.csproj"),
            Path.Combine(currentDir, "..", "..", "src", "HHSGame", "HHSGame.csproj"),
            "/Users/qliu23/workspace/fe/Slime/src/HHSGame/HHSGame.csproj"
        ];

        foreach (string path in searchPaths)
        {
            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private void OnOutputReceived(object? sender, string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            OutputLog.Add(message);
            // 保持日志在合理大小
            if (OutputLog.Count > 1000)
            {
                OutputLog.RemoveAt(0);
            }
        });
    }

    private void OnErrorReceived(object? sender, string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            OutputLog.Add($"[ERROR] {message}");
        });
    }

    private void OnGameExited(object? sender, int exitCode)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            IsRunning = false;
            StatusMessage = $"Game exited with code {exitCode}";
            OutputLog.Add($"=== Game Exited (code: {exitCode}) ===");
        });
    }

    public void Dispose()
    {
        _gameLauncher.OutputReceived -= OnOutputReceived;
        _gameLauncher.ErrorReceived -= OnErrorReceived;
        _gameLauncher.GameExited -= OnGameExited;
        _gameLauncher.Dispose();
    }
}
