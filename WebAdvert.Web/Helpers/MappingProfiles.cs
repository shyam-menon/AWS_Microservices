using AdvertApi.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Models.Home;
using WebAdvert.Web.ServiceClients;

namespace WebAdvert.Web.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            //Mapping from the advert microservice to local models
            CreateMap<CreateAdvertModel, AdvertModel>().ReverseMap();
            CreateMap<CreateAdvertResponse, AdvertResponse>().ReverseMap();
            CreateMap<ConfirmAdvertRequest, ConfirmAdvertModel>().ReverseMap();

            //Mapping for website models
            CreateMap<CreateAdvertViewModel, CreateAdvertModel>().ReverseMap();

            CreateMap<AdvertSearchType, SearchViewModel>()
                .ForMember(
                    dest => dest.Id, src => src.MapFrom(field => field.Id))
                .ForMember(
                    dest => dest.Title, src => src.MapFrom(field => field.Title));

            CreateMap<AdvertModel, AdvertModelClient>().ReverseMap();

            CreateMap<AdvertModelClient, IndexViewModel>()
                .ForMember(
                    dest => dest.Title, src => src.MapFrom(field => field.Title))
                .ForMember(dest => dest.Image, src => src.MapFrom(field => field.FilePath));

        }
    }
}
