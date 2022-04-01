using AutoMapper;
using Functions.Mongo.Features.DataSource;
using Microsoft.Extensions.Logging;

namespace Functions.Mongo.Features.DataService;

public class DataService<TCreateDataDto, TUpdateDto, TDataDto, TDocument> : IDataService<TCreateDataDto,TUpdateDto, TDataDto, TDocument> where TDocument : IDocument<string>
{
    private readonly IMongoDataSource<TDocument>                                           _dataSource;
    private readonly IMapper                                                               _mapper;
    private readonly ILogger<DataService<TCreateDataDto, TUpdateDto, TDataDto, TDocument>> _logger;

    public DataService(IMongoDataSource<TDocument> dataSource, IMapper mapper, ILogger<DataService<TCreateDataDto, TUpdateDto, TDataDto, TDocument>> logger)
    {
        _dataSource = dataSource;
        _mapper     = mapper;
        _logger     = logger;
    }
    public async Task<TDataDto> Create(TCreateDataDto createDataDto)
    {
        _logger.LogInformation("Creating {DocumentType}", typeof(TDocument).Name);
        
        _logger.LogTrace("Mapping {DtoType} to {DocumentType}", typeof(TCreateDataDto).Name, typeof(TDocument).Name);
        var document = _mapper.Map<TDocument>(createDataDto);
        
        _logger.LogTrace("Document Id is {DocumentId}", document.Id);
        await _dataSource.CreateAsync(document);
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }

    public async Task<TDataDto> Read(string id)
    {
        _logger.LogInformation("Reading {DocumentType} with id {DocumentId}", typeof(TDocument).Name, id);
        var document = await _dataSource.ReadAsync(id);
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }

    public async Task<IEnumerable<TDataDto>> ReadAll(int? skip = null, int? take = null)
    {
        _logger.LogInformation("Reading all {DocumentType}", typeof(TDocument).Name);
        var documents = await _dataSource.ReadAllAsync(skip, take);
        
        _logger.LogTrace("Mapping a list of {DocumentType} to a list of {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<IEnumerable<TDataDto>>(documents);
    }

    public async Task<TDataDto> Update(string id, TUpdateDto createDataDto)
    {
        _logger.LogInformation("Updating {DocumentType} with id {DocumentId}", typeof(TDocument).Name, id);
        var document = await _dataSource.ReadAsync(id);
        
        _logger.LogTrace("Mapping {DtoType} to {DocumentType}", typeof(TUpdateDto).Name, typeof(TDocument).Name);
        _mapper.Map(createDataDto, document);
        
        await _dataSource.UpdateAsync(document ?? throw new ArgumentException("Cannot be parsed into document", nameof(createDataDto)));
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }

    public async Task Delete(string id)
    {
        _logger.LogInformation("Deleting {DocumentType} with id {DocumentId}", typeof(TDocument).Name, id);
        await _dataSource.DeleteAsync(id);
    }
}