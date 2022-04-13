using Functions.Infrastructure.Features.EventHandlers;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Queries;

namespace Functions.Infrastructure.Features.QueryHandlers;

public abstract class QueryHandler<TQuery> : IQueryHandler<TQuery> where TQuery : IQuery
{
    public abstract Task<IQueryResponse> HandleAsync(TQuery query);

    async        Task IEventHandler<TQuery>.HandleAsync(TQuery @event) => await HandleAsync(@event);
    public async Task<IQueryResponse>       HandleAsync(IQuery query)  => await HandleAsync((TQuery) query);
    public async Task                       HandleAsync(IEvent @event) => await HandleAsync((TQuery) @event);
}