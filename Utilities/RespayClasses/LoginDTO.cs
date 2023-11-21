using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.RespayClasses
{
    public class LoginDTO
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class JSonResponse
    {
        public string code { get; set; }
        public DataRecord data { get; set; }

    }
    public class DataRecord
    {
        public string token { get; set; }
        public UserRecord user { get; set; }
    }
    public class UserRecord
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        //public string phoneNo { get; set; }
        //public string userId { get; set; }
        public string email { get; set; }
        public string defaultRole { get; set; }
    }
}
