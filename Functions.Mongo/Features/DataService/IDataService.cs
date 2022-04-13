using System.Linq.Expressions;

namespace Functions.Mongo.Features.DataService;

public interface IDataService<in TCreateDto, in TUpdateDto, TGetDataDto, TDocument>
{
    Task<TGetDataDto>          InsertAsync(TCreateDto                                                         createDataDto);
    Task<TGetDataDto>          FindByIdAsync(string                                                           id);
    Task<List<TMapped>> FindAndGetArrayFieldAsync<TElement, TMapped>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, List<TElement>>> field);

    Task<TGetDataDto> FindSingleByFieldAsync<TField>(Expression<Func<TDocument, TField>> field, TField value);
    Task<IEnumerable<TField>> FindAndGetFieldsAsync<TField>(Expression<Func<TDocument, TField>> field);
    Task<TField> FindAndGetFieldAsync<TField>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> field);
    Task<TMapped> FindAndGetFieldAsync<TField, TMapped>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> field);
    Task<IEnumerable<TGetDataDto>> FindAllAsync(int? skip = null, int? take = null);
    Task<TGetDataDto> UpdateAsync(string id, TUpdateDto createDataDto);
    Task DeleteAsync(string id);
}