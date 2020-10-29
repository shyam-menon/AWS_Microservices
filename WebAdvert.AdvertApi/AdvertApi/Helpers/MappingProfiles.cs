using AdvertApi.Models;
using AdvertApi.Services;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertApi.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<AdvertModel, AdvertDbModel>().ReverseMap();
        }
    }
}
