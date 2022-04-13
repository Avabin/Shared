using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public interface IEventHandler<in T> : IEventHandler where T : IEvent

{
    Task HandleAsync(T @event);
}

public interface IEventHandler
{
    Task HandleAsync(IEvent @event);
}