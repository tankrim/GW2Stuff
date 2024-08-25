using BarFoo.Infrastructure.ApiClients;
using BarFoo.Infrastructure.DTOs;
using BarFoo.Infrastructure.Exceptions;

using Microsoft.Extensions.Logging;

namespace BarFoo.Core.Services;

public class FetcherService : IFetcherService
{
    private readonly IGuildWars2ApiClient _apiClient;
    private readonly ILogger<FetcherService> _logger;
    private static readonly string[] _endpoints = { "daily", "weekly", "special" };
    private const int DelayBetweenRequests = 500; // milliseconds

    public FetcherService(IGuildWars2ApiClient apiClient, ILogger<FetcherService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ObjectiveDto>> FetchObjectivesForApiKeyAsync(string apikeyName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apikeyName))
            throw new ArgumentException("ApiKey name cannot be null or whitespace.", nameof(apikeyName));

        _logger.LogInformation("Starting to fetch objectives for apikey {ApiKey}", apikeyName);

        var tasks = _endpoints.Select(endpoint => FetchObjectivesForApiKeyAndEndpoint(apikeyName, endpoint, cancellationToken));
        var results = await Task.WhenAll(tasks);

        var allObjectives = results.SelectMany(r => r).ToList();

        _logger.LogInformation("Fetched {Count} objectives for apikey {ApiKey}", allObjectives.Count, apikeyName);

        return allObjectives;
    }

    public async Task<IDictionary<string, IEnumerable<ObjectiveDto>>> FetchObjectivesForAllApiKeysAsync(IEnumerable<string> apikeyNames, CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, IEnumerable<ObjectiveDto>>();
        var errors = new List<Exception>();

        foreach (var apikeyName in apikeyNames)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var objectives = await FetchObjectivesForApiKeyAsync(apikeyName, cancellationToken);
                results[apikeyName] = objectives;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ApiServiceUnavailableException ex)
            {
                _logger.LogWarning(ex, "Guild Wars 2 API is temporarily unavailable for apikey {ApiKeyName}", apikeyName);
                errors.Add(new Exception($"API temporarily unavailable for apikey {apikeyName}", ex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch objectives for apikey {ApiKeyName}", apikeyName);
                errors.Add(new Exception($"Failed to fetch objectives for apikey {apikeyName}", ex));
            }
        }

        if (errors.Count != 0)
        {
            throw new AggregateException("One or more errors occurred while fetching objectives", errors);
        }

        return results;
    }

    private async Task<IEnumerable<ObjectiveDto>> FetchObjectivesForApiKeyAndEndpoint(string apikeyName, string endpoint, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching objectives from endpoint {Endpoint} for apikey {ApiKey}", endpoint, apikeyName);

            cancellationToken.ThrowIfCancellationRequested();

            var objectiveDtos = await _apiClient.GetObjectivesAsync(endpoint, apikeyName);

            _logger.LogInformation("Fetched {Count} objectives from endpoint {Endpoint} for apikey {ApiKey}", objectiveDtos.Count(), endpoint, apikeyName);

            await Task.Delay(DelayBetweenRequests, cancellationToken);

            return objectiveDtos;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation cancelled while fetching objectives from {Endpoint} for {ApiKey}", endpoint, apikeyName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch objectives from {Endpoint} for {ApiKey}", endpoint, apikeyName);
            throw;
        }
    }
}