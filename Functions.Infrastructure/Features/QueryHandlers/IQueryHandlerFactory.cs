using Functions.Infrastructure.Features.EventHandlers;

namespace Functions.Infrastructure.Features.QueryHandlers;

public interface IQueryHandlerFactory
{
    IQueryHandler Create(Type queryType);
}