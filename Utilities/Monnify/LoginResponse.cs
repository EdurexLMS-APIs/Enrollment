using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Monnify
{
    public class LoginResponse
    {
        public bool requestSuccessful { get; set; }
        public string responseMessage { get; set; }
        public LoginresponseBody responseBody { get; set; }
    }
    public class LoginresponseBody
    {
        public string accessToken { get; set; }
        public int expiresIn { get; set; }
    }
}
