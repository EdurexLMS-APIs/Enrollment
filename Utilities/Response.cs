using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities
{
    public class Response<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Successful { get; set; }
    }
    public class ResponseList<T>
    {
        public List<T> Data { get; set; }
        public string Message { get; set; }
        public bool Successful { get; set; }
    }
}
