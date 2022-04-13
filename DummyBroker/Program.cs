using System.Text;
using System.Text.Json;
using DummyBroker;
using Functions.Infrastructure.Features;
using Functions.Infrastructure.Features.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = Function.CreateBuilder(args, "BROKER_", "MessageBroker", false);
builder.Services
       .AddHttpClient();
var app = builder.Build();
app.MapPost("/{ns}/{brokerName}", async (string ns, string brokerName,
                                         [FromHeader(Name = "Ce-Subject")] string subject,
                                         [FromBody]                        JsonElement json,
                                         [FromServices]                    HttpClient client,
                                         [FromServices]                    ILogger<Program> logger,
                                         [FromServices]                    IConfiguration config) =>
{
    var services = config.GetChildren().Single(x => x.Key == "ServiceMapping").Get<List<Service>>();

    if (services is null) return Results.StatusCode(404);

    var service = services.Single(x => x.SubjectName == subject);

    var request = new HttpRequestMessage(HttpMethod.Post, service.EventingUrl);
    request.Headers.Add("Ce-Subject", subject);

    var content = new StringContent(json.GetRawText(), Encoding.UTF8, "application/json");
    request.Content = content;

    var response = await client.SendAsync(request);

    logger.LogInformation("Event sent to ({Subject}) {Url}", subject, service.EventingUrl);
    logger.LogDebug("Status code: {StatusCode}", response.StatusCode);
    logger.LogDebug("Content: {Content}",        await response.Content.ReadAsStringAsync());

    return Results.StatusCode(202);
});
app.Run();