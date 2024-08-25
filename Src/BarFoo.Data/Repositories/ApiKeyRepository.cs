using BarFoo.Data.Contexts;
using BarFoo.Data.Exceptions;
using BarFoo.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BarFoo.Data.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly IDbContextFactory<BarFooDbContext> _contextFactory;
    private readonly ILogger<ApiKeyRepository> _logger;

    public ApiKeyRepository(
        IDbContextFactory<BarFooDbContext> contextFactory,
        ILogger<ApiKeyRepository> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiKey> AddAsync(ApiKey apiKey)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            await context.ApiKeys.AddAsync(apiKey);
            await context.SaveChangesAsync();
            return apiKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add apikey: {ApiKey}", apiKey.Name);
            throw new RepositoryException("Failed to add apikey", ex);
        }
    }

    public async Task<ApiKey> AddObjectivesAsync(ApiKey apikey, IEnumerable<Objective> objectives)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            context.Attach(apikey);
            foreach (var objective in objectives)
            {
                var existingObjective = await context.Objectives.FindAsync(objective.Id);
                if (existingObjective == null)
                {
                    await context.Objectives.AddAsync(objective);
                }
                apikey.ApiKeyObjectives.Add(new ApiKeyObjective { ApiKey = apikey, Objective = existingObjective ?? objective });
            }
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return apikey;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to add objectives for apikey: {ApiKey}", apikey.Name);
            throw new RepositoryException("Failed to add objectives", ex);
        }
    }

    public async Task<bool> DeleteAsync(string apikeyName)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var apikey = await context.ApiKeys.FindAsync(apikeyName);
            if (apikey != null)
            {
                context.ApiKeys.Remove(apikey);
                var result = await context.SaveChangesAsync();

                if (result! < 0)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete apikey: {Name}", apikeyName);
            throw new RepositoryException("Failed to delete apikey", ex);
        }
    }

    public async Task<bool> DeleteObjectivesAsync(ApiKey apikey)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var apikeyObjectives = await context.ApiKeyObjectives
                .Where(ao => ao.ApiKeyName == apikey.Name)
                .ToListAsync();

            context.ApiKeyObjectives.RemoveRange(apikeyObjectives);
            var result = await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return result! < 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to delete objectives for apikey: {ApiKey}", apikey.Name);
            throw new RepositoryException("Failed to delete objectives", ex);
        }
    }

    public async Task<bool> ExistsByNameAsync(string apikeyName)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ApiKeys.AnyAsync(a => a.Name == apikeyName);
    }

    public async Task<IEnumerable<ApiKey>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ApiKeys
            .Include(a => a.ApiKeyObjectives)
            .ThenInclude(ao => ao.Objective)
            .ToListAsync();
    }

    public async Task<ApiKey?> GetByNameAsync(string apikeyName)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var apiKeyData = await context.ApiKeys
            .Include(a => a.ApiKeyObjectives)
            .ThenInclude(ao => ao.Objective)
            .FirstOrDefaultAsync(a => a.Name == apikeyName);

        return apiKeyData;
    }

    public async Task<int> GetCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ApiKeys.CountAsync();
    }

    public async Task<bool> UpdateAsync(ApiKey apikey)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        try
        {
            var existingApiKey = await context.ApiKeys
                .Include(a => a.ApiKeyObjectives)
                .FirstOrDefaultAsync(a => a.Name == apikey.Name);

            if (existingApiKey == null)
            {
                throw new RepositoryException($"ApiKey not found: {apikey.Name}");
            }

            // Update apikey properties
            existingApiKey.HasBeenSyncedOnce = apikey.HasBeenSyncedOnce;
            existingApiKey.LastSyncTime = apikey.LastSyncTime;

            // Update objectives
            var existingObjectiveIds = existingApiKey.ApiKeyObjectives.Select(ao => ao.ObjectiveId).ToList();
            var newObjectiveIds = apikey.ApiKeyObjectives.Select(ao => ao.ObjectiveId).ToList();

            // Remove objectives that are no longer associated with the apikey
            var objectivesToRemove = existingApiKey.ApiKeyObjectives.Where(ao => !newObjectiveIds.Contains(ao.ObjectiveId)).ToList();
            foreach (var objectiveToRemove in objectivesToRemove)
            {
                existingApiKey.ApiKeyObjectives.Remove(objectiveToRemove);
            }

            // Add new objectives
            foreach (var newApiKeyObjective in apikey.ApiKeyObjectives.Where(ao => !existingObjectiveIds.Contains(ao.ObjectiveId)))
            {
                var objective = await context.Objectives.FindAsync(newApiKeyObjective.ObjectiveId);
                if (objective == null)
                {
                    objective = newApiKeyObjective.Objective;
                    context.Objectives.Add(objective);
                }
                existingApiKey.ApiKeyObjectives.Add(new ApiKeyObjective
                {
                    ApiKey = existingApiKey,
                    Objective = objective
                });
            }

            var result = await context.SaveChangesAsync();

            return result! < 0;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update apikey: {ApiKey}", apikey.Name);
            throw new RepositoryException("Failed to update apikey", ex);
        }
    }
}
