using AdvertApi.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models;
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
        }
    }
}
