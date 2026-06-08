using CommunityToolkit.Mvvm.ComponentModel;
using HHSEditor.Core.Services;

namespace HHSEditor.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "HHS Game Editor";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private CatalogEditorViewModel _catalogEditor;

    [ObservableProperty]
    private EnemyEditorViewModel _enemyEditor;

    public MainWindowViewModel()
    {
        var dataService = new JsonDataService();
        var projectService = new ProjectService();

        _catalogEditor = new CatalogEditorViewModel(dataService, projectService);
        _enemyEditor = new EnemyEditorViewModel(dataService, projectService);
    }
}
