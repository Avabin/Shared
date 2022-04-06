namespace Functions.Infrastructure.Features.Options;

public class EventingSettings
{
    public string ApiKey          { get; set; } = "";
    public string BrokerBaseUrl   { get; set; } = "";
    public string BrokerNamespace { get; set; } = "";
    public string BrokerName      { get; set; } = "";
    
    public string Source { get; set; } = "";
}