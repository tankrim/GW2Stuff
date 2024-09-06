using System.Collections.ObjectModel;

using BarFoo.Core.DTOs;
using BarFoo.Core.Interfaces;
using BarFoo.Core.Messages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ApiKeyViewModel : ViewModelBase
{
    private readonly ILogger<ApiKeyViewModel> _logger;
    private readonly IMessagingService _messagingService;
    private readonly IStore _store;

    [ObservableProperty]
    private ObservableCollection<ApiKeyDto> _apiKeys = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddApiKeyCommand))]
    private string? _name;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddApiKeyCommand))]
    private string? _token;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedApiKeyCommand))]
    private ApiKeyDto? _selectedApiKey;

    [ObservableProperty]
    private bool _isPaneOpen = false;

    public ApiKeyViewModel(
        ILogger<ApiKeyViewModel> logger,
        IMessagingService messagingService,
        IStore store)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public virtual async Task LoadApiKeysAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKeys = await _store.GetAllApiKeysAsync();
            ApiKeys.Clear();
            foreach (var apiKey in apiKeys)
            {
                ApiKeys.Add(apiKey);
            }
            _logger.LogInformation("Loaded {Count} API keys", ApiKeys.Count);
            _messagingService.Send(new ApiKeyMessages.ApiKeysLoadedMessage(ApiKeys));
            NotifyApiKeyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading API keys");
            throw;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddApiKey))]
    private async Task AddApiKeyAsync(CancellationToken cancellationToken)
    {
        if (!CanAddApiKey())
        {
            return;
        }

        try
        {
            var apiKey = await _store.CreateApiKeyAsync(Name!, Token!);
            ApiKeys.Add(apiKey);
            Name = string.Empty;
            Token = string.Empty;
            _messagingService.Send(new ApiKeyMessages.ApiKeyAddedMessage(apiKey));
            NotifyApiKeyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add API key");
            throw;
        }
    }

    private bool CanAddApiKey() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Token);

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedApiKey))]
    private async Task RemoveSelectedApiKeyAsync(CancellationToken cancellationToken)
    {
        if (!CanRemoveSelectedApiKey())
        {
            return;
        }

        try
        {
            var keyName = SelectedApiKey!.Name;
            await _store.DeleteApiKeyAsync(keyName);
            ApiKeys.Remove(SelectedApiKey);
            _messagingService.Send(new ApiKeyMessages.ApiKeyDeletedMessage(keyName));
            NotifyApiKeyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove API key: {ApiKey}", SelectedApiKey!.Name);
            throw;
        }
    }

    private bool CanRemoveSelectedApiKey() => SelectedApiKey is not null;

    protected virtual void NotifyApiKeyStateChanged()
    {
        _messagingService.Send(new ApiKeyMessages.ApiKeyStateChangedMessage());
    }
}