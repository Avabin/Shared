using Functions.Mongo.Features.DataService;
using Functions.Mongo.Features.DataSource;
using Functions.Mongo.Features.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Functions.Mongo.Features;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, Action<MongoSettings> configure)
    {
        services.AddOptions().Configure(configure);
        services.AddTransient(typeof(IMongoDataSource<>), typeof(MongoDataSource<>));
        services.AddTransient(typeof(IDataService<,,,>), typeof(DataService<,,,>));
        
        return services;
    }
}