using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Interface
{
   public interface IImageUpload
    {
        Task<Tuple<string,string>> UploadPhoto(IFormFile file, string institutionShortName);

    }
}
