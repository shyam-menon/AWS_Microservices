﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.AdvertManagement
{
    public class AdvertModelClient
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string FilePath { get; set; }
        public string Id { get; set; }
    }
}
