using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class PaymentInitialiseResponse
    {
        public string paymentRef { get; set; }
        public string checkOutURL { get; set; }
        public string errorMessage { get; set; }

    }
}
