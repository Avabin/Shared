using AutoMapper;

namespace Functions.Mongo.Features;

public class DataMapperProfile<TCreate, TGet, TUpdate, TDoc> : Profile
{
    public DataMapperProfile()
    {
        CreateMap<TDoc, TCreate>().ReverseMap();
        CreateMap<TDoc, TGet>().ReverseMap();
        CreateMap<TDoc, TUpdate>().ReverseMap();
        CreateMap<TCreate, TGet>().ReverseMap();
        CreateMap<TUpdate, TGet>().ReverseMap();
        CreateMap<TCreate, TUpdate>().ReverseMap();
    }
}

public class DataMapperProfile<TCreate, TGet, TDoc> : Profile
{
    public DataMapperProfile()
    {
        CreateMap<TDoc, TCreate>().ReverseMap();
        CreateMap<TDoc, TGet>().ReverseMap();
        CreateMap<TCreate, TGet>().ReverseMap();
    }
}