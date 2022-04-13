namespace Functions.Infrastructure.Features.Events;

public interface IEvent
{
    Guid CorrelationId { get; set; }
    string ApiKey { get; set; }
}

public interface IRequest : IEvent
{
    public string Source { get; set; }
}