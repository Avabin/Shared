using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.Queries;

public abstract class QueryBase : EventBase, IQuery
{
    public string Source { get; set; } = "";
}