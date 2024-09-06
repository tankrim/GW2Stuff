using BarFoo.Core.Interfaces;

using CommunityToolkit.Mvvm.Messaging;

namespace BarFoo.Infrastructure.Services;

public class WeakReferenceMessagingService : IMessagingService
{
    private readonly WeakReferenceMessenger _messenger;

    public WeakReferenceMessagingService()
    {
        _messenger = WeakReferenceMessenger.Default;
    }

    public void Register<TMessage>(object recipient, Action<object, TMessage> action) where TMessage : class
    {
        _messenger.Register<TMessage>(recipient, (r, m) => action(r, m));
    }

    public void Unregister<TMessage>(object recipient) where TMessage : class
    {
        _messenger.Unregister<TMessage>(recipient);
    }

    public void Send<TMessage>(TMessage message) where TMessage : class
    {
        _messenger.Send(message);
    }
}
