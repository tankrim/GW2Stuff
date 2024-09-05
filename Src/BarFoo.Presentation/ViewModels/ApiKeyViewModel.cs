using System.Collections.ObjectModel;

using BarFoo.Core.Interfaces;
using BarFoo.Core.DTOs;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.Logging;

namespace BarFoo.Presentation.ViewModels;

public partial class ApiKeyViewModel : ViewModelBase
{
    private readonly IStore _store;
    private readonly ILogger<ApiKeyViewModel> _logger;
    private readonly IMessagingService _messagingService;

    [ObservableProperty]
    private ObservableCollection<ApiKeyDto> _apiKeys = new ObservableCollection<ApiKeyDto>();

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
        IStore store,
        ILogger<ApiKeyViewModel> logger,
        IMessagingService messagingService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
    }

    public async Task LoadApiKeysAsync()
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
            _messagingService.Send(new LoadedApiKeysMessage(ApiKeys));
            NotifyApiKeyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading API keys");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddApiKey))]
    private async Task AddApiKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Token))
        {
            _logger.LogWarning("Attempted to add an API key with invalid name or token");
            return;
        }

        try
        {
            var apiKey = await _store.CreateApiKeyAsync(Name, Token);
            ApiKeys.Add(apiKey);
            Name = string.Empty;
            Token = string.Empty;
            _messagingService.Send(new ApiKeyAddedMessage(apiKey));
            NotifyApiKeyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add API key");
        }
    }

    private bool CanAddApiKey() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Token);

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedApiKey))]
    private async Task RemoveSelectedApiKeyAsync()
    {
        if (SelectedApiKey == null)
        {
            _logger.LogWarning("Attempted to remove an API key when no API key was selected");
            return;
        }

        try
        {
            var keyName = SelectedApiKey.Name;
            await _store.DeleteApiKeyAsync(SelectedApiKey.Name);
            ApiKeys.Remove(SelectedApiKey);
            _messagingService.Send(new ApiKeyDeletedMessage(keyName));
            NotifyApiKeyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove API key: {ApiKey}", SelectedApiKey.Name);
        }
    }

    private void NotifyApiKeyStateChanged()
    {
        _messagingService.Send(new ApiKeyStateChangedMessage());
    }

    private bool CanRemoveSelectedApiKey() => SelectedApiKey is not null;

    private async Task RemoveApiKeyAsync(ApiKeyDto apiKeyDto)
    {
        try
        {
            ApiKeys.Remove(apiKeyDto);
            await _store.DeleteApiKeyAsync(apiKeyDto.Name);
            _logger.LogInformation("Removed apiKey {ApiKey}", apiKeyDto);
            // We don't send ApiKeyDeletedMessage here since this removal doesn't affect other views.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing apiKey {ApiKey}", apiKeyDto);
        }
    }
}

public sealed class ApiKeyDeletedMessage : ValueChangedMessage<string>
{
    public ApiKeyDeletedMessage(string keyName)
        : base(keyName) { }
}

public sealed class LoadedApiKeysMessage : ValueChangedMessage<IEnumerable<ApiKeyDto>>
{
    public LoadedApiKeysMessage(IEnumerable<ApiKeyDto> dtos)
        : base(dtos) { }
}

public sealed class ApiKeyAddedMessage : ValueChangedMessage<ApiKeyDto>
{
    public ApiKeyAddedMessage(ApiKeyDto dto)
        : base(dto) { }
}