using System.Linq.Expressions;
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
    public async Task<TDataDto> InsertAsync(TCreateDataDto createDataDto)
    {
        _logger.LogInformation("Creating {DocumentType}", typeof(TDocument).Name);
        
        _logger.LogTrace("Mapping {DtoType} to {DocumentType}", typeof(TCreateDataDto).Name, typeof(TDocument).Name);
        var document = _mapper.Map<TDocument>(createDataDto);
        
        _logger.LogTrace("Document Id is {DocumentId}", document.Id);
        await _dataSource.InsertAsync(document);
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }
    
    public async Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> predicate) => 
        await _dataSource.ExistsAsync(predicate);


    public async Task<TDataDto> FindByIdAsync(string id)
    {
        _logger.LogInformation("Reading {DocumentType} with id {DocumentId}", typeof(TDocument).Name, id);
        var document = await _dataSource.FindOneByIdAsync(id);
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }

    public async Task<List<TMapped>> FindAndGetArrayFieldAsync<TElement, TMapped>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, List<TElement>>> field)
    {
        var fieldValues = await _dataSource.FindAndGetArrayFieldAsync(predicate, field);
        return _mapper.Map<List<TMapped>>(fieldValues);
    }

    public async Task<TDataDto>              FindSingleByFieldAsync<TField>(Expression<Func<TDocument, TField>> field, TField value)
    {
        _logger.LogInformation("Reading {DocumentType} with {FieldName} {FieldValue}", typeof(TDocument).Name, field.Name, value);
        var document = await _dataSource.FindSingleByFieldAsync(field, value);
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }

    public async Task<IEnumerable<TField>> FindAndGetFieldsAsync<TField>(Expression<Func<TDocument, TField>> field)
    {
        _logger.LogInformation("Reading {DocumentType} with {FieldName}", typeof(TDocument).Name, field.Name);
        var fields = await _dataSource.FindAndGetFieldsAsync(field);

        return fields;
    }

    public async Task<TField>  FindAndGetFieldAsync<TField>(Expression<Func<TDocument, bool>>          predicate, Expression<Func<TDocument, TField>> field)
    {
        _logger.LogInformation("Reading {DocumentType} with {FieldName}", typeof(TDocument).Name, field.Name);
        var fieldValue = await _dataSource.FindAndGetFieldAsync(predicate, field);

        return fieldValue;
    }

    public async Task<TMapped> FindAndGetFieldAsync<TField, TMapped>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>>                        field)
    {
        _logger.LogInformation("Reading {DocumentType} with {FieldName}", typeof(TDocument).Name, field.Name);
        var document = await _dataSource.FindAndGetFieldAsync(predicate, field);
        

        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TMapped).Name);
        return _mapper.Map<TMapped>(document);
    }

    public async Task<IEnumerable<TDataDto>> FindAllByFieldAsync<TField>(Expression<Func<TDocument, TField>>    field, TField value, int? skip = null, int? limit = null)
    {
        _logger.LogInformation("Reading {DocumentType} with {FieldName} {FieldValue}", typeof(TDocument).Name, field.Name, value);
        var documents = await _dataSource.FindAllByFieldAsync(field, value, skip, limit);
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<IEnumerable<TDataDto>>(documents);
    }

    public async Task<IEnumerable<TDataDto>> FindAllAsync(int? skip = null, int? take = null)
    {
        _logger.LogInformation("Reading all {DocumentType}", typeof(TDocument).Name);
        var documents = await _dataSource.FindAllAsync(skip, take);
        
        _logger.LogTrace("Mapping a list of {DocumentType} to a list of {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<IEnumerable<TDataDto>>(documents);
    }

    public async Task<TDataDto> UpdateAsync(string id, TUpdateDto createDataDto)
    {
        _logger.LogInformation("Updating {DocumentType} with id {DocumentId}", typeof(TDocument).Name, id);
        var document = await _dataSource.FindOneByIdAsync(id);
        
        _logger.LogTrace("Mapping {DtoType} to {DocumentType}", typeof(TUpdateDto).Name, typeof(TDocument).Name);
        _mapper.Map(createDataDto, document);
        
        await _dataSource.UpdateAsync(document ?? throw new ArgumentException("Cannot be parsed into document", nameof(createDataDto)));
        
        _logger.LogTrace("Mapping {DocumentType} to {DtoType}", typeof(TDocument).Name, typeof(TDataDto).Name);
        return _mapper.Map<TDataDto>(document);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting {DocumentType} with id {DocumentId}", typeof(TDocument).Name, id);
        await _dataSource.DeleteAsync(id);
    }
}