using Avalonia.Threading;

using BarFoo.Core.Messages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BarFoo.Presentation.ViewModels;

public partial class StatusBarViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private int _loadedObjectivesCount;

    [ObservableProperty]
    private int _filteredObjectivesCount;

    private readonly DispatcherTimer _timer;

    public StatusBarViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += Timer_Tick;

        WeakReferenceMessenger.Default.Register<ObjectiveMessages.ObjectivesChangedMessage>(this, HandleObjectivesChanged);
        WeakReferenceMessenger.Default.Register<IsUpdatingMessage>(this, HandleIsUpdating);
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

    private void HandleIsUpdating(object recipient, IsUpdatingMessage message)
    {
        SetIsUpdatingTemporarily();
    }

    public void SetIsUpdatingTemporarily()
    {
        IsUpdating = true;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        IsUpdating = false;
        _timer.Stop();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
        WeakReferenceMessenger.Default.Unregister<ObjectiveMessages.ObjectivesChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<IsUpdatingMessage>(this);
        GC.SuppressFinalize(this);
    }
}