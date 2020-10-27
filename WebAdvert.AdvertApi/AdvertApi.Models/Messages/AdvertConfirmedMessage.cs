using System;
using System.Collections.Generic;
using System.Text;

namespace AdvertApi.Models.Messages
{
    //Message class for SNS when advertisement is confirmed
    public class AdvertConfirmedMessage
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}
