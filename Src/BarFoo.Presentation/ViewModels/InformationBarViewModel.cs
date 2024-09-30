using Avalonia.Controls.Notifications;
using Avalonia.Threading;

using BarFoo.Core.Messages;

using CommunityToolkit.Mvvm.ComponentModel;

namespace BarFoo.Presentation.ViewModels;

public partial class InformationBarViewModel : ViewModelBase, IDisposable
{
    private readonly DispatcherTimer _clearTimer;
    private readonly TimeSpan _clearDelay = TimeSpan.FromSeconds(7);

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private string _updatingMessage = string.Empty;

    [ObservableProperty]
    private string _positiveMessage = string.Empty;

    [ObservableProperty]
    private bool _isStatusActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStatusActive))]
    private NotificationType _statusType = NotificationType.Information;

    public InformationBarViewModel()
    {
        _clearTimer = new DispatcherTimer
        {
            Interval = _clearDelay
        };
        _clearTimer.Tick += ClearTimer_Tick;
    }

    private void ClearTimer_Tick(object? sender, EventArgs e)
    {
        ClearStatus();
    }

    public void UpdateStatus(string message, NotificationType type)
    {
        StatusType = type;

        switch (message)
        {
            case "Updating...":
                UpdatingMessage = message;
                break;
            default:
                PositiveMessage = message;
                break;
        }

        IsStatusActive = true;
        RestartClearTimer();
    }

    private void RestartClearTimer()
    {
        _clearTimer.Stop();
        _clearTimer.Start();
    }

    private void ClearStatus()
    {
        UpdatingMessage = string.Empty;
        PositiveMessage = string.Empty;
        IsStatusActive = false;
        StatusType = 0;
        IsUpdating = false;
        _clearTimer.Stop();
    }

    public void Dispose()
    {
        _clearTimer.Stop();
        _clearTimer.Tick -= ClearTimer_Tick;
        GC.SuppressFinalize(this);
    }
}