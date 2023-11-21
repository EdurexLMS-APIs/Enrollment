using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class PaymentNotificationResponse
    {
        public string Payments { get; set; }
        public string Payment { get; set; }
        public string PaymentLogId { get; set; }
        public int Status { get; set; }
        public string StatusMessage { get; set; }
    }
}
