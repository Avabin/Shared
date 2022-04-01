namespace Functions.Mongo.Features.DataSource;

public interface IMongoDataSource<TDocument> : IDataSource<TDocument, string> where TDocument : IDocument<string>
{
    
}