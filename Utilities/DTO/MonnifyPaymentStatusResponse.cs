using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class MonnifyPaymentStatusResponse
    {
        [JsonProperty("requestSuccessful")]
        public bool RequestSuccessful { get; set; }

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseBody")]
        public PaymentStatusResponseDetails ResponseDetails { get; set; }
    }

    public class PaymentStatusResponseDetails
    {
        [JsonProperty("transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty("paymentReference")]
        public string PaymentReference { get; set; }

        [JsonProperty("amountPaid")]
        public string AmountPaid { get; set; }

        [JsonProperty("settlementAmount")]
        public string SettlementAmount { get; set; }

        [JsonProperty("paidOn")]
        public string PaymentDate { get; set; }

        [JsonProperty("paymentStatus")]
        public string PaymentStatus { get; set; }

        [JsonProperty("paymentDescription")]
        public string PaymentDescription { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("paymentMethod")]
        public string PaymentMethod { get; set; }

    }
}
