using System.Collections.ObjectModel;

using BarFoo.Core.DTOs;
using BarFoo.Core.Interfaces;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BarFoo.Core.Messages;

public class ApiKeyMessages
{
    public sealed class ApiKeyAddedMessage : ValueChangedMessage<ApiKeyDto>
    {
        public ApiKeyAddedMessage(ApiKeyDto dto)
            : base(dto) { }
    }

    public sealed class ApiKeyDeletedMessage : ValueChangedMessage<string>
    {
        public ApiKeyDeletedMessage(string keyName)
            : base(keyName) { }
    }
    public sealed class ApiKeysLoadedMessage : ValueChangedMessage<IEnumerable<ApiKeyDto>>
    {
        public ApiKeysLoadedMessage(IEnumerable<ApiKeyDto> dtos)
            : base(dtos) { }
    }

    public sealed class ApiKeyStateChangedMessage
    {
    }

    public sealed class ApiKeysUpdatedMessage : ValueChangedMessage<bool>
    {
        public ApiKeysUpdatedMessage() : base(true) { }
    }
}

public class ObjectiveMessages
{
    public sealed class ObjectivesChangedMessage : ValueChangedMessage<(string PropertyName, ObservableCollection<ObjectiveWithOthersDto> Value)>
    {
        public ObjectivesChangedMessage(string propertyName, ObservableCollection<ObjectiveWithOthersDto> value)
            : base((propertyName, value)) { }
    }
}

public sealed class IsUpdatingMessage : ValueChangedMessage<bool>
{
    public IsUpdatingMessage(bool isUpdating) : base(isUpdating) { }
}

public sealed class IsLoadingMessage
{
    public bool IsLoading { get; }

    public IsLoadingMessage(bool isLoading)
    {
        IsLoading = isLoading;
    }
}

public sealed class FilterChangedMessage : ValueChangedMessage<IFilter>
{
    public FilterChangedMessage(IFilter filter) : base(filter) { }
}
