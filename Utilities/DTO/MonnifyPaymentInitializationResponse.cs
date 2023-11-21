using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class MonnifyPaymentInitializationResponse
    {
        public bool premium { get; set; }

        [JsonProperty("requestSuccessful")]
        public bool RequestSuccessful { get; set; }

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseBody")]
        public PaymentInitializationResponseDetails ResponseDetails { get; set; }
    }


    public class PaymentInitializationResponseDetails
    {
        [JsonProperty("transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty("paymentReference")]
        public string PaymentReference { get; set; }

        [JsonProperty("merchantName")]
        public string MerchantName { get; set; }

        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty("enabledPaymentMethod")]
        public string[] EnabledPaymentMethod { get; set; }

        [JsonProperty("checkoutUrl")]
        public string CheckoutUrl { get; set; }

    }
}
