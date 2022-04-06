using System.Text;
using System.Text.Json;
using Functions.Infrastructure.Features.Events;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Functions.Infrastructure.Features.Extensions;

public static class WebApplicationExtensions
{
    private static string _apiKey = Environment.GetEnvironmentVariable("TDG_Eventing__ApiKey") ?? "apikey";
    private static JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
    {
        TypeNameHandling               = TypeNameHandling.Objects,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
    });

    public static WebApplication MapEvents(this WebApplication webApplication, string pattern = "/")
    {
        webApplication.MapPost(pattern, async ([FromBody] JsonElement json, [FromServices] IEventBus eventBus, [FromServices] ILogger<WebApplication> logger) =>
        {
            var apiKey = json.GetProperty("ApiKey").GetString();
            logger.LogTrace("Is ApiKey valid: {ApiKey}", _apiKey.Equals(apiKey, StringComparison.OrdinalIgnoreCase));

            if (apiKey is null || !apiKey.Equals(_apiKey))
                return Results.BadRequest(new { error = "Invalid API key" });
            
            var jsonString = json.GetRawText();
#pragma warning disable CS4014
            await Task.Run(() =>
#pragma warning restore CS4014
                     {
                         using var reader     = new JsonTextReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(jsonString))));

                         var @event = _serializer.Deserialize<IEvent>(reader);

                         if (@event is not null)
                             eventBus.Publish(@event);
                         else
                             logger.LogError("Could not deserialize event: {Event}", jsonString);
                     });

            return Results.Ok();
        });

        return webApplication;
    }
}