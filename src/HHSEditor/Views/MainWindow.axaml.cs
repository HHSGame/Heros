using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HHSEditor.ViewModels;

namespace HHSEditor.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        _viewModel = (MainWindowViewModel)DataContext;
    }

    private async void OnNewProject(object? sender, RoutedEventArgs e)
    {
        try
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Project Directory",
                AllowMultiple = false
            });

            if (folders.Count > 0 && _viewModel != null)
            {
                string projectPath = folders[0].Path.LocalPath;
                await _viewModel.CreateProjectFromPathAsync(projectPath);
            }
        }
        catch (Exception ex)
        {
            if (_viewModel != null)
            {
                _viewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }
    }

    private async void OnOpenProject(object? sender, RoutedEventArgs e)
    {
        try
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Project Directory",
                AllowMultiple = false
            });

            if (folders.Count > 0 && _viewModel != null)
            {
                string projectPath = folders[0].Path.LocalPath;
                await _viewModel.OpenProjectFromPathAsync(projectPath);
            }
        }
        catch (Exception ex)
        {
            if (_viewModel != null)
            {
                _viewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }
    }

    private async void OnOpenGameData(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_viewModel != null)
            {
                await _viewModel.OpenBuiltinGameDataAsync();
            }
        }
        catch (Exception ex)
        {
            if (_viewModel != null)
            {
                _viewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }
    }

    private async void OnSaveAll(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_viewModel != null)
            {
                await _viewModel.SaveAllAsync();
            }
        }
        catch (Exception ex)
        {
            if (_viewModel != null)
            {
                _viewModel.StatusMessage = $"Error: {ex.Message}";
            }
        }
    }

    private async void OnPlayGame(object? sender, RoutedEventArgs e)
    {
        try
        {
            var gameWindow = new GameWindow();
            gameWindow.Show();

            // 查找游戏配置
            string? configPath = FindGameConfigPath();
            if (configPath != null)
            {
                await gameWindow.LoadGameAsync(configPath);
            }
            else
            {
                _viewModel.StatusMessage = "No game config found. Please load game data first.";
            }
        }
        catch (Exception ex)
        {
            if (_viewModel != null)
            {
                _viewModel.StatusMessage = $"Error opening game: {ex.Message}";
            }
        }
    }

    private string? FindGameConfigPath()
    {
        string? projectPath = _viewModel?.CurrentProjectPath;
        if (projectPath == null)
        {
            // 尝试默认路径
            string defaultPath = "/Users/qliu23/workspace/fe/Slime/data/game.json";
            if (File.Exists(defaultPath))
            {
                return defaultPath;
            }
            return null;
        }

        string[] possiblePaths =
        [
            Path.Combine(projectPath, "game.json"),
            Path.Combine(projectPath, "data", "game.json")
        ];

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private void OnExit(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
