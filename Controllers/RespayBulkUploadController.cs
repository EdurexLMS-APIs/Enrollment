using CPEA.Utilities.Interface;
using CPEA.Utilities.RespayClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Data.OleDb;
using CPEA.Utilities;

namespace CPEA.Controllers
{
    public class RespayBulkUploadController : Controller
    {
        private readonly IRespayBulkUpload _bulkUpload;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        public RespayBulkUploadController(IRespayBulkUpload bulkupload, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _bulkUpload = bulkupload;
            _configuration = configuration;
            _environment = environment;
        }
        public IActionResult bulkPO()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> bulkPO(BulkPOUpload file)
        {
            var signinResponse = await _bulkUpload.signin();
            if(signinResponse != null)
            {
                GeneralClass.authToken = signinResponse.data.token;
            }
            //Create a Folder.
            string path = Path.Combine(this._environment.WebRootPath, "Uploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //Save the uploaded Excel file.
            string fileName = Path.GetFileName(file.bulkFile.FileName);
            string filePath = Path.Combine(path, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                file.bulkFile.CopyTo(stream);
            }
            //Read the connection string for the Excel file.
            string conString = this._configuration.GetConnectionString("ExcelConString");
            DataTable dt = new DataTable();
            dt.Columns.Add("firstName", typeof(string));
            dt.Columns.Add("lastName", typeof(string));
            dt.Columns.Add("companyName", typeof(string));
            dt.Columns.Add("email", typeof(string));
            dt.Columns.Add("phone", typeof(string));

            conString = string.Format(conString, fileName);

            using (OleDbConnection connExcel = new OleDbConnection(conString))
            {
                using (OleDbCommand cmdExcel = new OleDbCommand())
                {
                    using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                    {
                        cmdExcel.Connection = connExcel;

                        //Get the name of First Sheet.
                        connExcel.Open();
                        DataTable dtExcelSchema;
                        dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        string sheetName = dtExcelSchema.Rows[0]["POBulk"].ToString();
                        connExcel.Close();

                        //Read Data from First Sheet.
                        connExcel.Open();
                        cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                        odaExcel.SelectCommand = cmdExcel;
                        odaExcel.Fill(dt);
                        connExcel.Close();
                    }
                }
            }

            var bulkUp = new BulkPOUpload();
            foreach (DataRow row in dt.Rows)
            {
                var eachrec = new BulkPOField();
                foreach (DataColumn col in dt.Columns)
                {
                    eachrec.companyName = row[col.ColumnName].ToString();
                }
                bulkUp.FilesOnFileSystem.Add(eachrec);
            }
            
            var bulkPOUploadRes = await _bulkUpload.UploadPO(bulkUp);

            return View();
        }
    }
}
