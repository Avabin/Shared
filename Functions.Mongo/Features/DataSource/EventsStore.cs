using MongoDB.Driver;

namespace Functions.Mongo.Features.DataSource;

public class EventsStore
{
    private readonly MongoClient _client;
    public const     string      EventStoreCollection = "Events";
    public EventsStore(MongoClient client)
    {
        _client = client;
    }
}