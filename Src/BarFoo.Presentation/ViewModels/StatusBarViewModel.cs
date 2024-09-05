using System.Collections.ObjectModel;

using Avalonia.Threading;

using BarFoo.Core.DTOs;
using BarFoo.Infrastructure.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

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

        WeakReferenceMessenger.Default.Register<ObjectivesChangedMessage>(this, HandleObjectivesChanged);
        WeakReferenceMessenger.Default.Register<IsUpdatingMessage>(this, HandleIsUpdating);
    }

    private void HandleObjectivesChanged(object recipient, ObjectivesChangedMessage message)
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
        WeakReferenceMessenger.Default.Unregister<ObjectivesChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<IsUpdatingMessage>(this);
        GC.SuppressFinalize(this);
    }
}

public sealed class ObjectivesChangedMessage : ValueChangedMessage<(string PropertyName, ObservableCollection<ObjectiveWithOthersDto> Value)>
{
    public ObjectivesChangedMessage(string propertyName, ObservableCollection<ObjectiveWithOthersDto> value)
        : base((propertyName, value)) { }
}