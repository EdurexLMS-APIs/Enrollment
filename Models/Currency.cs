using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string major_symbol { get; set; }
    }
}
