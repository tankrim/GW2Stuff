using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class FilterViewModel : ViewModelBase, IFilter, IFilterViewModel, IDisposable
{
    private readonly ILogger<FilterViewModel> _logger;
    private readonly IMessagingService _messagingService;

    [ObservableProperty] private bool _filterDaily;
    [ObservableProperty] private bool _filterWeekly;
    [ObservableProperty] private bool _filterSpecial;
    [ObservableProperty] private bool _filterNotCompleted;
    [ObservableProperty] private bool _filterCompleted;
    [ObservableProperty] private bool _filterPvE;
    [ObservableProperty] private bool _filterPvP;
    [ObservableProperty] private bool _filterWvW;

    [ObservableProperty]
    private ObservableCollection<ApiKeyFilter> _apiKeyFilters = [];

    [ObservableProperty]
    private bool _isLoading;

    public IRelayCommand SendFilterChangedCommand { get; }

    public FilterViewModel(ILogger<FilterViewModel> logger, IMessagingService messagingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));

        SendFilterChangedCommand = new RelayCommand(SendFilterChangedMessage);

        PropertyChanged += OnPropertyChanged;
        ApiKeyFilters.CollectionChanged += OnApiKeyFiltersChanged;

        _messagingService.Register<ApiKeyMessages.ApiKeyAddedMessage>(this, HandleApiKeyAdded);
        _messagingService.Register<ApiKeyMessages.ApiKeyDeletedMessage>(this, HandleApiKeyDeleted);
        _messagingService.Register<ApiKeyMessages.ApiKeysLoadedMessage>(this, HandleApiKeysLoaded);
        _messagingService.Register<IsLoadingMessage>(this, HandleIsLoading);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("FilterViewModel property changed: {PropertyName}", e.PropertyName);
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
        _logger.LogDebug("FilterViewModel will send FilterChangedMessage. State: daily={fd}, weekly={fw}, special={fs}, notComplete={fnc}, completed={fc}, pve={fpve}, pvp={fpvp}, wvw={fwvw},  apiKeys={ak}",
            FilterDaily, FilterWeekly, FilterSpecial, FilterNotCompleted, FilterCompleted, FilterPvE, FilterPvP, FilterWvW,
            string.Join(", ", ApiKeyFilters.Where(f => f.IsSelected).Select(f => f.ApiKeyName)));
        _messagingService.Send(new FilterChangedMessage(this));
    }

    public void HandleApiKeyAdded(object recipient, ApiKeyMessages.ApiKeyAddedMessage message)
    {
        ApiKeyFilters.Add(new ApiKeyFilter(message.Value.Name));
    }

    public void HandleApiKeyDeleted(object recipient, ApiKeyMessages.ApiKeyDeletedMessage message)
    {
        var filterToRemove = ApiKeyFilters.FirstOrDefault(f => f.ApiKeyName == message.Value);
        if (filterToRemove != null)
        {
            ApiKeyFilters.Remove(filterToRemove);
        }
    }

    public void HandleApiKeysLoaded(object recipient, ApiKeyMessages.ApiKeysLoadedMessage message)
    {
        ApiKeyFilters.Clear();
        foreach (var dto in message.Value)
        {
            ApiKeyFilters.Add(new ApiKeyFilter(dto.Name));
        }
        _logger.LogInformation("Added {Count} API key filters", ApiKeyFilters.Count);
        SendFilterChangedCommand.Execute(null);
    }

    public void HandleIsLoading(object recipient, IsLoadingMessage message)
    {
        IsLoading = message.IsLoading;
    }

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
        ApiKeyFilters.CollectionChanged -= OnApiKeyFiltersChanged;

        foreach (var filter in ApiKeyFilters)
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

public partial class ApiKeyFilter : ObservableObject
{
    public string ApiKeyName { get; }

    [ObservableProperty]
    private bool _isSelected;

    public ApiKeyFilter(string apiKeyName)
    {
        ApiKeyName = apiKeyName;
        IsSelected = true;
    }
}