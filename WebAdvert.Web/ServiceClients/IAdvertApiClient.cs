﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;

namespace WebAdvert.Web.ServiceClients
{
    public interface IAdvertApiClient
    {
        //Using local class for request and response so as not to have to violate open close principle
        //when the API undergoes change
        Task<CreateAdvertResponse> Create(CreateAdvertModel model);

        Task<bool> Confirm(ConfirmAdvertRequest model);
    }
}