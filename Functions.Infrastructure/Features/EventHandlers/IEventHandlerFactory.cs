using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public interface IEventHandlerFactory
{
    IEventHandler<T> Create<T>() where T : IEvent;
}