using AutoMapper;

using BarFoo.Core.DTOs;
using BarFoo.Core.Interfaces;
using BarFoo.Data.Contexts;
using BarFoo.Data.Exceptions;
using BarFoo.Domain.Entities;
using BarFoo.Infrastructure.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BarFoo.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IDbContextFactory<BarFooDbContext> _contextFactory;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(
        IMapper mapper,
        IServiceProvider serviceProvider,
        IApiKeyRepository apiKeyRepository,
        IDbContextFactory<BarFooDbContext> contextFactory,
        ILogger<ApiKeyService> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiKeyDto> CreateApiKeyAsync(string name, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (await _apiKeyRepository.ExistsByNameAsync(name))
        {
            throw new DuplicateApiKeyException($"An api key with the name '{name}' already exists.");
        }

        var apiKey = ApiKey.CreateApiKey(name, token);

        try
        {
            await _apiKeyRepository.AddAsync(apiKey);

            return _mapper.Map<ApiKeyDto>(apiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create api key: {Name}", name);
            throw new ServiceException("Failed to create api key", ex);
        }
    }

    public async Task DeleteApiKeyAsync(string apiKeyName)
    {
        try
        {
            await _apiKeyRepository.DeleteAsync(apiKeyName);
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(ex, "Failed to delete api key: {Name}", apiKeyName);
            throw new ServiceException("Failed to delete api key", ex);
        }
    }

    public async Task<ApiKeyDto?> GetApiKeyAsync(string apiKeyName)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var apikey = await context.ApiKeys
                .Include(a => a.ApiKeyObjectives)
                .ThenInclude(ao => ao.Objective)
                .FirstOrDefaultAsync(a => a.Name == apiKeyName);

            return apikey != null ? _mapper.Map<ApiKeyDto>(apikey) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get api key: {Name}", apiKeyName);
            throw new ServiceException("Failed to get api key", ex);
        }
    }

    public async Task<IEnumerable<ApiKeyDto>> GetAllApiKeysAsync()
    {
        try
        {
            var apikeys = await _apiKeyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ApiKeyDto>>(apikeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all api keys");
            throw new ServiceException("Failed to get all api keys", ex);
        }
    }

    public async Task UpdateApiKeyAsync(ApiKeyDto apiKeyDto)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var apiKey = await GetApiKeyWithObjectives(context, apiKeyDto.Name);

            UpdateApiKeyProperties(apiKey, apiKeyDto);
            await UpdateObjectives(context, apiKey, apiKeyDto.Objectives);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "ApiKey not found: {Name}", apiKeyDto.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update api key: {Name}", apiKeyDto.Name);
            throw new ServiceException("Failed to update api key", ex);
        }
    }

    private async Task<ApiKey> GetApiKeyWithObjectives(BarFooDbContext context, string apikeyName)
    {
        var apikey = await context.ApiKeys
            .Include(a => a.ApiKeyObjectives)
            .ThenInclude(ao => ao.Objective)
            .FirstOrDefaultAsync(a => a.Name == apikeyName);

        if (apikey == null)
        {
            throw new NotFoundException($"ApiKey with name '{apikeyName}' not found.");
        }

        return apikey;
    }

    private void UpdateApiKeyProperties(ApiKey apiKey, ApiKeyDto apiKeyDto)
    {
        apiKey.HasBeenSyncedOnce = apiKeyDto.HasBeenSyncedOnce;
        apiKey.LastSyncTime = apiKeyDto.LastSyncTime;
    }

    private async Task UpdateObjectives(BarFooDbContext context, ApiKey apiKey, IEnumerable<ObjectiveDto> newObjectives)
    {
        var existingObjectives = apiKey.ApiKeyObjectives.ToDictionary(ao => ao.ObjectiveId, ao => ao);
        var newObjectivesDict = newObjectives.ToDictionary(o => o.Id, o => o);

        // Remove objectives no longer associated with the apiKey
        foreach (var objectiveToRemove in existingObjectives.Values.Where(ao => !newObjectivesDict.ContainsKey(ao.ObjectiveId)))
        {
            apiKey.ApiKeyObjectives.Remove(objectiveToRemove);
        }

        // Update or add objectives
        foreach (var newObjective in newObjectivesDict.Values)
        {
            if (existingObjectives.TryGetValue(newObjective.Id, out var existingApiKeyObjective))
            {
                // Update existing objective
                UpdateObjectiveProperties(existingApiKeyObjective.Objective, newObjective);
            }
            else
            {
                // Add new objective
                var objective = await GetOrCreateObjective(context, newObjective);
                apiKey.ApiKeyObjectives.Add(new ApiKeyObjective
                {
                    ApiKey = apiKey,
                    Objective = objective
                });
            }
        }
    }

    private void UpdateObjectiveProperties(Objective objective, ObjectiveDto objectiveDto)
    {
        objective.Title = objectiveDto.Title;
        objective.Track = objectiveDto.Track;
        objective.Acclaim = objectiveDto.Acclaim;
        objective.ProgressCurrent = objectiveDto.ProgressCurrent;
        objective.ProgressComplete = objectiveDto.ProgressComplete;
        objective.Claimed = objectiveDto.Claimed;
        objective.ApiEndpoint = objectiveDto.ApiEndpoint;
    }

    private async Task<Objective> GetOrCreateObjective(BarFooDbContext context, ObjectiveDto objectiveDto)
    {
        var objective = await context.Objectives.FindAsync(objectiveDto.Id);
        if (objective == null)
        {
            objective = new Objective
            {
                Id = objectiveDto.Id
            };
            context.Objectives.Add(objective);
        }
        UpdateObjectiveProperties(objective, objectiveDto);
        return objective;
    }

    private async Task UpdateAllApiKeysAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting scheduled update of all apikeys");

        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IStore>();

        try
        {
            await store.SyncObjectivesForAllApiKeysAsync();
            _logger.LogInformation("Completed scheduled update of all apikeys");
        }
        catch (AggregateException ae)
        {
            _logger.LogError(ae, "Multiple errors occurred during scheduled update of apikeys");
            foreach (var innerException in ae.InnerExceptions)
            {
                _logger.LogError(innerException, "Error details");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during scheduled update of apikeys");
        }
    }
}