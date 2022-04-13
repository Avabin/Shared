using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.EventHandlers;

public interface IRespondingEventHandlerFactory
{
    IRespondingEventHandler Create(Type eventType);
}

public class RespondingEventHandlerFactory : IRespondingEventHandlerFactory
{
    private readonly IServiceProvider _provider;

    public RespondingEventHandlerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }


    public IRespondingEventHandler Create(Type eventType)
    {
        var eventHandlerType = typeof(IRespondingEventHandler<>).MakeGenericType(eventType);
        return (IRespondingEventHandler) _provider.GetRequiredService(eventHandlerType);
    }
}