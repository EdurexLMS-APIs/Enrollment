using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class MonnifyPaymentInitializationRequest
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerEmail")]
        public string CustomerEmail { get; set; }

        [JsonProperty("paymentReference")]
        public string PaymentReference { get; set; }

        [JsonProperty("paymentDescription")]
        public string PaymentDescription { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("contractCode")]
        public string ContractCode { get; set; }

        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty("paymentMethods")]
        public string[] PaymentMethods { get; set; }

        [JsonProperty("accountReference")]
        public string AccountReference { get; set; }

        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        [JsonProperty("incomeSplitConfig")]
        public List<IncomeSplitConfig> IncomeSplitConfig { get; set; }
    }

    public class IncomeSplitConfig
    {
        public string subAccountCode { get; set; }
        public decimal feePercentage { get; set; } = 0;
        public decimal splitAmount { get; set; }
        public bool feeBearer { get; set; } = false;
    }
}
