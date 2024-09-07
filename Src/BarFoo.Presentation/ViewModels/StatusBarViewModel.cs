using Avalonia.Controls.Notifications;
using Avalonia.Threading;

using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;

using CommunityToolkit.Mvvm.ComponentModel;

namespace BarFoo.Presentation.ViewModels;

public partial class StatusBarViewModel : ViewModelBase, IDisposable
{
    private readonly IMessagingService _messagingService;
    private readonly DispatcherTimer _clearTimer;
    private readonly TimeSpan _clearDelay = TimeSpan.FromSeconds(5);

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private int _loadedObjectivesCount;

    [ObservableProperty]
    private int _filteredObjectivesCount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isStatusActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStatusActive))]
    private NotificationType _statusType = NotificationType.Information;


    public StatusBarViewModel(IMessagingService messagingService)
    {
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));


        _clearTimer = new DispatcherTimer
        {
            Interval = _clearDelay
        };
        _clearTimer.Tick += ClearTimer_Tick;

        _messagingService.Register<ObjectiveMessages.ObjectivesChangedMessage>(this, HandleObjectivesChanged);
    }

    private void HandleObjectivesChanged(object recipient, ObjectiveMessages.ObjectivesChangedMessage message)
    {
        if (message.Value.PropertyName == nameof(ObjectivesViewModel.Objectives))
        {
            LoadedObjectivesCount = message.Value.Value.Count;
        }
        if (message.Value.PropertyName == nameof(ObjectivesViewModel.FilteredObjectives))
        {
            FilteredObjectivesCount = message.Value.Value.Count;
        }
    }

    private void ClearTimer_Tick(object? sender, EventArgs e)
    {
        ClearStatus();
    }

    public void UpdateStatus(string message, NotificationType type)
    {
        StatusMessage = message;
        StatusType = type;
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
        StatusMessage = string.Empty;
        IsStatusActive = false;
        StatusType = 0;
        IsUpdating = false;
        _clearTimer.Stop();
    }

    public void Dispose()
    {
        _clearTimer.Stop();
        _clearTimer.Tick -= ClearTimer_Tick;
        _messagingService.Unregister<ObjectiveMessages.ObjectivesChangedMessage>(this);
        _messagingService.Unregister<IsUpdatingMessage>(this);
        GC.SuppressFinalize(this);
    }
}