using AdvertApi.Models;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public interface IAdvertStorageService
    {
        Task<string> Add(AdvertModel model);
        Task Confirm(ConfirmAdvertModel model);

        Task<AdvertDbModel> FindById(string id);
        // Health check for Dynamo DB
        Task<bool> CheckHealthAsync();
    }
}
