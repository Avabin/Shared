using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features;

public interface IEventingService
{
    Task PublishEventAsync(IEvent @event, string target = "");
    Task SendCommandAsync(ICommand command, string target = "");

    Task<T> QueryAsync<T>(IQuery query, string target = "") where T : IEvent;
}