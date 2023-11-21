using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class InterswitchRequest
    {
        public CustomerInformationRequest CustomerValidationRe { get; set; }
        public PaymentNotificationRequest PaymentValidationRe { get; set; }
    }
}
