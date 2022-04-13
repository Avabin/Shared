using System.Reflection;
using Functions.Infrastructure.Features.EventHandlers;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Options;
using Functions.Infrastructure.Features.QueryHandlers;

namespace Functions.Infrastructure.Features.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventHandler<T, TEvent>(this IServiceCollection services) where T : class, IEventHandler<TEvent> where TEvent : IEvent
    {
        services.AddHostedService<EventDispatcherHostedService<TEvent>>();
        services.AddTransient<IEventHandler<TEvent>, T>();
        return services;
    }

    public static IServiceCollection AddEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var eventHandlers = assembly.GetExportedTypes().Where(type => type.IsAssignableTo(typeof(IEventHandler))).ToList();

        foreach (var handler in eventHandlers)
        {
            var eventHandler = handler.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));
            var respondingEventHandler = handler.GetInterfaces()
                                                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() ==
                                                                     typeof(IRespondingEventHandler<>));
            var queryHandler = handler.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<>));

            if (eventHandler is not null)
            {
                services.AddTransient(eventHandler, handler);
            }
            
            if (respondingEventHandler is not null && eventHandler is not null)
            {
                services.AddTransient(eventHandler,           handler);
                services.AddTransient(respondingEventHandler, handler);
            }

            if (queryHandler is not null && eventHandler is not null)
            {
                services.AddTransient(eventHandler, handler);
                services.AddTransient(queryHandler, handler);
            }
        }
        
        return services;
    }

    public static IServiceCollection AddEventing(this IServiceCollection services, Action<EventingSettings> configure) => 
        services.Configure(configure).AddEventingCore();

    public static IServiceCollection AddEventing(this IServiceCollection services, IConfigurationSection section) => 
        services.Configure<EventingSettings>(section).AddEventingCore();

    private static IServiceCollection AddEventingCore(this IServiceCollection services) => 
        services.AddSingleton<IEventingService, EventingService>()
                .AddSingleton<IEventBus, EventBus>()
                .AddTransient<IEventHandlerFactory, EventHandlerFactory>();
}