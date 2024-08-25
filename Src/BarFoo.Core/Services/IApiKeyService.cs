using BarFoo.Infrastructure.DTOs;

namespace BarFoo.Core.Services;

public interface IApiKeyService
{
    Task<ApiKeyDto> CreateApiKeyAsync(string name, string token);
    Task DeleteApiKeyAsync(string apikeyName);
    Task<ApiKeyDto?> GetApiKeyAsync(string apikeyName);
    Task<IEnumerable<ApiKeyDto>> GetAllApiKeysAsync();
    Task UpdateApiKeyAsync(ApiKeyDto apikeyDto);
}