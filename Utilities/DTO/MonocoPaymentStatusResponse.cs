using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class MonocoPaymentStatusResponse
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("data")]
        public MonocoPaymentStatusResponseDetails data { get; set; }

        //[JsonProperty("updated_at")]
        //public string CreatedDate { get; set; }
    }

    public class MonocoPaymentStatusResponseDetails
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        //[JsonProperty("account")]
        //public string Account { get; set; }

        //[JsonProperty("customer")]
        //public string Customer { get; set; }

        [JsonProperty("reference")]
        public string PaymentReference { get; set; }

        [JsonProperty("account")]
        public MonocoPaymentAccountResponseDetails Account { get; set; }

    }
    public class MonocoPaymentAccountResponseDetails
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

    }
}
