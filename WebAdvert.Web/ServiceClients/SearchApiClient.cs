using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebAdvert.Web.Models;

namespace WebAdvert.Web.ServiceClients
{
    public class SearchApiClient : ISearchApiClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        public SearchApiClient(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public async Task<List<AdvertSearchType>> Search(string keyword)
        {
            var result = new List<AdvertSearchType>();

            var BaseAddress = _configuration.GetSection("SearchApi").GetValue<string>("BaseUrl");
            var callUrl = $"{BaseAddress}/search/v1/{keyword}";

            var httpResponse = await _client.GetAsync(new Uri(callUrl)).ConfigureAwait(false);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var allAdverts = await httpResponse.Content.ReadAsAsync<List<AdvertSearchType>>()
                    .ConfigureAwait(false);
                result.AddRange(allAdverts);
            }

            return result;
        }
    }
}
