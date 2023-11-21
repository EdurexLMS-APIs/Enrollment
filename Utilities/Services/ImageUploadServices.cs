using CPEA.Utilities.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CPEA.Utilities.Services
{
    public class ImageUploadServices : IImageUpload
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public ImageUploadServices(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<Tuple<string, string>> UploadPhoto(IFormFile file,string institutionShortName)
        {
            try
            {
                if (file.Length < 0)
                {
                    return await Task.FromResult(new Tuple<string, string>("No file selected", null));
                }

                // Get uploaded file
                var folderName = "InstitutionLogos";
                var webRootPath = _hostingEnvironment.WebRootPath;
                var pathToSave = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                var dbPath = string.Empty;
                var fullpath = string.Empty;
                string ext = Path.GetExtension(file.FileName);
                var filename = institutionShortName + ext;
                fullpath = Path.Combine(pathToSave, filename);

                dbPath = Path.Combine(folderName, filename);

                if (!(ext.ToLower() == ".jpg" || ext.ToLower() == ".gif" || ext.ToLower() == ".jpeg" || ext.ToLower() == ".png"))
                {
                    return await Task.FromResult(new Tuple<string, string>("Invalid file type", null));
                }
                using (var stream = new FileStream(fullpath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return await Task.FromResult(new Tuple<string, string>("Successful", dbPath));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
