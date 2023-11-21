using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.InterswitchClasses
{
    public class PaymentItems
    {
        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public int Quality { get; set; }

        public double Price { get; set; }

        public double Subtotal { get; set; }

        public int Tax { get; set; }

        public double Total { get; set; }
    }
}
