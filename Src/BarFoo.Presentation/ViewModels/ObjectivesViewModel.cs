using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using BarFoo.Core.Services;
using BarFoo.Infrastructure.DTOs;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ObjectivesViewModel : ViewModelBase, IRecipient<FilterChangedMessage>, IRecipient<ApiKeyUpdatedMessage>
{
    private readonly IStore _store;
    private readonly ILogger<ObjectivesViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _objectives;

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _filteredObjectives;

    public ObjectivesViewModel(
        IStore store,
        ILogger<ObjectivesViewModel> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _objectives = new ObservableCollection<ObjectiveWithOthersDto>();
        _filteredObjectives = new ObservableCollection<ObjectiveWithOthersDto>();

        WeakReferenceMessenger.Default.Register<FilterChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<ApiKeyUpdatedMessage>(this);

        Objectives.CollectionChanged += (_, _) => OnObjectivesChanged(Objectives);
        FilteredObjectives.CollectionChanged += (_, _) => OnFilteredObjectivesChanged(FilteredObjectives);
    }

    public async Task LoadObjectivesAsync()
    {
        try
        {
            var objectives = await _store.GetAllObjectivesWithOthersAsync();
            Objectives.Clear();
            foreach (var objective in objectives)
            {
                Objectives.Add(objective);
            }
            FilterObjectives(null);
            _logger.LogInformation("Loaded {Count} objectives", Objectives.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading objectives");
        }
    }

    public void Receive(FilterChangedMessage message)
    {
        FilterObjectives(message.Value);
    }

    public async void Receive(ApiKeyUpdatedMessage message)
    {
        await LoadObjectivesAsync();
    }

    private void FilterObjectives(FilterViewModel? filter)
    {
        IEnumerable<ObjectiveWithOthersDto> filtered = Objectives;

        if (filter != null)
        {
            filtered = filtered.Where(o =>
                (!filter.FilterDaily || o.ApiEndpoint == "daily") &&
                (!filter.FilterWeekly || o.ApiEndpoint == "weekly") &&
                (!filter.FilterSpecial || o.ApiEndpoint == "special") &&
                (!filter.FilterCompleted || o.Claimed) &&
                (!filter.FilterPvE || o.Track == "PvE") &&
                (!filter.FilterPvP || o.Track == "PvP") &&
                (!filter.FilterWvW || o.Track == "WvW") &&
                filter.ApiKeyFilters.Any(af => af.IsSelected && af.ApiKeyName == o.ApiKeyName)
            );
        }

        FilteredObjectives.Clear();
        foreach (var objective in filtered)
        {
            FilteredObjectives.Add(objective);
        }
    }

    partial void OnObjectivesChanged(ObservableObject value);
    partial void OnObjectivesChanged(ObservableCollection<ObjectiveWithOthersDto> value)
    {
        WeakReferenceMessenger.Default.Send(new ObjectivesChangedMessage(nameof(Objectives), value));
    }

    partial void OnFilteredObjectivesChanged(ObservableObject value);
    partial void OnFilteredObjectivesChanged(ObservableCollection<ObjectiveWithOthersDto> value)
    {
        WeakReferenceMessenger.Default.Send(new ObjectivesChangedMessage(nameof(FilteredObjectives), value));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<ObjectivesChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ApiKeyUpdatedMessage>(this);
    }
}

public sealed class ObjectivesChangedMessage : ValueChangedMessage<(string PropertyName, ObservableCollection<ObjectiveWithOthersDto> Value)>
{
    public ObjectivesChangedMessage(string propertyName, ObservableCollection<ObjectiveWithOthersDto> value)
        : base((propertyName, value))
    {
    }
}