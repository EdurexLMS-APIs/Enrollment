using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities
{
    public static class GeneralClass
    {
        public static string pRef { get; set; }
        public static string authToken { get; set; }
        public static string email { get; set; } = "";
        public static string deposit { get; set; } = "";
        public static string FullName { get; set; } = "";
        public static string stNumber { get; set; }
        public static string source { get; set; }
        public static string role { get; set; }
        public static bool certOnly { get; set; } = false;
    }
}
