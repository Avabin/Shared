namespace Functions.Mongo.Features.DataSource;

public interface IDataSource<TDocument, TId> where TDocument : IDocument<TId>
{
    // async CRUD operations
    // create
    Task<TDocument> CreateAsync(TDocument document);

    // read
    Task<TDocument?> ReadAsync(TId id);

    Task<IEnumerable<TDocument>> ReadAllAsync(int? skip, int? take);

    // update
    Task<TDocument> UpdateAsync(TDocument document);

    // delete
    Task DeleteAsync(TId id);
}