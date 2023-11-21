using CPEA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.ViewModel
{
    public class ProgramVM
    {
        public List<Programs> ProgramList { get; set; }
        public Programs newProgram { get; set; }
    }
}
