namespace BarFoo.Core.Interfaces;

public interface IMessagingService
{
    void Send<TMessage>(TMessage message) where TMessage : class;
}
