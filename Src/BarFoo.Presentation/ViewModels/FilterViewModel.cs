using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class FilterViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<FilterViewModel> _logger;
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

    public FilterViewModel(ILogger<FilterViewModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        PropertyChanged += OnPropertyChanged;
        ApiKeyFilters.CollectionChanged += OnApiKeyFiltersChanged;

        WeakReferenceMessenger.Default.Register<ApiKeyAddedMessage>(this, HandleApiKeyAdded);
        WeakReferenceMessenger.Default.Register<ApiKeyDeletedMessage>(this, HandleApiKeyDeleted);
        WeakReferenceMessenger.Default.Register<LoadedApiKeysMessage>(this, HandleLoadedApiKeys);
        WeakReferenceMessenger.Default.Register<IsLoadingMessage>(this, HandleIsLoading);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("FilterViewModel property changed: {PropertyName}", e.PropertyName);
        SendFilterChangedMessage();
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

        SendFilterChangedMessage();
    }

    private void OnApiKeyFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApiKeyFilter.IsSelected))
        {
            _logger.LogDebug("ApiKeyFilter IsSelected changed");
            SendFilterChangedMessage();
        }
    }

    private void SendFilterChangedMessage()
    {
        _logger.LogDebug("FilterViewModel will send FilterChangedMessage. State: daily={fd}, weekly={fw}, special={fs}, pve={fpve}, pvp={fpvp}, wvw={fwvw}, completed={fc}, apiKeys={ak}",
            FilterDaily, FilterWeekly, FilterSpecial, FilterPvE, FilterPvP, FilterWvW, FilterCompleted,
            string.Join(", ", ApiKeyFilters.Where(f => f.IsSelected).Select(f => f.ApiKeyName)));
        WeakReferenceMessenger.Default.Send(new FilterChangedMessage(this));
    }

    private void HandleApiKeyAdded(object recipient, ApiKeyAddedMessage message)
    {
        ApiKeyFilters.Add(new ApiKeyFilter(message.Value.Name));
    }

    private void HandleApiKeyDeleted(object recipient, ApiKeyDeletedMessage message)
    {
        var filterToRemove = ApiKeyFilters.FirstOrDefault(f => f.ApiKeyName == message.Value);
        if (filterToRemove != null)
        {
            ApiKeyFilters.Remove(filterToRemove);
        }
    }

    private void HandleLoadedApiKeys(object recipient, LoadedApiKeysMessage message)
    {
        ApiKeyFilters.Clear();
        foreach (var dto in message.Value)
        {
            ApiKeyFilters.Add(new ApiKeyFilter(dto.Name));
        }
        _logger.LogInformation("Added {Count} API key filters", ApiKeyFilters.Count);
        SendFilterChangedMessage();
    }

    private void HandleIsLoading(object recipient, IsLoadingMessage message)
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

        WeakReferenceMessenger.Default.Unregister<ApiKeyAddedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ApiKeyDeletedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<LoadedApiKeysMessage>(this);
        WeakReferenceMessenger.Default.Unregister<IsLoadingMessage>(this);
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

public sealed class FilterChangedMessage : ValueChangedMessage<FilterViewModel>
{
    public FilterChangedMessage(FilterViewModel filter) : base(filter) { }
}

public sealed class IsLoadingMessage
{
    public bool IsLoading { get; }

    public IsLoadingMessage(bool isLoading)
    {
        IsLoading = isLoading;
    }
}