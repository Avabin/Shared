using System.Net;
using System.Reactive.Linq;
using System.Text;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Functions.Infrastructure.Features;

internal class EventingService : IEventingService
{
    private const    int                        TimeoutInSeconds = 15;
    private readonly IEventBus                  _eventBus;
    private readonly IOptions<EventingSettings> _options;
    private readonly ILogger<EventingService>   _logger;
    private readonly JsonSerializerSettings     _jsonSettings;
    protected        EventingSettings           Settings => _options.Value;
    protected        string                     Endpoint => $"{Settings.BrokerNamespace}/{Settings.BrokerName}";

    private readonly Lazy<HttpClient> _httpClient;
    protected        HttpClient       HttpClient => _httpClient.Value;

    public EventingService(IEventBus eventBus, IOptions<EventingSettings> options, ILogger<EventingService> logger)
    {
        _eventBus    = eventBus;
        _options     = options;
        _logger = logger;
        _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling               = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        _httpClient = new Lazy<HttpClient>(() =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Settings.BrokerBaseUrl)
            };
            httpClient.Timeout = TimeSpan.FromSeconds(TimeoutInSeconds);
            return httpClient;
        });
    }

    public async Task PublishEventAsync(IEvent @event, string target = "") => 
        await PublishEvent(@event, target);

    public async Task SendCommandAsync(ICommand command, string target = "") => 
        await PublishEvent(command, target);

    public async Task<T> QueryAsync<T>(IQuery query, string target = "") where T : IEvent
    {
        // send query
        await PublishEvent(query, target);

        // listen for response
        var result = await _eventBus.OfType<T>()
                                    .FirstAsync(x => x.CorrelationId == query.CorrelationId)
                                    .Timeout(TimeSpan.FromSeconds(TimeoutInSeconds));

        return result;
    }
    
    public async Task PublishEvent(IEvent @event, string target = "") 
    {
        _logger.LogInformation("Publishing event {EventType} ({EventId}) to {Target}", @event.GetType().Name, @event.CorrelationId, target);
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        
        if (target is not "")
            request.Headers.Add("Ce-Subject", target);

        request.Headers.Add("Ce-Id",     @event.CorrelationId.ToString());
        request.Headers.Add("Ce-Source", Settings.Source);
        request.Headers.Add("Ce-specversion", "1.0");
        request.Headers.Add("Ce-Type",   @event.GetType().Name);
        var serialized = JsonConvert.SerializeObject(@event, _jsonSettings);
        
        request.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
        
        var result = await HttpClient.SendAsync(request);

        if (result.StatusCode != HttpStatusCode.Accepted)
        {
            _logger.LogError("Failed to publish event {EventType} ({EventId}) to {Target}", @event.GetType().Name, @event.CorrelationId, target);
            _logger.LogError("Status code: {StatusCode}", result.StatusCode);
            _logger.LogError("Reason: {Reason}", result.ReasonPhrase);
            _logger.LogError("Content: {Content}", await result.Content.ReadAsStringAsync());
        }
    }
}