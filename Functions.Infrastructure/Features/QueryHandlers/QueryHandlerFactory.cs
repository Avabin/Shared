namespace Functions.Infrastructure.Features.QueryHandlers;

public class QueryHandlerFactory : IQueryHandlerFactory
{
    private readonly IServiceProvider _provider;

    public QueryHandlerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IQueryHandler Create(Type queryType)
    {
        var queryHandlerType = typeof(IQueryHandler<>).MakeGenericType(queryType);
        return (IQueryHandler) _provider.GetRequiredService(queryHandlerType);
    }
}