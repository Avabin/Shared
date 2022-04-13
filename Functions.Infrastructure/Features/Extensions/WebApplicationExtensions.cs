using System.Text;
using System.Text.Json;
using Functions.Infrastructure.Features.EventHandlers;
using Functions.Infrastructure.Features.Events;
using Functions.Infrastructure.Features.Options;
using Functions.Infrastructure.Features.Queries;
using Functions.Infrastructure.Features.QueryHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Functions.Infrastructure.Features.Extensions;

public static class WebApplicationExtensions
{
    private static JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
    {
        TypeNameHandling               = TypeNameHandling.Objects,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
    });

    public static WebApplication ConfigureRestApi(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapEvents("/events");

        return app;
    }

    public static WebApplication MapEvents(this WebApplication webApplication, string pattern = "/")
    {
        webApplication.MapPost(pattern, async (HttpResponse                              response, HttpRequest request,
                                               [FromServices] IEventHandlerFactory       handlerFactory,
                                               [FromServices] IEventBus                  eventBus,
                                               [FromServices] ILogger<WebApplication>    logger,
                                               [FromServices] IOptions<EventingSettings> options) =>
        {
            using var sr         = new StreamReader(request.Body);
            var       bodyString = await sr.ReadToEndAsync();
            using var reader =
                new JsonTextReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(bodyString))));

            var @event = _serializer.Deserialize<IEvent>(reader);

            if (@event is null)
            {
                logger.LogError("Could not deserialize event {Body}", bodyString);
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { error = "Could not deserialize event" });
                return;
            }

            var eventType = $"xyz.avabin.{@event.GetType().Name}";
            logger.LogTrace("Received event {EventType}", eventType);

            var handler     = handlerFactory.Create(@event.GetType());
            var handlerType = handler?.GetType().Name;
            logger.LogTrace("Found handler type {HandlerType}", handlerType);

            switch (handler, @event)
            {
                case (null, { } e):
                {
                    logger.LogDebug("No handler found for event {EventType}. Publishing to internal event bus",
                                    eventType);
                    eventBus.Publish(e);
                    response.StatusCode = 202;
                    await response.WriteAsJsonAsync(new { message = "Event published to internal event bus" });
                    break;
                }
                case (IQueryHandler queryHandler, IQuery query):
                {
                    logger.LogDebug("Found query handler {HandlerType} for query {QueryType}", handlerType,
                                    query.GetType().Name);
                    var resultSubject = request.Headers["Ce-Source"];
                    var result        = await queryHandler.HandleAsync(query);
                    result.ApiKey        = options.Value.ApiKey;
                    result.CorrelationId = query.CorrelationId;

                    logger.LogInformation("Responding with result {ResultType} handled by {HandlerType} to {ResultSubject}",
                                          result.GetType().Name, handlerType, resultSubject);
                    await response.WriteEventAsync(result, options.Value.Source, resultSubject);
                    break;
                }
                case (IRespondingEventHandler respondingHandler, { } e):
                {
                    logger.LogDebug("Found responding handler {HandlerType} for event {EventType}", handlerType,
                                    eventType);
                    var eventResult = await respondingHandler.HandleAsync(e);
                    eventResult.ApiKey = options.Value.ApiKey;
                    eventResult.CorrelationId = e.CorrelationId;

                    logger.LogInformation("Responding with event {EventType} handled by {HandlerType}",
                                          eventResult.GetType().Name, handlerType);
                    await response.WriteEventAsync(eventResult, options.Value.Source);
                    break;
                }
                case ({ } eh, { } e):
                    logger.LogDebug("Found event handler {HandlerType} for event {EventType}", handlerType, eventType);
                    await eh.HandleAsync(e);
                    response.StatusCode = 202;
                    await response.WriteAsJsonAsync(new { message = "Event handled" });
                    break;
            }
        });

        return webApplication;
    }

    private static async Task WriteEventAsync(this HttpResponse response, IEvent e, string source,
                                              string?           target = null)
    {
        response.StatusCode          = 202;
        response.Headers.ContentType = "application/json";
        response.Headers.Add("Ce-Id",          e.CorrelationId.ToString());
        response.Headers.Add("Ce-Specversion", "1.0");
        response.Headers.Add("Ce-Source",      source);
        if (target is null or "") response.Headers.Add("Ce-Subject", target);
        response.Headers.Add("Ce-Type",        $"xyz.avabin.{e.GetType().Name}");
        response.Headers.Add("Ce-Contenttype", "application/json");

        var       sb = new StringBuilder();
        using var jw = new JsonTextWriter(new StringWriter(sb));
        _serializer.Serialize(jw, e);

        await response.WriteAsync(sb.ToString());
    }
}