using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class programFees
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float PriceNGN { get; set; }
        public float DepositNGN { get; set; }
        public float PriceUSD { get; set; }
        public float DepositUSD { get; set; }
    }
}
