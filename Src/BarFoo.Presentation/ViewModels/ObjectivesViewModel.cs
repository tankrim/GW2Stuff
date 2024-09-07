using System.Collections.ObjectModel;

using BarFoo.Core.DTOs;
using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;
using BarFoo.Presentation.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ObjectivesViewModel : ViewModelBase, IDisposable
{
    private readonly IStore _store;
    private readonly IClipboardService _clipboardService;
    private readonly ILogger<ObjectivesViewModel> _logger;
    private readonly IMessagingService _messagingService;

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _objectives = new();

    [ObservableProperty]
    private ObservableCollection<ObjectiveWithOthersDto> _filteredObjectives = new();

    private readonly FilterViewModel _currentFilter;

    public ObjectivesViewModel(
        FilterViewModel filterViewModel,
        IStore store,
        IClipboardService clipboardService,
        ILogger<ObjectivesViewModel> logger,
        IMessagingService messagingService)
    {
        _currentFilter = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));

        _messagingService.Register<FilterChangedMessage>(this, HandleFilterChanged);
        _messagingService.Register<ApiKeyMessages.ApiKeyStateChangedMessage>(this, HandleApiKeyStateChanged);
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
            _messagingService.Send(new ObjectiveMessages.ObjectivesChangedMessage(nameof(Objectives), Objectives));
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
            var filterState = _currentFilter.GetFilterState();
            var filtered = Objectives.Where(o =>
                            (filterState.FilterDaily && o.ApiEndpoint == "daily" ||
                              filterState.FilterWeekly && o.ApiEndpoint == "weekly" ||
                              filterState.FilterSpecial && o.ApiEndpoint == "special" ||
                            (!filterState.FilterDaily && !filterState.FilterWeekly && !filterState.FilterSpecial)) &&
                            (filterState.FilterPvE && o.Track == "PvE" ||
                              filterState.FilterPvP && o.Track == "PvP" ||
                              filterState.FilterWvW && o.Track == "WvW" ||
                            (!filterState.FilterPvE && !filterState.FilterPvP && !filterState.FilterWvW)) &&
                            (!filterState.FilterCompleted || (o.Claimed || o.ProgressCurrent == o.ProgressComplete)) &&
                            (!filterState.FilterNotCompleted || (!o.Claimed && o.ProgressCurrent != o.ProgressComplete)) &&
                              filterState.SelectedApiKeys.Contains(o.ApiKeyName)
                           ).ToList();

            _logger.LogDebug("ApplyCurrentFilter before clearing. Current count: {Count}", FilteredObjectives.Count);
            FilteredObjectives.Clear();
            foreach (var objective in filtered)
            {
                FilteredObjectives.Add(objective);
            }

            _logger.LogInformation("Filter applied. Filtered objectives count: {Count}", FilteredObjectives.Count);
            _logger.LogDebug("Filter state: daily={fd}, weekly={fw}, special={fs}, pve={fpve}, pvp={fpvp}, wvw={fwvw}, completed={fc}",
                filterState.FilterDaily, filterState.FilterWeekly, filterState.FilterSpecial,
                filterState.FilterPvE, filterState.FilterPvP, filterState.FilterWvW, filterState.FilterCompleted);

            _messagingService.Send(new ObjectiveMessages.ObjectivesChangedMessage(nameof(FilteredObjectives), FilteredObjectives));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while applying filter");
        }
    }

    private void HandleFilterChanged(object recipient, FilterChangedMessage message)
    {
        _logger.LogInformation("Received FilterChangedMessage. Applying filter...");
        _logger.LogDebug("Received FilterChangedMessage state: daily={fd}, weekly={fw}, special={fs}, pve={fpve}, pvp={fpvp}, wvw={fwvw}, completed={fc}",
            message.FilterState.FilterDaily, message.FilterState.FilterWeekly, message.FilterState.FilterSpecial, message.FilterState.FilterPvE,
            message.FilterState.FilterPvP, message.FilterState.FilterWvW, message.FilterState.FilterCompleted);
        ApplyCurrentFilter();
        _messagingService.Send(new ObjectiveMessages.ObjectivesChangedMessage(nameof(FilteredObjectives), FilteredObjectives));
    }

    private async void HandleApiKeyStateChanged(object recipient, ApiKeyMessages.ApiKeyStateChangedMessage message)
    {
        await LoadObjectivesAsync();
        _messagingService.Send(new ObjectiveMessages.ObjectivesChangedMessage(nameof(Objectives), Objectives));
    }

    [RelayCommand]
    private async Task CopyFilteredObjectivesToClipboardClick()
    {

        if (FilteredObjectives != null && FilteredObjectives.Count != 0)
        {
            var groupedTitles = FilteredObjectives
                .GroupBy(o => o.ApiKeyName)
                .Select(g => string.Join(" ~~ ",
                    new[] { g.Key }.Concat(g.Select(o => o.Title))
                ));

            var groupedTitlesStrings = string.Join(" || ", groupedTitles);
            await _clipboardService.SetTextAsync(groupedTitlesStrings);
        }
    }

    public void Dispose()
    {
        _messagingService.Unregister<FilterChangedMessage>(this);
        _messagingService.Unregister<ApiKeyMessages.ApiKeyStateChangedMessage>(this);
        GC.SuppressFinalize(this);
    }
}