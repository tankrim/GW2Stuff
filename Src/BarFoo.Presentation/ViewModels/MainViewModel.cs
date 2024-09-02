﻿using BarFoo.Core.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly IStore _store;

    public ApiKeyViewModel ApiKeyVM { get; }
    public ObjectivesViewModel ObjectivesVM { get; }
    public FilterViewModel FilterVM { get; }

    public PactSupplyNetworkAgentViewModel PactSupplyNetworkAgentVM { get; }
    public StatusBarViewModel StatusBarVM { get; }

    [ObservableProperty]
    private bool _isPaneOpen;

    public MainViewModel(
        ApiKeyViewModel apiKeyVM,
        ObjectivesViewModel objectivesVM,
        FilterViewModel filterVM,
        PactSupplyNetworkAgentViewModel pactSupplyNetworkAgentVM,
        StatusBarViewModel statusBarVM,
        IStore store,
        ILogger<MainViewModel> logger)
    {
        ApiKeyVM = apiKeyVM ?? throw new ArgumentNullException(nameof(apiKeyVM));
        ObjectivesVM = objectivesVM ?? throw new ArgumentNullException(nameof(objectivesVM));
        FilterVM = filterVM ?? throw new ArgumentNullException(nameof(filterVM));
        PactSupplyNetworkAgentVM = pactSupplyNetworkAgentVM ?? throw new ArgumentNullException(nameof(pactSupplyNetworkAgentVM));
        StatusBarVM = statusBarVM ?? throw new ArgumentNullException(nameof(statusBarVM));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        WeakReferenceMessenger.Default.Register<ApiKeyStateChangedMessage>(this, HandleApiKeyStateChanged);
        WeakReferenceMessenger.Default.Register<IsUpdatingMessage>(this, HandleIsUpdating);
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing MainViewModel");
        await _store.InitializeAsync();
        await ApiKeyVM.LoadApiKeysAsync();
        await ObjectivesVM.LoadObjectivesAsync();
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<ApiKeyStateChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<IsUpdatingMessage>(this);
        ObjectivesVM.Dispose();
        GC.SuppressFinalize(this);
    }

    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
        ApiKeyVM.IsPaneOpen = IsPaneOpen;
    }

    [RelayCommand]
    private async Task DoManualSync()
    {
        await _store.SyncObjectivesForAllApiKeysAsync();
    }

    private async void HandleApiKeyStateChanged(object recipient, ApiKeyStateChangedMessage message)
    {
        await ObjectivesVM.LoadObjectivesAsync();
    }

    private void HandleIsUpdating(object recipient, IsUpdatingMessage message)
    {
        StatusBarVM.SetIsUpdatingTemporarily();
    }
}

public sealed class ApiKeyStateChangedMessage
{
}