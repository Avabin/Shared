using System.Linq.Expressions;
using Functions.Mongo.Features.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Functions.Mongo.Features.DataSource;

public class MongoDataSource<TDocument> : IMongoDataSource<TDocument> where TDocument : IDocument<string>
{
    private readonly ILogger<MongoDataSource<TDocument>> _logger;
    private readonly IOptions<MongoSettings>             _options;
    protected        MongoSettings                       Settings => _options.Value;
    protected        MongoClient                         Client   { get; }
    private readonly Lazy<IMongoDatabase>                _database;
    protected        IMongoDatabase                      Database => _database.Value;
    private readonly Lazy<IMongoCollection<TDocument>>   _collection;
    protected        IMongoCollection<TDocument>         Collection => _collection.Value;

    public MongoDataSource(IOptions<MongoSettings> options, ILogger<MongoDataSource<TDocument>> logger,
                           MongoClient             mongoClient)
    {
        Client   = mongoClient;
        _options = options;
        _logger  = logger;

        _database = new Lazy<IMongoDatabase>(() => Client.GetDatabase(Settings.DatabaseName));
        _collection =
            new Lazy<IMongoCollection<TDocument>>(() => Database.GetCollection<TDocument>(typeof(TDocument).Name));
    }

    public virtual async Task<TDocument> InsertAsync(TDocument document)
    {
        _logger.LogInformation("Creating document {DocumentId}", document.Id);
        await Collection.InsertOneAsync(document);
        return document;
    }
    
    public async Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> predicate)
    {
        _logger.LogInformation("Checking if document exists");
        return await Collection.CountDocumentsAsync(predicate) > 0;
    }

    public async Task<TDocument?> FindOneByIdAsync(string id)
    {
        _logger.LogInformation("Reading document {DocumentId}", id);
        return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TDocument>> FindAllAsync(int? skip, int? take)
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

    public async Task<TDocument?> FindSingleByFieldAsync<TField>(Expression<Func<TDocument, TField>> field,
                                                                 TField                              value)
    {
        _logger.LogInformation("Reading document by field {Field}", field.Name);
        var filter = Builders<TDocument>.Filter.Eq(field, value);
        return await Collection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<TDocument>> FindAllByFieldAsync<TField>(
        Expression<Func<TDocument, TField>> field, TField value, int? skip, int? limit)
    {
        _logger.LogInformation("Reading all documents by field {Field}", field.Name);
        var filter = Builders<TDocument>.Filter.Eq(field, value);
        return await Collection.Find(filter).Skip(skip).Limit(limit).ToListAsync();
    }

    public async Task<IEnumerable<TField>> FindAndGetFieldsAsync<TField>(
        Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> field, int? skip = null,
        int?                              take = null)
    {
        _logger.LogInformation("Reading all documents by field {Field}", field.Name);
        var filter = Builders<TDocument>.Filter.Where(predicate);
        return await Collection.Find(filter).Project(field).Skip(skip).Limit(take).ToListAsync();
    }

    public async Task<IEnumerable<TField>> FindAndGetFieldsAsync<TField>(
        Expression<Func<TDocument, TField>> field, int? skip = null, int? limit = null)
    {
        _logger.LogInformation("Reading all documents by field {Field}", field.Name);
        var result = await Collection
                          .Find(FilterDefinition<TDocument>.Empty)
                          .Project(field)
                          .Skip(skip)
                          .Limit(limit)
                          .ToListAsync();

        return result;
    }

    public async Task<TField> FindAndGetFieldAsync<TField>(string id, Expression<Func<TDocument, TField>> field)
    {
        _logger.LogInformation("Reading field {Field} of document {DocumentId}", field.Name, id);
        var filter = Builders<TDocument>.Filter.Eq(x => x.Id, id);
        return await Collection.Find(filter).Project(field).SingleOrDefaultAsync();
    }

    public async Task<TField> FindAndGetFieldAsync<TField>(Expression<Func<TDocument, bool>>   predicate,
                                                           Expression<Func<TDocument, TField>> field)
    {
        _logger.LogInformation("Reading field {Field} of document {DocumentId}", field.Name, predicate.ToString());
        var filter     = Builders<TDocument>.Filter.Where(predicate);
        var projection = Builders<TDocument>.Projection.Expression(field);
        var document   = await Collection.Find(filter).Project(projection).SingleOrDefaultAsync();

        return document;
    }
    
    public async Task<List<TElement>> FindAndGetArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>>   filter, Expression<Func<TDocument, List<TElement>>> field)
    {
        _logger.LogTrace("{Action} of {DocumentType} on field {Field}", nameof(FindAndGetArrayFieldAsync), typeof(TDocument).Name, field);
        
        var result = await Collection.Find(filter).Project(field).SingleOrDefaultAsync();

        return result;
    }


    public async Task AddElementToArrayFieldAsync<TElement>(
        string id, Expression<Func<TDocument, IEnumerable<TElement>>> field, TElement newElement)
    {
        _logger.LogTrace("{Action}: Adding element {Element} to field {Field} of type {DocumentType}",
                         nameof(AddElementToArrayFieldAsync), newElement, field, typeof(TDocument).Name);

        var filter = Builders<TDocument>.Filter.Eq(s => s.Id, id);
        var update = Builders<TDocument>.Update.AddToSet(field, newElement);
        await Collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task AddElementToArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>> filter,
                                                            Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                                            TElement newElement)
    {
        _logger.LogTrace("{Action}: Adding element {Element} to field {Field} of type {DocumentType}",
                         nameof(AddElementToArrayFieldAsync), newElement, field, typeof(TDocument).Name);

        var update = Builders<TDocument>.Update.AddToSet(field, newElement);
        await Collection.FindOneAndUpdateAsync(filter, update);
    }

    public async Task RemoveElementFromArrayFieldAsync<TElement>(
        string id, Expression<Func<TDocument, IEnumerable<TElement>>> field, TElement like) =>
        await RemoveElementFromArrayFieldAsync(x => x.Id == id, field, like);

    public async Task RemoveElementFromArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>> filter,
                                                                 Expression<Func<TDocument, IEnumerable<TElement>>>
                                                                     field, TElement like) =>
        await RemoveElementFromArrayFieldAsync((FilterDefinition<TDocument>)filter, field, like);

    private async Task RemoveElementFromArrayFieldAsync<TElement>(FilterDefinition<TDocument> filter,
                                                                  Expression<Func<TDocument, IEnumerable<TElement>>>
                                                                      field, TElement like)
    {
        _logger.LogDebug("{Action}: Removing element {Element} from field {Field} of type {DocumentType}",
                         nameof(RemoveElementFromArrayFieldAsync), like, field, typeof(TDocument).Name);
        var update = Builders<TDocument>.Update.Pull(field, like);
        await Collection.FindOneAndUpdateAsync(filter, update);
    }
    
    public async Task AddElementsToArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, IEnumerable<TElement>>> field, IEnumerable<TElement> newElements)
    {
        _logger.LogDebug("{Action}: Adding elements {Elements} to field {Field} of type {DocumentType}",
                         nameof(AddElementsToArrayFieldAsync), newElements, field, typeof(TDocument).Name);
        var update = Builders<TDocument>.Update.AddToSetEach(field, newElements);
        await Collection.FindOneAndUpdateAsync(filter, update);
    }
    
    public async Task RemoveElementsFromArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>> filter, Expression<Func<TDocument, IEnumerable<TElement>>> field, IEnumerable<TElement> like)
    {
        _logger.LogDebug("{Action}: Removing elements {Elements} from field {Field} of type {DocumentType}",
                         nameof(RemoveElementsFromArrayFieldAsync), like, field, typeof(TDocument).Name);
        var update = Builders<TDocument>.Update.PullAll(field, like);
        await Collection.FindOneAndUpdateAsync(filter, update);
    }
}