using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using BarFoo.Core.Services;
using BarFoo.Infrastructure.DTOs;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ObjectivesViewModel : ViewModelBase, IDisposable
{
    private readonly IStore _store;
    private readonly ILogger<ObjectivesViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _objectives = new();

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _filteredObjectives = new();

    private FilterViewModel _currentFilter;

    public ObjectivesViewModel(
        FilterViewModel filterViewModel,
        IStore store,
        ILogger<ObjectivesViewModel> logger)
    {
        _currentFilter = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        WeakReferenceMessenger.Default.Register<FilterChangedMessage>(this, HandleFilterChanged);
        WeakReferenceMessenger.Default.Register<ApiKeyStateChangedMessage>(this, HandleApiKeyStateChanged);
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
            _logger.LogInformation("Loaded {Count} objectives", Objectives.Count);
            ApplyCurrentFilter();
            WeakReferenceMessenger.Default.Send(new ObjectivesChangedMessage(nameof(Objectives), Objectives));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading objectives");
        }
    }

    private void ApplyCurrentFilter()
    {
        try
        {
            var filtered = Objectives.Where(o =>
                (_currentFilter.FilterDaily && o.ApiEndpoint == "daily" ||
                 _currentFilter.FilterWeekly && o.ApiEndpoint == "weekly" ||
                 _currentFilter.FilterSpecial && o.ApiEndpoint == "special" ||
                 (!_currentFilter.FilterDaily && !_currentFilter.FilterWeekly && !_currentFilter.FilterSpecial)) &&
                (_currentFilter.FilterPvE && o.Track == "PvE" ||
                 _currentFilter.FilterPvP && o.Track == "PvP" ||
                 _currentFilter.FilterWvW && o.Track == "WvW" ||
                 (!_currentFilter.FilterPvE && !_currentFilter.FilterPvP && !_currentFilter.FilterWvW)) &&
                (!_currentFilter.FilterCompleted || !o.Claimed) &&
                _currentFilter.ApiKeyFilters.Any(af => af.IsSelected && af.ApiKeyName == o.ApiKeyName)
            ).ToList();

            _logger.LogDebug("ApplyCurrentFilter before clearing. Current count: {Count}", FilteredObjectives.Count);
            FilteredObjectives.Clear();
            foreach (var objective in filtered)
            {
                FilteredObjectives.Add(objective);
            }

            _logger.LogInformation("Filter applied. Filtered objectives count: {Count}", FilteredObjectives.Count);
            _logger.LogDebug("Filter state: daily={fd}, weekly={fw}, special={fs}, pve={fpve}, pvp={fpvp}, wvw={fwvw}, completed={fc}",
                _currentFilter.FilterDaily, _currentFilter.FilterWeekly, _currentFilter.FilterSpecial,
                _currentFilter.FilterPvE, _currentFilter.FilterPvP, _currentFilter.FilterWvW, _currentFilter.FilterCompleted);

            WeakReferenceMessenger.Default.Send(new ObjectivesChangedMessage(nameof(FilteredObjectives), FilteredObjectives));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while applying filter");
        }
    }

    private void HandleFilterChanged(object recipient, FilterChangedMessage message)
    {
        _logger.LogInformation("Received FilterChangedMessage. Applying filter...");
        _logger.LogDebug("Received FilterChangedMessage state: daily={fd}, weekly={fw}, special={fs}, pve={fpve}, pvp={fpvp}, wvw={fwvw}, completed={fc}", message.Value.FilterDaily, message.Value.FilterWeekly, message.Value.FilterSpecial, message.Value.FilterPvE, message.Value.FilterPvP, message.Value.FilterWvW, message.Value.FilterCompleted);
        ApplyCurrentFilter();
        WeakReferenceMessenger.Default.Send(new ObjectivesChangedMessage(nameof(FilteredObjectives), FilteredObjectives));
    }

    private async void HandleApiKeyStateChanged(object recipient, ApiKeyStateChangedMessage message)
    {
        await LoadObjectivesAsync();
        WeakReferenceMessenger.Default.Send(new ObjectivesChangedMessage(nameof(Objectives), Objectives));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<FilterChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ApiKeyStateChangedMessage>(this);
        GC.SuppressFinalize(this);
    }
}