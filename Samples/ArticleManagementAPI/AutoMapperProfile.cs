using ArticleManagementAPI.Entities;
using ArticleManagementAPI.Models;
using AutoMapper;
using Azure.Data.Tables;

namespace ArticleManagementAPI
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TopicCreationRequestModel, TopicEntity>()
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.TopicName))
                .ForMember(dest => dest.TopicCode, opt => opt.MapFrom(src => src.TopicCode))
                .ForMember(dest => dest.TopicDescription, opt => opt.MapFrom(src => src.TopicDescription))
                .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => src.TopicName))
                .ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.TopicCode));

            CreateMap<TopicEntity, TopicResponseModel>()
                .ForMember(dest => dest.TopicCode, opt => opt.MapFrom(src => src.TopicCode))
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.TopicName))
                .ForMember(dest => dest.TopicDescription, opt => opt.MapFrom(src => src.TopicDescription))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }
    }
}
