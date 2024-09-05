using BarFoo.Core.DTOs;

namespace BarFoo.Core.Interfaces;

public interface IStore
{
    Task<ApiKeyDto> CreateApiKeyAsync(string apikeyName, string apiKey);
    Task DeleteApiKeyAsync(string apikeyName);
    Task<ApiKeyDto?> GetApiKeyAsync(string apikeyName);
    Task<IEnumerable<ApiKeyDto>> GetAllApiKeysAsync();
    Task<IEnumerable<ObjectiveDto>> GetAllObjectivesAsync();
    Task<IEnumerable<ObjectiveWithOthersDto>> GetAllObjectivesWithOthersAsync();
    Task<IEnumerable<ObjectiveDto>> GetFilteredObjectivesAsync(Func<ObjectiveDto, bool> predicate);
    Task<IEnumerable<ObjectiveDto>> GetObjectivesForApiKeyAsync(string apikeyName);
    Task InitializeAsync();
    Task<ApiKeyDto> SyncObjectivesForApiKeyAsync(string apikeyName);
    Task SyncObjectivesForAllApiKeysAsync();
    bool IsInitialized { get; }
}