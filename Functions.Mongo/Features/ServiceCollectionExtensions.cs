using Functions.Mongo.Features.DataService;
using Functions.Mongo.Features.DataSource;
using Functions.Mongo.Features.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Functions.Mongo.Features;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, Action<MongoSettings> configure)
    {
        services.AddOptions().Configure(configure);
        services.AddSingleton(sp => new MongoClient(sp.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString));
        services.AddTransient(typeof(IMongoDataSource<>), typeof(MongoDataSource<>));
        services.AddTransient(typeof(IDataService<,,,>), typeof(DataService<,,,>));
        
        return services;
    }
}