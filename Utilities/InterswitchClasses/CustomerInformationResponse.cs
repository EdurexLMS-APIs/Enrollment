using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class CustomerInformationResponse
    {
        public string MerchantReference { get; set; }
        public string Customers { get; set; }
        public string Customer { get; set; }
        public string CustReference { get; set; }
        public string CustomerReferenceAlternate { get; set; }
        public int Status { get; set; }
        public string StatusMessage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ThirdPartyCode { get; set; }
        public double Amount { get; set; }
    }
    public class Customer1
    {
        public Customer2 customerDetail { get; set; }
    }
    public class Customer2
    {
        public int Status { get; set; }
        public string CustReference { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public float Amount { get; set; }
    }
}
