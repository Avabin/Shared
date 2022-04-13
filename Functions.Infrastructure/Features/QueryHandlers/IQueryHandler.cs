using Functions.Infrastructure.Features.EventHandlers;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Queries;

namespace Functions.Infrastructure.Features.QueryHandlers;

public interface IQueryHandler<in TQuery> : IEventHandler<TQuery>,IQueryHandler where TQuery : IQuery
{
    new Task<IQueryResponse> HandleAsync(TQuery query);
}

public interface IQueryHandler : IEventHandler
{
    Task<IQueryResponse> HandleAsync(IQuery query);
}