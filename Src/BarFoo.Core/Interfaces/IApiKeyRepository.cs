using BarFoo.Domain.Entities;

namespace BarFoo.Core.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey> AddAsync(ApiKey account);
    Task<ApiKey> AddObjectivesAsync(ApiKey account, IEnumerable<Objective> objectives);
    Task<bool> DeleteAsync(string name);
    Task<bool> DeleteObjectivesAsync(ApiKey account);
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<ApiKey>> GetAllAsync();
    Task<ApiKey?> GetByNameAsync(string name);
    Task<int> GetCountAsync();
    Task<bool> UpdateAsync(ApiKey account);
}
