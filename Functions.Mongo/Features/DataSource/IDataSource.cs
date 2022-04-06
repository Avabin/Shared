namespace Functions.Mongo.Features.DataSource;

public interface IDataSource<TDocument, TId> where TDocument : IDocument<TId>
{
    // async CRUD operations
    // create
    Task<TDocument> InsertAsync(TDocument document);

    // read
    Task<TDocument?> FindOneByIdAsync(TId id);

    Task<IEnumerable<TDocument>> FindAllAsync(int? skip, int? take);

    // update
    Task<TDocument> UpdateAsync(TDocument document);

    // delete
    Task DeleteAsync(TId id);
}