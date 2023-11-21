using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class MonoPaymentInitializationResponse
    {
        public string Id { get; set; }
        public string type { get; set; }
        public string amount { get; set; }
        public string description { get; set; }
        public string reference { get; set; }
        public string payment_link { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public bool RequestSuccessful { get; set; }
        public string CheckoutUrl { get; set; }
        //[JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }
    }

    public class MonoPaymentInitializationResponse2
    {
        public string Id { get; set; }
        public string type { get; set; }
        public string amount { get; set; }
        public string description { get; set; }
        public string reference { get; set; }
        public string CheckoutUrl { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public bool premium { get; set; }
    }
}

