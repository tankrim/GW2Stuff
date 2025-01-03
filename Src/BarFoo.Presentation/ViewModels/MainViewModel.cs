﻿using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;
using BarFoo.Presentation.Interfaces;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly IStore _store;
    private readonly IMessagingService _messagingService;
    private readonly IStatusUpdateService _statusUpdateService;

    public ApiKeyViewModel ApiKeyVM { get; }
    public ObjectivesViewModel ObjectivesVM { get; }
    public FilterViewModel FilterVM { get; }
    public PactSupplyNetworkAgentViewModel PactSupplyNetworkAgentVM { get; }
    public ArcDpsViewModel ArcDpsVM { get; }
    public InformationBarViewModel InformationBarVM { get; }
    public ProblemBarViewModel ProblemBarVM { get; }

    [ObservableProperty]
    private bool _isPaneOpen;

    public MainViewModel(
        ApiKeyViewModel apiKeyVM,
        ObjectivesViewModel objectivesVM,
        FilterViewModel filterVM,
        PactSupplyNetworkAgentViewModel pactSupplyNetworkAgentVM,
        ArcDpsViewModel arcDpsVM,
        InformationBarViewModel informationBarVM,
        ProblemBarViewModel problemBarVM,
        IStore store,
        ILogger<MainViewModel> logger,
        IMessagingService messagingService,
        IStatusUpdateService statusUpdateService)
    {
        ApiKeyVM = apiKeyVM ?? throw new ArgumentNullException(nameof(apiKeyVM));
        ObjectivesVM = objectivesVM ?? throw new ArgumentNullException(nameof(objectivesVM));
        FilterVM = filterVM ?? throw new ArgumentNullException(nameof(filterVM));
        PactSupplyNetworkAgentVM = pactSupplyNetworkAgentVM ?? throw new ArgumentNullException(nameof(pactSupplyNetworkAgentVM));
        ArcDpsVM = arcDpsVM ?? throw new ArgumentNullException($"{nameof(arcDpsVM)}");
        InformationBarVM = informationBarVM ?? throw new ArgumentNullException(nameof(informationBarVM));
        ProblemBarVM = problemBarVM ?? throw new ArgumentNullException(nameof(problemBarVM));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
        _statusUpdateService = statusUpdateService ?? throw new ArgumentNullException(nameof(statusUpdateService));

        _messagingService.Register<ApiKeyMessages.ApiKeyStateChangedMessage>(this, HandleApiKeyStateChanged);
        _messagingService.Register<IsUpdatingMessage>(this, HandleIsUpdating);
        _messagingService.Register<ApiKeyMessages.ApiKeysUpdatedMessage>(this, HandleApiKeysUpdated);
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
        _messagingService.Unregister<ApiKeyMessages.ApiKeyStateChangedMessage>(this);
        _messagingService.Unregister<IsUpdatingMessage>(this);
        _messagingService.Unregister<ApiKeyMessages.ApiKeysUpdatedMessage>(this);
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
        _messagingService.Send(new IsUpdatingMessage(true));
        _logger.LogInformation("Starting manual sync and reloading of objectives.");
        await _store.SyncObjectivesForAllApiKeysAsync();
        await ObjectivesVM.LoadObjectivesAsync();
        _messagingService.Send(new IsUpdatingMessage(false));
        _logger.LogInformation("Manual sync complete.");
    }

    private async void HandleApiKeyStateChanged(object recipient, ApiKeyMessages.ApiKeyStateChangedMessage message)
    {
        await ObjectivesVM.LoadObjectivesAsync();
    }

    private void HandleIsUpdating(object recipient, IsUpdatingMessage message)
    {
        _statusUpdateService.ShowUpdatingNotification();
    }

    private async void HandleApiKeysUpdated(object recipient, ApiKeyMessages.ApiKeysUpdatedMessage message)
    {
        _logger.LogInformation("Received ApiKeysUpdatedMessage. Reloading objectives.");
        await ObjectivesVM.LoadObjectivesAsync();
    }
}