using System.Reactive.Linq;
using Functions.Infrastructure.Features.EventHandlers;

namespace Functions.Infrastructure.Features.Events;

public class EventDispatcherHostedService<T> : IHostedService where T : IEvent
{
    private readonly IEventBus                                _eventBus;
    private readonly IEventHandlerFactory                     _handlerFactory;
    private readonly ILogger<EventDispatcherHostedService<T>> _logger;
    private          IDisposable?                             _sub;

    public EventDispatcherHostedService(IEventBus eventBus, IEventHandlerFactory handlerFactory, ILogger<EventDispatcherHostedService<T>> logger)
    {
        _eventBus       = eventBus;
        _handlerFactory = handlerFactory;
        _logger    = logger;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sub = _eventBus
              .OfType<T>()
              .Do(x => 
                          _handlerFactory
                             .Create<T>()
                             .HandleAsync(x))
              .Subscribe();
        
        _logger.LogDebug("Event dispatcher for {EventType} started", typeof(T).Name);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken  cancellationToken)
    {
        _sub?.Dispose();
        _logger.LogDebug("Event dispatcher for {EventType} stopped", typeof(T).Name);
        return Task.CompletedTask;
    }
}