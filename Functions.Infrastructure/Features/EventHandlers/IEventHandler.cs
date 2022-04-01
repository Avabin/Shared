using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public interface IEventHandler<in T> where T : IEvent

{
    Task Handle(T @event);
}