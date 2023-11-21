using CPEA.Utilities.RespayClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IRespayBulkUpload
    {
        Task<JSonResponse> signin();
        Task<string> UploadPO(BulkPOUpload dto);
    }
}
