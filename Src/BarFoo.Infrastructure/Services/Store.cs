using System.Collections.Concurrent;

using BarFoo.Core.Interfaces;
using BarFoo.Core.DTOs;
using BarFoo.Infrastructure.Exceptions;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BarFoo.Infrastructure.Services;

public class Store : IStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFetcherService _fetcherService;
    private readonly ILogger<Store> _logger;
    private readonly IMessagingService _messagingService;
    private readonly ConcurrentDictionary<string, ApiKeyDto> _apiKeys;

    public bool IsInitialized { get; private set; } = false;

    public Store(
        IServiceProvider serviceProvider,
        IFetcherService fetcherService,
        ILogger<Store> logger,
        IMessagingService messagingService)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _fetcherService = fetcherService ?? throw new ArgumentNullException(nameof(fetcherService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
        _apiKeys = new ConcurrentDictionary<string, ApiKeyDto>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Store with existing apikeys");

            var apikeyService = _serviceProvider.GetRequiredService<IApiKeyService>();
            var existingApiKeys = await apikeyService.GetAllApiKeysAsync();

            foreach (var apikey in existingApiKeys)
            {
                _apiKeys[apikey.Name] = apikey;
            }

            IsInitialized = true;
            _logger.LogInformation("Store initialized with {Count} existing apikeys", _apiKeys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Store with existing apikeys");
            throw new StoreException("Failed to initialize Store", ex);
        }
    }
    public async Task<ApiKeyDto> CreateApiKeyAsync(string apikeyName, string apiKey)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var apikeyService = scope.ServiceProvider.GetRequiredService<IApiKeyService>();

            var apikeyDto = await apikeyService.CreateApiKeyAsync(apikeyName, apiKey);
            _apiKeys[apikeyDto.Name] = apikeyDto;

            await SyncObjectivesForApiKeyAsync(apikeyDto.Name);

            return apikeyDto;
        }
        catch (DuplicateApiKeyException ex)
        {
            _logger.LogError(ex, "Attempted to create an apikey which already exists: {ApiKeyName}", apikeyName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create apikey: {ApiKeyName}", apikeyName);
            throw new StoreException("Failed to create apikey", ex);
        }
    }

    public async Task DeleteApiKeyAsync(string apikeyName)
    {
        if (!_apiKeys.ContainsKey(apikeyName))
        {
            _logger.LogError("Attempted to delete an apikey which does not exist: {ApiKeyName}", apikeyName);
            throw new ApiKeyNotFoundException($"No such apikey: '{apikeyName}'.");
        }

        try
        {
            var apikeyService = _serviceProvider.GetRequiredService<IApiKeyService>();
            await apikeyService.DeleteApiKeyAsync(apikeyName);
            _apiKeys.TryRemove(apikeyName, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete apikey: {ApiKeyName}", apikeyName);
            throw new StoreException("Failed to delete apikey", ex);
        }
    }

    public Task<ApiKeyDto?> GetApiKeyAsync(string apikeyName)
    {
        return Task.FromResult(_apiKeys.TryGetValue(apikeyName, out var apikey) ? apikey : null);
    }

    public Task<IEnumerable<ApiKeyDto>> GetAllApiKeysAsync()
    {
        return Task.FromResult(_apiKeys.Values.AsEnumerable());
    }

    public async Task<IEnumerable<ObjectiveDto>> GetAllObjectivesAsync()
    {
        var allObjectives = new List<ObjectiveDto>();
        foreach (var apikey in _apiKeys.Values)
        {
            var apikeyObjectives = apikey.Objectives.Select(o => new ObjectiveDto
            {
                Id = o.Id,
                Title = o.Title,
                Track = o.Track,
                Acclaim = o.Acclaim,
                ProgressCurrent = o.ProgressCurrent,
                ProgressComplete = o.ProgressComplete,
                Claimed = o.Claimed,
                ApiEndpoint = o.ApiEndpoint,
                ApiKeyName = apikey.Name
            });
            allObjectives.AddRange(apikeyObjectives);
        }
        return allObjectives;
    }

    public async Task<IEnumerable<ObjectiveWithOthersDto>> GetAllObjectivesWithOthersAsync()
    {
        var allObjectives = new List<ObjectiveWithOthersDto>();
        var objectiveMap = new Dictionary<int, List<string>>();

        // First pass: collect all objectives and associated apikeys
        foreach (var apikey in _apiKeys.Values)
        {
            foreach (var objective in apikey.Objectives)
            {
                if (!objectiveMap.TryGetValue(objective.Id, out List<string>? value))
                {
                    value = [];
                    objectiveMap[objective.Id] = value;
                }

                value.Add(apikey.Name);
            }
        }

        // Second pass: create ObjectiveWithOthersDto instances
        foreach (var apikey in _apiKeys.Values)
        {
            foreach (var objective in apikey.Objectives)
            {
                var others = string.Join(",", objectiveMap[objective.Id].Where(a => a != apikey.Name));
                var objectiveWithOthers = new ObjectiveWithOthersDto
                {
                    Id = objective.Id,
                    Title = objective.Title,
                    Track = objective.Track,
                    Acclaim = objective.Acclaim,
                    ProgressCurrent = objective.ProgressCurrent,
                    ProgressComplete = objective.ProgressComplete,
                    Claimed = objective.Claimed,
                    ApiEndpoint = objective.ApiEndpoint,
                    ApiKeyName = apikey.Name,
                    Others = string.IsNullOrEmpty(others) ? "" : others
                };
                allObjectives.Add(objectiveWithOthers);
            }
        }

        return allObjectives;
    }

    public Task<IEnumerable<ObjectiveDto>> GetFilteredObjectivesAsync(Func<ObjectiveDto, bool> predicate)
    {
        var filteredObjectives = _apiKeys.Values
            .SelectMany(a => a.Objectives ?? Enumerable.Empty<ObjectiveDto>())
            .Where(predicate);
        return Task.FromResult(filteredObjectives);
    }

    public Task<IEnumerable<ObjectiveDto>> GetObjectivesForApiKeyAsync(string apikeyName)
    {
        if (!_apiKeys.TryGetValue(apikeyName, out var storedApiKeyDto))
        {
            _logger.LogError("Attempted to get objectives for an apikey which does not exist: {ApiKeyName}", apikeyName);
            throw new ApiKeyNotFoundException($"No such apikey: '{apikeyName}'.");
        }

        return Task.FromResult(storedApiKeyDto.Objectives ?? Enumerable.Empty<ObjectiveDto>());
    }

    public async Task<ApiKeyDto> SyncObjectivesForApiKeyAsync(string apikeyName)
    {
        if (!_apiKeys.TryGetValue(apikeyName, out var storedApiKeyDto))
        {
            _logger.LogError("Attempted to sync objectives for an apikey which does not exist: {ApiKeyName}", apikeyName);
            throw new ApiKeyNotFoundException($"No such apikey: '{apikeyName}'.");
        }

        try
        {
            var newObjectives = await _fetcherService.FetchObjectivesForApiKeyAsync(apikeyName, new CancellationToken());

            // Create a new ApiKeyDto with updated objectives
            var updatedApiKeyDto = new ApiKeyDto
            {
                Name = storedApiKeyDto.Name,
                Key = storedApiKeyDto.Key,
                HasToken = storedApiKeyDto.HasToken,
                HasBeenSyncedOnce = true,
                LastSyncTime = DateTime.UtcNow,
                Objectives = newObjectives.ToList()
            };

            var apikeyService = _serviceProvider.GetRequiredService<IApiKeyService>();
            await apikeyService.UpdateApiKeyAsync(updatedApiKeyDto);

            // Fetch the most up-to-date data from the database
            var latestApiKeyDto = await apikeyService.GetApiKeyAsync(apikeyName);

            if (latestApiKeyDto != null)
            {
                // Update the in-memory cache with the latest data from the database
                _apiKeys[apikeyName] = latestApiKeyDto;

                _messagingService.Send(new ApiKeyUpdatedMessage(apikeyName));
                return latestApiKeyDto;
            }
            else
            {
                throw new StoreException($"Failed to retrieve updated ApiKey data for {apikeyName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync objectives for apikey: {ApiKeyName}", apikeyName);
            throw new StoreException("Failed to sync objectives", ex);
        }
    }

    public async Task SyncObjectivesForAllApiKeysAsync()
    {
        var apikeyNames = _apiKeys.Keys.ToList();
        var fetcherService = _serviceProvider.GetRequiredService<IFetcherService>();

        try
        {
            var allObjectives = await fetcherService.FetchObjectivesForAllApiKeysAsync(apikeyNames, CancellationToken.None);

            foreach (var (apikeyName, objectives) in allObjectives)
            {
                if (_apiKeys.TryGetValue(apikeyName, out var apikeyDto))
                {
                    apikeyDto.UpdateObjectives(objectives);

                    var apiKeyService = _serviceProvider.GetRequiredService<IApiKeyService>();
                    await apiKeyService.UpdateApiKeyAsync(apikeyDto);

                    _apiKeys[apikeyName] = apikeyDto;

                    _messagingService.Send(new ApiKeyUpdatedMessage(apikeyName));
                }
            }
        }
        catch (AggregateException ae)
        {
            _logger.LogError(ae, "Errors occurred while fetching objectives for apikeys");
            // Here you might want to implement a notification system to inform the user about the sync issues
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while syncing objectives for all apikeys");
            // Here you might want to implement a notification system to inform the user about the sync issues
        }
    }
}

public sealed class ApiKeyUpdatedMessage : ValueChangedMessage<string>
{
    public ApiKeyUpdatedMessage(string apiKeyName) : base(apiKeyName) { }
}