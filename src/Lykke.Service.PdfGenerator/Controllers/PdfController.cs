using Lykke.Service.PdfGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace Lykke.Service.PdfGenerator.Controllers
{
    [RoutePrefix("api/pdf")]
    public class PdfController : ApiController
    {
        private readonly string _containerName;

        public PdfController()
        {
            _containerName = CloudConfigurationManager.GetSetting("PdfBlobContainer");
        }

        [HttpPost]
        [Route("generate")]
        public async Task<IHttpActionResult> Generate(PdfGenerateModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.HtmlSource))
                return BadRequest("Please provide HtmlSource data.");
            
            try
            {
                byte[] data = null;
                data = GeneratePdfByItext(model.HtmlSource);

                var blobName = model.BlobName ?? Guid.NewGuid().ToString();
                var fileName = model.FileName ?? $"{blobName}.pdf";
                await StoreDataAsync(data, blobName, fileName);
                
                return Json(new { BlobContainer = _containerName, BlobName = blobName });
            }
            catch (Exception ex)
            {
                var errors = GetAllExceptions(ex).Distinct().ToList();
                return Json(new { Errors = errors });
            }
        }

        private byte[] GeneratePdfByItext(string htmlSource)
        {
            byte[] bytes;
            
            //Create a stream that we can write to, in this case a MemoryStream
            using (var ms = new MemoryStream())
            {
                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                using (var doc = new Document())
                {

                    //Create a writer that's bound to our PDF abstraction and our stream
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        
                        using (var srHtml = new StringReader(htmlSource))
                        {
                            iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                        }
                        

                        doc.Close();
                    }
                }
                
                bytes = ms.ToArray();
            }

            return bytes;
        }

        private List<string> GetAllExceptions(Exception ex, List<string> strExceptions = null)
        {
            if (ex == null)
                return strExceptions;

            if (strExceptions == null)
                strExceptions = new List<string>();

            var aggregateException = ex as AggregateException;
            if (aggregateException != null)
            {
                foreach (var innerEx in aggregateException.InnerExceptions)
                {
                    strExceptions.AddRange(GetAllExceptions(innerEx, strExceptions));
                }
            }
            else
            {
                strExceptions.Add(ex.ToString());
            }

            if (ex.InnerException != null)
                strExceptions.AddRange(GetAllExceptions(ex.InnerException, strExceptions));

            return strExceptions;
        }

        private async Task StoreDataAsync(byte[] data, string blobName, string fileName)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlobConnectionString"));

            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);
            container.CreateIfNotExists();

            var blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.Metadata["fileName"] = fileName;
            await blockBlob.SetMetadataAsync();
            await blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
        }
    }
}
