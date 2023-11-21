using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.RespayClasses
{
    public class BulkPOUpload
    {
        public ICollection<BulkPOField> FilesOnFileSystem { get; set; }
        public IFormFile bulkFile { get; set; }
    }
    public class BulkPOField
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }
}
