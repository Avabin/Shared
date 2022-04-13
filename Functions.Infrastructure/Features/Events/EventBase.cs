namespace Functions.Infrastructure.Features.Events;

public abstract class EventBase : IEvent
{
    public Guid   CorrelationId { get; set; } = Guid.NewGuid();
    public string ApiKey        { get; set; } = "";
}