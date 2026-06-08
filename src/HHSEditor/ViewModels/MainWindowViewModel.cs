using CommunityToolkit.Mvvm.ComponentModel;

namespace HHSEditor.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "HHS Game Editor";

    [ObservableProperty]
    private string _statusMessage = "Ready";
}
