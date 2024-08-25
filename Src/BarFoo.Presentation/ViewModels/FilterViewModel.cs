using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using BarFoo.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BarFoo.Presentation.ViewModels;

public partial class FilterViewModel : ViewModelBase,
    IDisposable, IRecipient<ApiKeyDeletedMessage>, IRecipient<ApiKeyAddedMessage>, IRecipient<LoadedApiKeysMessage>
{
    [ObservableProperty] private bool _filterDaily;
    [ObservableProperty] private bool _filterWeekly;
    [ObservableProperty] private bool _filterSpecial;
    [ObservableProperty] private bool _filterCompleted;
    [ObservableProperty] private bool _filterPvE;
    [ObservableProperty] private bool _filterPvP;
    [ObservableProperty] private bool _filterWvW;

    private ObservableCollection<ApiKeyFilter> _apiKeyFilters = [];
    public ObservableCollection<ApiKeyFilter> ApiKeyFilters
    {
        get => _apiKeyFilters;
        set
        {
            if (_apiKeyFilters != value)
            {
                if (_apiKeyFilters != null)
                {
                    _apiKeyFilters.CollectionChanged -= OnApiKeyFiltersChanged;
                }
                _apiKeyFilters = value;
                if (_apiKeyFilters != null)
                {
                    _apiKeyFilters.CollectionChanged += OnApiKeyFiltersChanged;
                }
                OnPropertyChanged();
            }
        }
    }

    public FilterViewModel()
    {
        PropertyChanged += OnPropertyChanged;
        ApiKeyFilters.CollectionChanged += OnApiKeyFiltersChanged;

        WeakReferenceMessenger.Default.Register<ApiKeyAddedMessage>(this);
        WeakReferenceMessenger.Default.Register<ApiKeyDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<LoadedApiKeysMessage>(this);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
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
        SendFilterChangedMessage();
    }

    private void SendFilterChangedMessage()
    {
        WeakReferenceMessenger.Default.Send(new FilterChangedMessage(this));
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

        GC.SuppressFinalize(this);
    }

    public void Receive(ApiKeyDeletedMessage message)
    {
        var filterToRemove = ApiKeyFilters.FirstOrDefault(f => f.ApiKeyName == message.Value.Name);
        if (filterToRemove != null)
        {
            ApiKeyFilters.Remove(filterToRemove);
        }
    }

    public void Receive(ApiKeyAddedMessage message)
    {
        ApiKeyFilters.Add(new ApiKeyFilter(message.Value.Name));
    }

    public void Receive(LoadedApiKeysMessage message)
    {
        ApiKeyFilters.Clear();
        foreach (var name in message.Value)
        {
            ApiKeyFilters.Add(new ApiKeyFilter(name));
        }
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
    public FilterChangedMessage(FilterViewModel filter) : base(filter)
    {
    }
}