using BarFoo.Core.DTOs;

namespace BarFoo.Core.Interfaces;

public interface IApiKeyService
{
    Task<ApiKeyDto> CreateApiKeyAsync(string name, string token);
    Task DeleteApiKeyAsync(string apikeyName);
    Task<ApiKeyDto?> GetApiKeyAsync(string apikeyName);
    Task<IEnumerable<ApiKeyDto>> GetAllApiKeysAsync();
    Task UpdateApiKeyAsync(ApiKeyDto apikeyDto);
}