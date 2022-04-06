namespace Functions.Mongo.Features.DataService;

public interface IDataService<in TCreateDto, in TUpdateDto, TGetDataDto, TDocument>
{
    Task<TGetDataDto> InsertAsync(TCreateDto createDataDto);
    Task<TGetDataDto> FindByIdAsync(string   id);
    Task<IEnumerable<TGetDataDto>> FindAllAsync(int? skip = null, int? take = null);
    Task<TGetDataDto>              UpdateAsync(string id, TUpdateDto createDataDto);
    Task                    DeleteAsync(string id);
}