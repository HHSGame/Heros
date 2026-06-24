using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using HHSEditor.ViewModels;

namespace HHSEditor.Views;

public partial class MapEditorView : UserControl
{
    public MapEditorView()
    {
        InitializeComponent();
    }

    private void OnTileAction(object? sender, (int X, int Y) e)
    {
        if (DataContext is MapEditorViewModel viewModel)
        {
            viewModel.HandleTileAction(e.X, e.Y);
            if (viewModel.SelectedMap != null)
            {
                viewModel.SelectedMap.CursorX = e.X;
                viewModel.SelectedMap.CursorY = e.Y;
            }
        }
    }

    private void OnCloseTabClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is MapTabViewModel tab)
        {
            if (DataContext is MapEditorViewModel viewModel)
            {
                viewModel.CloseMapCommand.Execute(tab);
            }
        }
    }

    private void OnMapFileDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is string fileName)
        {
            if (DataContext is MapEditorViewModel viewModel)
            {
                viewModel.OpenMapFileCommand.Execute(fileName);
            }
        }
    }
}
