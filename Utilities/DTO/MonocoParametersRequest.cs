using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.DTO
{
    public class MonocoParametersRequest
    {
        public string amount { get; set; } //in kobo
        public string type { get; set; }
        public string description { get; set; }
        public string reference { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public int? duration { get; set; }
        public string interval { get; set; }
        public string redirect_url { get; set; }
    }
}
