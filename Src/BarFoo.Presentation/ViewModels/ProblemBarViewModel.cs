using Avalonia.Controls.Notifications;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BarFoo.Presentation.ViewModels;

public partial class ProblemBarViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _negativeMessage = string.Empty;

    [ObservableProperty]
    private bool _isStatusActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStatusActive))]
    private NotificationType _statusType = NotificationType.Information;

    [ObservableProperty]
    private bool _isVisible;

    public ProblemBarViewModel() { }

    public void UpdateStatus(string message, NotificationType type)
    {
        StatusType = type;
        NegativeMessage = message;
        IsStatusActive = true;
        IsVisible = true;
    }

    [RelayCommand]
    public void Close()
    {
        ClearStatus();
    }

    private void ClearStatus()
    {
        NegativeMessage = string.Empty;
        IsStatusActive = false;
        IsVisible = false;
        StatusType = 0;
    }
}