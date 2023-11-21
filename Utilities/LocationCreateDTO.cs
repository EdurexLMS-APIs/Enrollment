using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities
{
    public class LocationCreateDTO
    {
        [Required]
        public int cityId { get; set; }

        [Required]
        public string streetNumber { get; set; }

        public string area { get; set; }

        public string estate { get; set; }

        public string streetName { get; set; }
    }
}
