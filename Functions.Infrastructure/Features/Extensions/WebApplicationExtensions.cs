using System.Text;
using System.Text.Json;
using Functions.Infrastructure.Features.Events;
using Microsoft.AspNetCore.Mvc;
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

    public static WebApplication MapEvents(this WebApplication webApplication, string pattern = "/")
    {
        webApplication.MapPost(pattern, ([FromBody] JsonElement json, [FromServices] IEventBus eventBus) =>
        {
            var jsonString = json.GetRawText();
#pragma warning disable CS4014
            Task.Run(() =>
#pragma warning restore CS4014
                     {
                         using var reader     = new JsonTextReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(jsonString))));

                         var @event = _serializer.Deserialize<IEvent>(reader);

                         eventBus.Publish(@event);
                     });

            return Results.Ok();
        });

        return webApplication;
    }
}