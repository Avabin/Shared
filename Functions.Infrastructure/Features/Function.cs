using Functions.Infrastructure.Features.Extensions;
using Serilog;

namespace Functions.Infrastructure.Features;

public static class Function
{
    public static WebApplicationBuilder CreateBuilder(string[] args, string envPrefix, string serviceName)
    {
        var seqUrl = Environment.GetEnvironmentVariable($"SEQ_URL") ?? "http://localhost:5341";
        Console.WriteLine("SEQ_URL: " + seqUrl);
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, provider, s) => s
                                                         .MinimumLevel.Verbose()
                                                         .WriteTo.Console()
                                                         .Enrich.FromLogContext()
                                                         .Enrich.WithProperty("Service", serviceName)
                                                         .MinimumLevel.Verbose()
                                                         .WriteTo.Seq(seqUrl)
                                                         .Enrich.FromLogContext()
                                                         .Enrich.WithProperty("Service", serviceName));
        builder.Configuration.AddEnvironmentVariables(envPrefix);
        builder.Services.AddEventing(builder.Configuration.GetRequiredSection("Eventing"));
        
        return builder;
    }
}