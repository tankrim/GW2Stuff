using BarFoo.Core.DTOs;

namespace BarFoo.Core.Interfaces;

public interface IGuildWars2ApiClient
{
    Task<IEnumerable<ObjectiveDto>> GetObjectivesAsync(string endpoint, string accountName);
}
