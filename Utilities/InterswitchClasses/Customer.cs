using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class Customer
    {
        public int Status { get; set; }

        public string StatusMessage { get; set; }

        public string CustReference { get; set; }

        public string CustomerReferenceAlternate { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OtherName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { set; get; }

        public string ThirdPartyCode { set; get; }

        public double Amount { get; set; }

        public object PayItems { get; internal set; }
    }
}
