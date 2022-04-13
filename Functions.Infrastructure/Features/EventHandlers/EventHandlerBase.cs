using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public abstract class EventHandlerBase<T> : IEventHandler<T> where T : IEvent
{
    public abstract Task HandleAsync(T      @event);
    public async    Task HandleAsync(IEvent @event) => await HandleAsync((T)@event);
}