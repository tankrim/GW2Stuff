using BarFoo.Infrastructure.DTOs;

namespace BarFoo.Infrastructure.ApiClients;

public interface IGuildWars2ApiClient
{
    Task<IEnumerable<ObjectiveDto>> GetObjectivesAsync(string endpoint, string accountName);
}
