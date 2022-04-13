using System.Linq.Expressions;

namespace Functions.Mongo.Features.DataSource;

public interface IMongoDataSource<TDocument> : IDataSource<TDocument, string> where TDocument : IDocument<string>
{
    Task<TDocument?> FindSingleByFieldAsync<TField>(Expression<Func<TDocument, TField>> field, TField value);

    Task<IEnumerable<TDocument>> FindAllByFieldAsync<TField>(Expression<Func<TDocument, TField>> field, TField value,
                                                             int?                                skip,  int?   limit);

    Task<IEnumerable<TField>> FindAndGetFieldsAsync<TField>(Expression<Func<TDocument, TField>> field, int? skip = null,
                                                            int?                                limit = null);

    Task<TField> FindAndGetFieldAsync<TField>(string id, Expression<Func<TDocument, TField>> field);

    Task<TField> FindAndGetFieldAsync<TField>(Expression<Func<TDocument, bool>>   predicate,
                                              Expression<Func<TDocument, TField>> field);

    Task AddElementToArrayFieldAsync<TElement>(string   id, Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                               TElement newElement);

    Task AddElementToArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>>                  filter,
                                               Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                               TElement                                           newElement);

    Task RemoveElementFromArrayFieldAsync<TElement>(string id, Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                                    TElement like);

    Task RemoveElementFromArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>>                  filter,
                                                    Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                                    TElement                                           like);

    Task AddElementsToArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>>                  filter,
                                                Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                                IEnumerable<TElement>                              newElements);

    Task RemoveElementsFromArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>>                  filter,
                                                     Expression<Func<TDocument, IEnumerable<TElement>>> field,
                                                     IEnumerable<TElement>                              like);

    Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> predicate);
    Task<List<TElement>> FindAndGetArrayFieldAsync<TElement>(Expression<Func<TDocument, bool>>   filter, Expression<Func<TDocument, List<TElement>>> field);
}