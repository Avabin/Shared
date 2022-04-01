namespace Functions.Mongo.Features.DataService;

public interface IDataService<in TCreateDto, in TUpdateDto, TGetDataDto, TDocument>
{
    Task<TGetDataDto>              Create(TCreateDto createDataDto);
    Task<TGetDataDto>              Read(string    id);
    Task<IEnumerable<TGetDataDto>> ReadAll(int? skip = null, int? take = null);
    Task<TGetDataDto>              Update(string id, TUpdateDto createDataDto);
    Task                    Delete(string id);
}