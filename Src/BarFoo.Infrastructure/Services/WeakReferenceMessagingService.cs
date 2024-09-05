using BarFoo.Core.Interfaces;

using CommunityToolkit.Mvvm.Messaging;

namespace BarFoo.Infrastructure.Services;

public class WeakReferenceMessagingService : IMessagingService
{
    public void Send<TMessage>(TMessage message) where TMessage : class
    {
        WeakReferenceMessenger.Default.Send(message);
    }
}
