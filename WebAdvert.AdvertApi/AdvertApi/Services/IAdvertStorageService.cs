using AdvertApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public interface IAdvertStorageService
    {
        Task<string> Add(AdvertModel model);
        Task ConfirmAsync(ConfirmAdvertModel model);

        Task<AdvertDbModel> FindByIdAsync(string id);
        // Health check for Dynamo DB
        Task<bool> CheckHealthAsync();

        Task<List<AdvertModel>> GetAllAsync();
    }
}
