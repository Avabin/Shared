using System.Net;
using System.Reactive.Linq;
using System.Text;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Options;
using Functions.Infrastructure.Features.Queries;
using Functions.Infrastructure.Features.QueryHandlers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Functions.Infrastructure.Features;

internal class EventingService : IEventingService
{
    private string                    SinkUrl          => Environment.GetEnvironmentVariable("K_SINK") ?? $"{Settings.BrokerBaseUrl}/{Endpoint}";
    private const    int                        TimeoutInSeconds = 3600;
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
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(TimeoutInSeconds);
            return httpClient;
        });
    }

    public async Task SendAsync(IEvent @event, string target = "") => 
        await PublishAsync(@event, target);

    public async Task<T> SendAndReceiveAsync<T>(IEvent @event, string target = "") where T : IEvent
    {
        var id = @event.CorrelationId;
        await PublishAsync(@event, target);
        return await AwaitResponseAsync<T>(id);
    }

    public async Task SendCommandAsync(ICommand command, string target = "") => 
        await PublishAsync(command, target);

    public async Task<T> SendQueryAsync<T>(IQuery query, string target = "") where T : IQueryResponse
    {
        var source = Settings.Source;
        query.Source = source; // return result to query source
        // send query
        
        await PublishAsync(query, target);

        // listen for response
        return await AwaitResponseAsync<T>(@query.CorrelationId);
    }
    
    public async Task<T> AwaitResponseAsync<T>(Guid? id = null) where T : IEvent
    {
        var responseObservable =_eventBus.OfType<T>();

        if (id is not null) 
            responseObservable = responseObservable.FirstAsync(x => x.CorrelationId == id);

        responseObservable = responseObservable.Timeout(TimeSpan.FromSeconds(TimeoutInSeconds));

        return await responseObservable;
    }

    public async Task PublishAsync(IEvent @event, string target = "") 
    {
        _logger.LogInformation("Publishing event {EventType} ({EventId}) to {Target}", @event.GetType().Name, @event.CorrelationId, target);
        var request = new HttpRequestMessage(HttpMethod.Post, SinkUrl);
        
        if (target is not "")
            request.Headers.Add("Ce-Subject", target);

        request.Headers.Add("Ce-Id",     @event.CorrelationId.ToString());
        request.Headers.Add("Ce-Source", Settings.Source);
        request.Headers.Add("Ce-specversion", "1.0");
        var eventType = $"xyz.avabin.{@event.GetType().Name}";
        request.Headers.Add("Ce-Type",   eventType);
        request.Headers.Add("Ce-Time",   DateTime.UtcNow.ToString("o"));
        
        _logger.LogTrace("Broker url is {BrokerUrl}", request.RequestUri);
        _logger.LogTrace("Event id {EventId}, Event source {EventSource}, Specversion {SpecVersion}, Event type {EventType}, Event subject {EventSubject}", @event.CorrelationId, Settings.Source, "1.0",eventType, target);
        @event.ApiKey = Settings.ApiKey;
        var serialized = JsonConvert.SerializeObject(@event, _jsonSettings);
        _logger.LogTrace("Event data: {EventData}", serialized);
        
        request.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
        
        var result = await HttpClient.SendAsync(request);

        if (result.StatusCode != HttpStatusCode.Accepted)
        {
            _logger.LogError("Failed to publish event {EventType} ({EventId}) to {Target}", @event.GetType().Name, @event.CorrelationId, target);
        }
        _logger.LogInformation("Publishing {EventId} status code {StatusCode}", @event.CorrelationId, result.StatusCode);
        _logger.LogTrace("Status code: {StatusCode}", result.StatusCode);
        _logger.LogTrace("Reason: {Reason}",          result.ReasonPhrase);
        _logger.LogTrace("Content: {Content}", await result.Content.ReadAsStringAsync());
    }

    public async Task NotifyAsync(IEvent @event)
    {
        _logger.LogInformation("Publishing notification {EventType} ({EventId})", @event.GetType().Name, @event.CorrelationId);
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);

        request.Headers.Add("Ce-Id",          @event.CorrelationId.ToString());
        request.Headers.Add("Ce-Source",      Settings.Source);
        request.Headers.Add("Ce-specversion", "1.0");
        var eventType = $"xyz.avabin.{@event.GetType().Name}";
        request.Headers.Add("Ce-Type", eventType);
        request.Headers.Add("Ce-Time", DateTime.UtcNow.ToString("o"));
        
        _logger.LogTrace("Broker url is {BrokerUrl}", request.RequestUri);
        _logger.LogTrace("Event id {EventId}, Event source {EventSource}, Specversion {SpecVersion}, Event type {EventType}", @event.CorrelationId, Settings.Source, "1.0",eventType);
        @event.ApiKey = Settings.ApiKey;
        var serialized = JsonConvert.SerializeObject(@event, _jsonSettings);
        _logger.LogTrace("Event data: {EventData}", serialized);
        
        request.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
        
        var result = await HttpClient.SendAsync(request);

        if (result.StatusCode != HttpStatusCode.Accepted)
        {
            _logger.LogError("Failed to publish event {EventType} ({EventId})", @event.GetType().Name, @event.CorrelationId);
        }
        _logger.LogInformation("Publishing {EventId} status code {StatusCode}", @event.CorrelationId, result.StatusCode);
        _logger.LogTrace("Status code: {StatusCode}", result.StatusCode);
        _logger.LogTrace("Reason: {Reason}",          result.ReasonPhrase);
        _logger.LogTrace("Content: {Content}",        await result.Content.ReadAsStringAsync());
    }
}