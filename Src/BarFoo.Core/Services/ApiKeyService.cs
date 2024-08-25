using AutoMapper;

using BarFoo.Core.Exceptions;
using BarFoo.Data.Contexts;
using BarFoo.Data.Exceptions;
using BarFoo.Data.Repositories;
using BarFoo.Domain.Entities;
using BarFoo.Infrastructure.DTOs;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BarFoo.Core.Services;

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
        IApiKeyRepository apikeyRepository,
        IDbContextFactory<BarFooDbContext> contextFactory,
        ILogger<ApiKeyService> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _apiKeyRepository = apikeyRepository ?? throw new ArgumentNullException(nameof(apikeyRepository));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger;
    }

    public async Task<ApiKeyDto> CreateApiKeyAsync(string name, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        if (await _apiKeyRepository.ExistsByNameAsync(name))
        {
            throw new DuplicateApiKeyException($"An apikey with the name '{name}' already exists.");
        }

        var apikey = ApiKey.CreateApiKey(name, token);

        try
        {
            await _apiKeyRepository.AddAsync(apikey);

            return _mapper.Map<ApiKeyDto>(apikey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create apikey: {Name}", name);
            throw new ServiceException("Failed to create apikey", ex);
        }
    }

    public async Task DeleteApiKeyAsync(string apikeyName)
    {
        try
        {
            await _apiKeyRepository.DeleteAsync(apikeyName);
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(ex, "Failed to delete apikey: {Name}", apikeyName);
            throw new ServiceException("Failed to delete apikey", ex);
        }
    }

    public async Task<ApiKeyDto?> GetApiKeyAsync(string apikeyName)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var apikey = await context.ApiKeys
                .Include(a => a.ApiKeyObjectives)
                .ThenInclude(ao => ao.Objective)
                .FirstOrDefaultAsync(a => a.Name == apikeyName);

            return apikey != null ? _mapper.Map<ApiKeyDto>(apikey) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get apikey: {Name}", apikeyName);
            throw new ServiceException("Failed to get apikey", ex);
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
            _logger.LogError(ex, "Failed to get all apikeys");
            throw new ServiceException("Failed to get all apikeys", ex);
        }
    }

    public async Task UpdateApiKeyAsync(ApiKeyDto apikeyDto)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var apikey = await GetApiKeyWithObjectives(context, apikeyDto.Name);

            UpdateApiKeyProperties(apikey, apikeyDto);
            await UpdateObjectives(context, apikey, apikeyDto.Objectives);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "ApiKey not found: {Name}", apikeyDto.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update apikey: {Name}", apikeyDto.Name);
            throw new ServiceException("Failed to update apikey", ex);
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

    private void UpdateApiKeyProperties(ApiKey apikey, ApiKeyDto apikeyDto)
    {
        apikey.HasBeenSyncedOnce = apikeyDto.HasBeenSyncedOnce;
        apikey.LastSyncTime = apikeyDto.LastSyncTime;
    }

    private async Task UpdateObjectives(BarFooDbContext context, ApiKey apikey, IEnumerable<ObjectiveDto> newObjectives)
    {
        var existingObjectives = apikey.ApiKeyObjectives.ToDictionary(ao => ao.ObjectiveId, ao => ao);
        var newObjectivesDict = newObjectives.ToDictionary(o => o.Id, o => o);

        // Remove objectives no longer associated with the apikey
        foreach (var objectiveToRemove in existingObjectives.Values.Where(ao => !newObjectivesDict.ContainsKey(ao.ObjectiveId)))
        {
            apikey.ApiKeyObjectives.Remove(objectiveToRemove);
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
                apikey.ApiKeyObjectives.Add(new ApiKeyObjective
                {
                    ApiKey = apikey,
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