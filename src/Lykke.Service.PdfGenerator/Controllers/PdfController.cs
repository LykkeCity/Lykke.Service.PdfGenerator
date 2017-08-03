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
                using (MemoryStream ms = new MemoryStream())
                {
                    var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(model.HtmlSource, PdfSharp.PageSize.A4, 0);
                    pdf.Save(ms);
                    data = ms.ToArray();
                }

                var blobName = Guid.NewGuid().ToString();
                await StoreDataAsync(data, blobName);
                

                return Json(new { BlobContainer = _containerName, BlobName = blobName });
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        private async Task StoreDataAsync(byte[] data, string blobName)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlobConnectionString"));

            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);
            container.CreateIfNotExists();
            
            var blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
        }
    }
}
