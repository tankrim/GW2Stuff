using System.Collections.ObjectModel;

using BarFoo.Core.Services;
using BarFoo.Infrastructure.DTOs;

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
        ILogger<ApiKeyViewModel> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            _logger.LogInformation("Loaded {Count} apiKeys", ApiKeys.Count);
            var loadedApiKeyNames = ApiKeys.Select(x => x.Name);
            WeakReferenceMessenger.Default.Send(new LoadedApiKeysMessage(loadedApiKeyNames));
            _logger.LogInformation("ApiKeyViewModel sent LoadedApiKeysMessage with: {ApiKeyNames} ", loadedApiKeyNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading apiKeys");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddApiKey))]
    private async Task AddApiKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Token))
        {
            _logger.LogWarning("Tried to add an apiKey with invalid name or token");
            return;
        }

        var apiKey = await _store.CreateApiKeyAsync(Name, Token);

        try
        {
            ApiKeys.Add(apiKey);

            Name = string.Empty;
            Token = string.Empty;

            WeakReferenceMessenger.Default.Send<ApiKeyAddedMessage>(new ApiKeyAddedMessage(apiKey));
            _logger.LogInformation("ApiKeyViewModel sent ApiKeyAddedMessage for {ApiKeyName}", apiKey.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to add apiKey {ApiKey}", apiKey);

            await RemoveApiKeyAsync(apiKey);
        }
    }

    private bool CanAddApiKey() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Token);

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedApiKey))]
    private async Task RemoveSelectedApiKeyAsync()
    {
        if (SelectedApiKey is null)
        {
            throw new Exception("SelectedApiKey is null");
        }

        var apiKeyToRemove = SelectedApiKey;

        try
        {
            await _store.DeleteApiKeyAsync(apiKeyToRemove.Name);
            ApiKeys.Remove(apiKeyToRemove);
            _logger.LogInformation("ApiKey removed: {apiKey}", apiKeyToRemove);

            WeakReferenceMessenger.Default.Send<ApiKeyDeletedMessage>(new ApiKeyDeletedMessage(apiKeyToRemove));
            _logger.LogInformation("ApiKeyViewModel sent ApiKeyDeletedMessage for {ApiKeyName}", apiKeyToRemove.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to remove {apiKey}", apiKeyToRemove);
        }
        finally
        {
            SelectedApiKey = null;
        }

        _logger.LogWarning("Tried to remove an apiKey when no apiKey was selected.");
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

public sealed class ApiKeyAddedMessage : ValueChangedMessage<ApiKeyDto>
{
    public ApiKeyAddedMessage(ApiKeyDto apiKey) : base(apiKey)
    {
    }
}

public sealed class ApiKeyDeletedMessage : ValueChangedMessage<ApiKeyDto>
{
    public ApiKeyDeletedMessage(ApiKeyDto apiKey) : base(apiKey)
    {
    }
}
public sealed class LoadedApiKeysMessage : ValueChangedMessage<IEnumerable<string>>
{
    public LoadedApiKeysMessage(IEnumerable<string> apiKeyNames) : base(apiKeyNames)
    {
    }
}
