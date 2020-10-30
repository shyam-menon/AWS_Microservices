using AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;        

        public AdvertApiClient(IConfiguration configuration, HttpClient client, IMapper mapper)
        {
            _mapper = mapper;
            _configuration = configuration;
            _client = client;

            var baseUrl = _configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
            _client.BaseAddress = new Uri(baseUrl);
            //_client.DefaultRequestHeaders.Add(name: "Content-Type", value: "application/json");           
        }

        public async Task<bool> Confirm(ConfirmAdvertRequest model)
        {
            var advertModel = _mapper.Map<ConfirmAdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertModel);

            var content = new StringContent(jsonModel, System.Text.Encoding.UTF8, "application/json");

            var response = await _client
               .PutAsync(new Uri($"{_client.BaseAddress}/confirm"), content)
               .ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<CreateAdvertResponse> Create(CreateAdvertModel model)
        {
            var advertApiModel = _mapper.Map<AdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertApiModel);

            var apiUrl = new Uri($"{_client.BaseAddress}/create");

            var content = new StringContent(jsonModel, System.Text.Encoding.UTF8, "application/json");

            var response = await _client
                .PostAsync(apiUrl, content)
                .ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var createAdvertResponse = JsonConvert.DeserializeObject<AdvertResponse>(responseJson);

            var advertResponse = _mapper.Map<CreateAdvertResponse>(createAdvertResponse); 

            return advertResponse;
        }

        public async Task<List<AdvertModelClient>> GetAllAsync()
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_client.BaseAddress}/all")).ConfigureAwait(false);
            var fullAdvert = await apiCallResponse.Content.ReadAsAsync<List<AdvertModel>>().ConfigureAwait(false);
            return fullAdvert.Select(x => _mapper.Map<AdvertModelClient>(x)).ToList();
        }

        public async Task<AdvertModelClient> GetAsync(string advertId)
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_client.BaseAddress}/{advertId}")).ConfigureAwait(false);
            var selectedAdvert = await apiCallResponse.Content.ReadAsAsync<AdvertModel>().ConfigureAwait(false);
            return _mapper.Map<AdvertModelClient>(selectedAdvert);
        }
    }
}
