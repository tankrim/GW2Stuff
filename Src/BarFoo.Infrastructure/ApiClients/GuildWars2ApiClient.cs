using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

using BarFoo.Core.DTOs;
using BarFoo.Core.Interfaces;
using BarFoo.Infrastructure.Exceptions;
using BarFoo.Infrastructure.Models;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

namespace BarFoo.Infrastructure.ApiClients;

public class GuildWars2ApiClient : IGuildWars2ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly ILogger<GuildWars2ApiClient> _logger;

    public GuildWars2ApiClient(HttpClient httpClient, IApiKeyRepository apiKeyRepository, ILogger<GuildWars2ApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri("https://api.guildwars2.com/");

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception.Exception, "Request failed. Retry attempt {RetryCount} after {RetryInterval}s", retryCount, timeSpan.TotalSeconds);
                }
            );
    }

    //public async Task<IEnumerable<ObjectiveDto>> GetObjectivesAsync(string endpoint, string accountName)
    //{
    //    ArgumentException.ThrowIfNullOrWhiteSpace(nameof(endpoint));
    //    ArgumentException.ThrowIfNullOrWhiteSpace(nameof(accountName));

    //    var apiKey = await GetApiKeyForAccount(accountName);

    //    // Clear any existing headers to avoid duplicates
    //    _httpClient.DefaultRequestHeaders.Clear();

    //    try
    //    {
    //        // Use query parameter instead of header
    //        var response = await _retryPolicy.ExecuteAsync(() =>
    //            _httpClient.GetAsync($"v2/account/wizardsvault/{endpoint}?access_token={apiKey}"));

    //        await HandleResponseStatusCode(response, accountName, endpoint);

    //        var content = await response.Content.ReadAsStringAsync();

    //        if (string.IsNullOrWhiteSpace(content) || content == "{}")
    //        {
    //            _logger.LogError("API returned empty response for apiEndpoint {Endpoint}", endpoint);
    //            throw new ApiResponseFormatException($"Unexpected empty response from Guild Wars 2 API. Endpoint: {endpoint}");
    //        }

    //        try
    //        {
    //            var apiResponse = DeserializeApiResponse(content)
    //                    ?? throw new ApiResponseFormatException($"Invalid response format from Guild Wars 2 API. Endpoint: {endpoint}");

    //            return MapApiResponseToObjectives(apiResponse, endpoint, accountName);
    //        }
    //        catch (JsonException ex)
    //        {
    //            _logger.LogError(ex, "Failed to deserialize API response for apiEndpoint {Endpoint}", endpoint);
    //            throw new ApiResponseFormatException($"Invalid response format from Guild Wars 2 API. Endpoint: {endpoint}", ex);
    //        }
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        _logger.LogError(ex, "Network error occurred while fetching objectives for apiEndpoint {Endpoint}", endpoint);
    //        throw new ApiConnectionException($"Failed to connect to the Guild Wars 2 API. Endpoint: {endpoint}", ex);
    //    }
    //    catch (TaskCanceledException ex)
    //    {
    //        _logger.LogError(ex, "Request timed out for apiEndpoint {Endpoint}", endpoint);
    //        throw new ApiTimeoutException($"Request to Guild Wars 2 API timed out. Endpoint: {endpoint}", ex);
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "Failed to deserialize API response for apiEndpoint {Endpoint}", endpoint);
    //        throw new ApiResponseFormatException($"Invalid response format from Guild Wars 2 API. Endpoint: {endpoint}", ex);
    //    }
    //    catch (Exception ex) when (ex is not ApiClientException)
    //    {
    //        _logger.LogError(ex, "Unexpected error occurred while fetching objectives for apiEndpoint {Endpoint}", endpoint);
    //        throw new ApiClientException($"An unexpected error occurred while communicating with the Guild Wars 2 API. Endpoint: {endpoint}", ex);
    //    }
    //}

    public async Task<IEnumerable<ObjectiveDto>> GetObjectivesAsync(string endpoint, string accountName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(endpoint));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(accountName));

        var apiKey = await GetApiKeyForAccount(accountName);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            _httpClient.DefaultRequestHeaders.Add("X-Schema-Version", "latest");
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync($"v2/account/wizardsvault/{endpoint}"));

            await HandleResponseStatusCode(response, accountName, endpoint);

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content) || content == "{}")
            {
                _logger.LogError("API returned empty response for apiEndpoint {Endpoint}", endpoint);
                throw new ApiResponseFormatException($"Unexpected empty response from Guild Wars 2 API. Endpoint: {endpoint}");
            }

            try
            {
                var apiResponse = DeserializeApiResponse(content)
                        ?? throw new ApiResponseFormatException($"Invalid response format from Guild Wars 2 API. Endpoint: {endpoint}");

                return MapApiResponseToObjectives(apiResponse, endpoint, accountName);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize API response for apiEndpoint {Endpoint}", endpoint);
                throw new ApiResponseFormatException($"Invalid response format from Guild Wars 2 API. Endpoint: {endpoint}", ex);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while fetching objectives for apiEndpoint {Endpoint}", endpoint);
            throw new ApiConnectionException($"Failed to connect to the Guild Wars 2 API. Endpoint: {endpoint}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timed out for apiEndpoint {Endpoint}", endpoint);
            throw new ApiTimeoutException($"Request to Guild Wars 2 API timed out. Endpoint: {endpoint}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize API response for apiEndpoint {Endpoint}", endpoint);
            throw new ApiResponseFormatException($"Invalid response format from Guild Wars 2 API. Endpoint: {endpoint}", ex);
        }
        catch (Exception ex) when (ex is not ApiClientException)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching objectives for apiEndpoint {Endpoint}", endpoint);
            throw new ApiClientException($"An unexpected error occurred while communicating with the Guild Wars 2 API. Endpoint: {endpoint}", ex);
        }
    }

    private async Task HandleResponseStatusCode(HttpResponseMessage response, string accountName, string endpoint)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return;
            case HttpStatusCode.Unauthorized:
                _logger.LogWarning("Unauthorized access for account {Account} on apiEndpoint {Endpoint}", accountName, endpoint);
                throw new ApiUnauthorizedException($"The provided API key for account {accountName} is invalid or has expired.");
            case HttpStatusCode.Forbidden:
                _logger.LogWarning("Forbidden access for account {Account} on apiEndpoint {Endpoint}", accountName, endpoint);
                throw new ApiForbiddenException($"The API key for account {accountName} does not have the required permissions for this request.");
            case HttpStatusCode.NotFound:
                _logger.LogWarning("Resource not found for apiEndpoint {Endpoint}", endpoint);
                throw new ApiNotFoundException($"The requested resource was not found. Endpoint: {endpoint}");
            case HttpStatusCode.TooManyRequests:
                _logger.LogWarning("Rate limit exceeded for account {Account} on apiEndpoint {Endpoint}", accountName, endpoint);
                throw new ApiRateLimitException($"Rate limit exceeded for account {accountName}. Please try again later.");
            case HttpStatusCode.ServiceUnavailable:
                _logger.LogWarning("Guild Wars 2 API is temporarily unavailable for account {Account} on apiEndpoint {Endpoint}", accountName, endpoint);
                throw new ApiServiceUnavailableException($"The Guild Wars 2 API is temporarily unavailable. Please try again later.");
            default:
                _logger.LogError("Unexpected HTTP status code {StatusCode} received for account {Account} on apiEndpoint {Endpoint}", response.StatusCode, accountName, endpoint);
                await Task.CompletedTask;
                throw new ApiClientException($"Unexpected HTTP status code {response.StatusCode} received from the Guild Wars 2 API.");
        }
    }

    private static ApiResponse? DeserializeApiResponse(string content)
    {
        try
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };
            var options = jsonSerializerOptions;
            return JsonSerializer.Deserialize<ApiResponse>(content, options);
        }
        catch (JsonException ex)
        {
            throw new ApiResponseFormatException("Failed to deserialize API response: " + ex.Message, ex);
        }
    }

    private static List<ObjectiveDto> MapApiResponseToObjectives(ApiResponse apiResponse, string apiEndpoint, string accountName)
    {
        var lowestExpectedNumberOfObjectivesInObjectivesArray = 4;

        if (apiResponse.Objectives.Count < lowestExpectedNumberOfObjectivesInObjectivesArray)
        {
            throw new JsonException("The JSON objectives array was empty or contained too few objectives.");
        }

        var result = apiResponse.Objectives.Select(o => new ObjectiveDto(
            o.Id,
            o.Title,
            o.Track,
            o.Acclaim,
            o.ProgressCurrent,
            o.ProgressComplete,
            o.Claimed,
            apiEndpoint,
            accountName
        )).ToList();

        return result;
    }

    private async Task<string> GetApiKeyForAccount(string accountName)
    {
        var apiKey = await _apiKeyRepository.GetByNameAsync(accountName);
        return apiKey?.Key ?? throw new InvalidOperationException($"No API key found for account {accountName}");
    }
}
