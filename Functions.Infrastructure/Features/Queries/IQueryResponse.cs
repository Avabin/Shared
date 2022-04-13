using Functions.Infrastructure.Features.Events;

namespace Functions.Infrastructure.Features.Queries;

public interface IQueryResponse<T> : IQueryResponse
{
    public T? Result { get; set; }
}

public interface IQueryResponse : IEvent
{
}