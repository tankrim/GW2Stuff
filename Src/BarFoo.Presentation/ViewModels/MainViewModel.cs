using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using BarFoo.Core.Services;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly IStore _store;

    public ApiKeyViewModel ApiKeyVM { get; }
    public ObjectivesViewModel ObjectivesVM { get; }
    public FilterViewModel FilterVM { get; }
    public StatusBarViewModel StatusBarVM { get; }

    [ObservableProperty]
    private bool _isPaneOpen;

    public MainViewModel(
        ApiKeyViewModel apiKeyVM,
        ObjectivesViewModel objectivesVM,
        FilterViewModel filterVM,
        StatusBarViewModel statusBarVM,
        IStore store,
        ILogger<MainViewModel> logger)
    {
        ApiKeyVM = apiKeyVM;
        ObjectivesVM = objectivesVM;
        FilterVM = filterVM;
        StatusBarVM = statusBarVM;
        _store = store;
        _logger = logger;

        RegisterMessengers();
    }

    private void RegisterMessengers()
    {
        WeakReferenceMessenger.Default.Register<LoadedApiKeysMessage>(this, async (r, m) =>
        {
            _logger.LogInformation("MainViewModel received {Message} with: {Value}", m, m.Value);
            await ObjectivesVM.LoadObjectivesAsync();
        });

        WeakReferenceMessenger.Default.Register<ApiKeyAddedMessage>(this, async (r, m) =>
        {
            _logger.LogInformation("MainViewModel received {Message} with: {Value}", m, m.Value);
            await ObjectivesVM.LoadObjectivesAsync();
        });

        WeakReferenceMessenger.Default.Register<ApiKeyDeletedMessage>(this, async (r, m) =>
        {
            _logger.LogInformation("MainViewModel received {Message} with: {Value}", m, m.Value);
            await ObjectivesVM.LoadObjectivesAsync();
        });

        WeakReferenceMessenger.Default.Register<ApiKeyUpdatedMessage>(this, async (r, m) =>
        {
            _logger.LogInformation("MainViewModel received {Message} with: {Value}", m, m.Value);
            await ObjectivesVM.LoadObjectivesAsync();
        });

        WeakReferenceMessenger.Default.Register<IsUpdatingMessage>(this, (r, m) =>
        {
            _logger.LogInformation("MainViewModel received {Message} with: {Value}", m, m.Value);
            StatusBarVM.SetIsUpdatingTemporarily();
        });
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing MainViewModel");
        await ApiKeyVM.LoadApiKeysAsync();
        var apiKeys = await _store.GetAllApiKeysAsync();
        await ObjectivesVM.LoadObjectivesAsync();
        _logger.LogInformation("MainViewModel initialization completed");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ObjectivesVM.Dispose();
            // Dispose other disposable resources...
        }
    }

    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
        ApiKeyVM.IsPaneOpen = IsPaneOpen;
    }
}