using CPEA.Utilities.InterswitchClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IInterSwitch
    {
        Task<string> PaymentValidation(string request);
    }
}
