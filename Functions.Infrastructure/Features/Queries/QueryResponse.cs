namespace Functions.Infrastructure.Features.Queries;

public abstract class QueryResponse<T> : IQueryResponse<T>
{
    public Guid   CorrelationId { get; set; } = Guid.NewGuid();
    public string ApiKey        { get; set; } = "";
    public T?     Result        { get; set; }
}