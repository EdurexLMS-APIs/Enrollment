using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
     public class CustomerInformationRequest
    {
        public string ServiceUrl { get; set; } //M

        public string ServiceUsername { get; set; }

        public string ServicePassword { get; set; }

        public string FTPUrl { get; set; }

        public string FTPUsername { get; set; }

        public string FTPassword { get; set; }

        public string MerchantReference { get; set; } //M

        public string CustReference { get; set; }    //M

        public string PaymentItemCategoryCode { get; set; }

        public string PaymentItemCode { get; set; }

        public string RequestReference { get; set; }

        public string TerminalId { get; set; }

        // public int Amount { set; get; }

        public string ThirdPartyCode { set; get; }
    }
}