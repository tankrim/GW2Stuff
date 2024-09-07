using BarFoo.Core.Interfaces;

using BarFoo.Core.Messages;
using BarFoo.Presentation.Filters;

using CommunityToolkit.Mvvm.Input;

namespace BarFoo.Presentation.Interfaces;

public interface IFilterViewModel : IFilter
{
    FilterState FilterState { get; }
    bool IsLoading { get; set; }

    void HandleApiKeyAdded(object recipient, ApiKeyMessages.ApiKeyAddedMessage message);
    void HandleApiKeyDeleted(object recipient, ApiKeyMessages.ApiKeyDeletedMessage message);
    void HandleApiKeysLoaded(object recipient, ApiKeyMessages.ApiKeysLoadedMessage message);
    void HandleIsLoading(object recipient, IsLoadingMessage message);
    IRelayCommand SendFilterChangedCommand { get; }
}
