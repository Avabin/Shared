using Functions.Infrastructure.Features.EventHandlers;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Options;

namespace Functions.Infrastructure.Features.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventHandler<T, TEvent>(this IServiceCollection services) where T : class, IEventHandler<TEvent> where TEvent : IEvent
    {
        services.AddHostedService<EventDispatcherHostedService<TEvent>>();
        services.AddTransient<IEventHandler<TEvent>, T>();
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