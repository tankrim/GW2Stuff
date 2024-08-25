using System.Collections.ObjectModel;

using Avalonia.Threading;

using BarFoo.Infrastructure.DTOs;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BarFoo.Presentation.ViewModels;

public partial class StatusBarViewModel : ViewModelBase, IRecipient<ObjectivesChangedMessage>
{
    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _loadedObjectives;

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _filteredObjectives;

    private readonly DispatcherTimer _timer;

    public StatusBarViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += Timer_Tick;
        LoadedObjectives = [];
        FilteredObjectives = [];

        // Register for messages
        WeakReferenceMessenger.Default.Register<ObjectivesChangedMessage>(this);
    }

    public void Receive(ObjectivesChangedMessage message)
    {
        if (message.Value.PropertyName == nameof(ObjectivesViewModel.Objectives))
        {
            LoadedObjectives = message.Value.Value;
        }
        if (message.Value.PropertyName == nameof(ObjectivesViewModel.FilteredObjectives))
        {
            FilteredObjectives = message.Value.Value;
        }
    }

    public void SetIsUpdatingTemporarily()
    {
        IsUpdating = true;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        IsUpdating = false;
        _timer.Stop();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
        WeakReferenceMessenger.Default.Unregister<ObjectivesChangedMessage>(this);
    }
}