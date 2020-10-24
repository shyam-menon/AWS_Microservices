﻿using AdvertApi.Models;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public interface IAdvertStorageService
    {
        Task<string> Add(AdvertModel model);
        Task Confirm(ConfirmAdvertModel model);
        // Health check for Dynamo DB
        Task<bool> CheckHealthAsync();
    }
}
