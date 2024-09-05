using BarFoo.Core.DTOs;

namespace BarFoo.Core.Interfaces;

public interface IFetcherService
{
    Task<IEnumerable<ObjectiveDto>> FetchObjectivesForApiKeyAsync(string apikeyName, CancellationToken cancellationToken);
    Task<IDictionary<string, IEnumerable<ObjectiveDto>>> FetchObjectivesForAllApiKeysAsync(IEnumerable<string> apikeyNames, CancellationToken cancellationToken);
}