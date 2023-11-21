using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IMessagingService
    {
        Task<bool> SendEmail(string email, string message, string subject);
        Task<bool> SendSMS(string phone, string message);
    }
}
