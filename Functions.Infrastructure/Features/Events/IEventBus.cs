namespace Functions.Infrastructure.Features.Events;

public interface IEventBus
{
    void Publish(IEvent @event);
    IObservable<T> OfType<T>() where T : IEvent;
}