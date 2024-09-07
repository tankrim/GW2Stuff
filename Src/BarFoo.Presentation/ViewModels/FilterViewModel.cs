using System.Collections.Specialized;
using System.ComponentModel;

using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;
using BarFoo.Presentation.Filters;
using BarFoo.Presentation.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class FilterViewModel : ViewModelBase, IFilter, IFilterViewModel, IDisposable
{
    private readonly ILogger<FilterViewModel> _logger;
    private readonly IMessagingService _messagingService;

    [ObservableProperty]
    private FilterState _filterState;

    [ObservableProperty]
    private bool _isLoading;

    public IRelayCommand SendFilterChangedCommand { get; }

    public FilterViewModel(ILogger<FilterViewModel> logger, IMessagingService messagingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));

        FilterState = new FilterState
        {
            FilterDaily = false,
            FilterWeekly = false,
            FilterSpecial = false,
            FilterPvE = false,
            FilterPvP = false,
            FilterWvW = false,
            FilterNotCompleted = false,
            FilterCompleted = false
        };
        SendFilterChangedCommand = new RelayCommand(SendFilterChangedMessage);

        FilterState.PropertyChanged += OnFilterStatePropertyChanged;
        FilterState.ApiKeyFilters.CollectionChanged += OnApiKeyFiltersChanged;

        _messagingService.Register<ApiKeyMessages.ApiKeyAddedMessage>(this, HandleApiKeyAdded);
        _messagingService.Register<ApiKeyMessages.ApiKeyDeletedMessage>(this, HandleApiKeyDeleted);
        _messagingService.Register<ApiKeyMessages.ApiKeysLoadedMessage>(this, HandleApiKeysLoaded);
        _messagingService.Register<IsLoadingMessage>(this, HandleIsLoading);
    }

    private void OnFilterStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("FilterState property changed: {PropertyName}", e.PropertyName);
        SendFilterChangedCommand.Execute(null);
    }

    private void OnApiKeyFiltersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ApiKeyFilter newFilter in e.NewItems)
            {
                newFilter.PropertyChanged += OnApiKeyFilterPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ApiKeyFilter oldFilter in e.OldItems)
            {
                oldFilter.PropertyChanged -= OnApiKeyFilterPropertyChanged;
            }
        }

        SendFilterChangedCommand.Execute(null);
    }

    private void OnApiKeyFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApiKeyFilter.IsSelected))
        {
            _logger.LogDebug("ApiKeyFilter IsSelected changed");
            SendFilterChangedCommand.Execute(null);
        }
    }

    private void SendFilterChangedMessage()
    {
        _logger.LogDebug("FilterViewModel will send FilterChangedMessage. "
            + "State: daily={fd}, weekly={fw}, special={fs}, notComplete={fnc}, completed={fc}, pve={fpve}, pvp={fpvp}, wvw={fwvw}, apiKeys={ak}",
            FilterState.FilterDaily, FilterState.FilterWeekly, FilterState.FilterSpecial, FilterState.FilterNotCompleted, FilterState.FilterCompleted,
            FilterState.FilterPvE, FilterState.FilterPvP, FilterState.FilterWvW,
            string.Join(", ", FilterState.ApiKeyFilters.Where(f => f.IsSelected).Select(f => f.ApiKeyName)));
        _messagingService.Send(new FilterChangedMessage(FilterState));
    }

    public IFilterState GetFilterState() => FilterState;

    public void HandleApiKeyAdded(object recipient, ApiKeyMessages.ApiKeyAddedMessage message)
    {
        FilterState.ApiKeyFilters.Add(new ApiKeyFilter(message.Value.Name));
    }

    public void HandleApiKeyDeleted(object recipient, ApiKeyMessages.ApiKeyDeletedMessage message)
    {
        var filterToRemove = FilterState.ApiKeyFilters.FirstOrDefault(f => f.ApiKeyName == message.Value);
        if (filterToRemove != null)
        {
            FilterState.ApiKeyFilters.Remove(filterToRemove);
        }
    }

    public void HandleApiKeysLoaded(object recipient, ApiKeyMessages.ApiKeysLoadedMessage message)
    {
        FilterState.ApiKeyFilters.Clear();
        foreach (var dto in message.Value)
        {
            FilterState.ApiKeyFilters.Add(new ApiKeyFilter(dto.Name));
        }
        _logger.LogInformation("Added {Count} API key filters", FilterState.ApiKeyFilters.Count);
        SendFilterChangedCommand.Execute(null);
    }

    public void HandleIsLoading(object recipient, IsLoadingMessage message)
    {
        IsLoading = message.IsLoading;
    }

    public void Dispose()
    {
        FilterState.PropertyChanged -= OnFilterStatePropertyChanged;
        FilterState.ApiKeyFilters.CollectionChanged -= OnApiKeyFiltersChanged;

        foreach (var filter in FilterState.ApiKeyFilters)
        {
            filter.PropertyChanged -= OnApiKeyFilterPropertyChanged;
        }

        _messagingService.Unregister<ApiKeyMessages.ApiKeyAddedMessage>(this);
        _messagingService.Unregister<ApiKeyMessages.ApiKeyDeletedMessage>(this);
        _messagingService.Unregister<ApiKeyMessages.ApiKeysLoadedMessage>(this);
        _messagingService.Unregister<IsLoadingMessage>(this);
        GC.SuppressFinalize(this);
    }
}