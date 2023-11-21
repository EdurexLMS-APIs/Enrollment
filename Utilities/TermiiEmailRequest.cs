using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities
{
    public class TermiiEmailRequest
    {
        public string to { get; set; }
        public string from { get; set; }
        public string sms { get; set; }
        public string type { get; set; }
        public string api_key { get; set; }
        public string channel { get; set; }
        public mediaTermii media { get; set; }
    }
    public class mediaTermii
    {
        public string url { get; set; }
        public string caption { get; set; }
    }
}
