using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Queries;
using Functions.Infrastructure.Features.QueryHandlers;

namespace Functions.Infrastructure.Features;

public interface IEventingService
{
    Task    SendAsync(IEvent              @event,  string target = "");
    Task<T> SendAndReceiveAsync<T>(IEvent @event,  string target = "") where T : IEvent;
    Task    SendCommandAsync(ICommand     command, string target = "");
    Task<T> SendQueryAsync<T>(IQuery      query,   string target = "") where T : IQueryResponse;
    Task<T> AwaitResponseAsync<T>(Guid?   id                    = null) where T : IEvent;
    Task    PublishAsync(IEvent           @event, string target = "");
    Task    NotifyAsync(IEvent           @event);
}