using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public abstract class RespondingEventHandler<TEvent> : EventHandlerBase<TEvent>,IRespondingEventHandler<TEvent> where TEvent : IEvent
{
    async Task IEventHandler<TEvent>.HandleAsync(TEvent @event) => await HandleAsync(@event);
    async    Task<IEvent>       IRespondingEventHandler.HandleAsync(IEvent @event) => await HandleAsync((TEvent) @event);

    public abstract override Task<IEvent> HandleAsync(TEvent @event);
}