using CommunityToolkit.Mvvm.Messaging;

namespace BarFoo.Core.Interfaces;

public interface IMessagingService
{
    void Register<TMessage>(object recipient, Action<object, TMessage> action) where TMessage : class;
    void Unregister<TMessage>(object recipient) where TMessage : class;
    void Send<TMessage>(TMessage message) where TMessage : class;
}
