using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public interface IRespondingEventHandler<in TEvent> : IRespondingEventHandler, IEventHandler<TEvent> where TEvent : IEvent
{
    new Task<IEvent> HandleAsync(TEvent @event);
}

public interface IRespondingEventHandler : IEventHandler
{
    new Task<IEvent> HandleAsync(IEvent @event);
}