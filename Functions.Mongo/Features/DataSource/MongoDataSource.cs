using Functions.Mongo.Features.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Functions.Mongo.Features.DataSource;

public class MongoDataSource<TDocument> : IMongoDataSource<TDocument> where TDocument : IDocument<string>
{
    private readonly IOptions<MongoSettings>             _options;
    private readonly ILogger<MongoDataSource<TDocument>> _logger;
    protected        MongoSettings                       Settings => _options.Value;

    private readonly Lazy<MongoClient>                 _client;
    public           MongoClient                       Client => _client.Value;
    private readonly Lazy<IMongoDatabase>              _database;
    public           IMongoDatabase                    Database => _database.Value;
    private readonly Lazy<IMongoCollection<TDocument>> _collection;
    public           IMongoCollection<TDocument>       Collection => _collection.Value;

    public MongoDataSource(IOptions<MongoSettings> options, ILogger<MongoDataSource<TDocument>> logger)
    {
        _options = options;
        _logger  = logger;

        _client = new Lazy<MongoClient>(() => new MongoClient(Settings.ConnectionString));
        _database = new Lazy<IMongoDatabase>(() => Client.GetDatabase(Settings.DatabaseName));
        _collection = new Lazy<IMongoCollection<TDocument>>(() => Database.GetCollection<TDocument>(typeof(TDocument).Name));
    }

    public async Task<TDocument> CreateAsync(TDocument document)
    {
        _logger.LogInformation("Creating document {DocumentId}", document.Id);
        await Collection.InsertOneAsync(document);
        return document;
    }

    public async Task<TDocument?> ReadAsync(string id)
    {
        _logger.LogInformation("Reading document {DocumentId}", id);
        return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TDocument>> ReadAllAsync(int? skip, int? take)
    {
        _logger.LogInformation("Reading all documents");
        return await Collection.Find(x => true).Skip(skip).Limit(take).ToListAsync();
    }

    public async Task<TDocument> UpdateAsync(TDocument document)
    {
        _logger.LogInformation("Updating document {DocumentId}", document.Id);
        await Collection.ReplaceOneAsync(x => x.Id == document.Id, document);
        return document;
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting document {DocumentId}", id);
        await Collection.DeleteOneAsync(x => x.Id == id);
    }
}