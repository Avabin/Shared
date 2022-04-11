using Functions.Infrastructure.Features.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

namespace Functions.Infrastructure.Features;

public static class Function
{
    public static WebApplicationBuilder CreateBuilder(string[] args, string envPrefix, string serviceName, bool configureEvents = true)
    {
        var seqUrl       = Environment.GetEnvironmentVariable($"SEQ_URL")      ?? "http://localhost:5341";
        var hostname     = Environment.GetEnvironmentVariable("HOSTNAME")      ?? "localhost";
        Console.WriteLine("SEQ_URL: " + seqUrl);
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, provider, s) => s
                                                         .MinimumLevel.Verbose()
                                                         .WriteTo.Console()
                                                         .Enrich.FromLogContext()
                                                         .Enrich.WithProperty("Service", serviceName)
                                                         .Enrich.WithProperty("Hostname", hostname)
                                                         .MinimumLevel.Verbose()
                                                         .WriteTo.Seq(seqUrl)
                                                         .Enrich.FromLogContext()
                                                         .Enrich.WithProperty("Hostname", hostname)
                                                         .Enrich.WithProperty("Service", serviceName));
        builder.Configuration.AddEnvironmentVariables(envPrefix);
        if(configureEvents) builder.Services.AddEventing(builder.Configuration.GetRequiredSection("Eventing"));

        return builder;
    }
    
    public static WebApplicationBuilder AddDefaultAuthentication(this WebApplicationBuilder builder)
    {
        var jwtAuthority = Environment.GetEnvironmentVariable("JWT_AUTHORITY") ?? "http://localhost:8080";
        var jwtAudience  = Environment.GetEnvironmentVariable("JWT_AUDIENCE")  ?? "http://localhost:8080";
        
        builder.Services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            o.Authority            = jwtAuthority;
            o.Audience             = jwtAudience;
        });
        
        return builder;
    }
}