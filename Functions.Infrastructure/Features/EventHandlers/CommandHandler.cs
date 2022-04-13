using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public abstract class CommandHandler<T> : EventHandlerBase<T> where T : IEvent
{
}