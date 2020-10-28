﻿using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebAdvert.SearchWorker
{
    public static class ElasticSearchHelper
    {
        private static IElasticClient _client;
        public static IElasticClient GetInstance(IConfiguration config)
        {
            if (_client == null)
            {
                var url = config.GetSection("ES").GetValue<string>("url");
                var settings = new ConnectionSettings(new Uri(url)).DefaultIndex("adverts");
                _client = new ElasticClient(settings);
            }
            return _client;
        }
    }
}
