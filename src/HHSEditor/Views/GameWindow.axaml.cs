using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using HHSEditor.ViewModels;

namespace HHSEditor.Views;

public partial class GameWindow : Window
{
    private GameWindowViewModel? _viewModel;

    public GameWindow()
    {
        InitializeComponent();
        _viewModel = new GameWindowViewModel();
        DataContext = _viewModel;
    }

    /// <summary>
    /// 使用指定配置路径加载游戏
    /// </summary>
    public async Task LoadGameAsync(string configPath)
    {
        if (_viewModel != null)
        {
            await _viewModel.LoadGameAsync(configPath);
        }
    }

    private void OnExit(object? sender, RoutedEventArgs e)
    {
        _viewModel?.Dispose();
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_viewModel != null)
        {
            _viewModel.HandleKeyPress(e.Key);
            e.Handled = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel?.Dispose();
        base.OnClosed(e);
    }
}
