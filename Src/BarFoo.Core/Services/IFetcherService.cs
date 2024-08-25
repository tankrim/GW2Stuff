using BarFoo.Infrastructure.DTOs;

namespace BarFoo.Core.Services;

public interface IFetcherService
{
    Task<IEnumerable<ObjectiveDto>> FetchObjectivesForApiKeyAsync(string apikeyName, CancellationToken cancellationToken);
    Task<IDictionary<string, IEnumerable<ObjectiveDto>>> FetchObjectivesForAllApiKeysAsync(IEnumerable<string> apikeyNames, CancellationToken cancellationToken);
}