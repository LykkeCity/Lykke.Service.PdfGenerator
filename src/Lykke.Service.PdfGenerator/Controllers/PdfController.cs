﻿using Lykke.Service.PdfGenerator.Models;
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

            //var agrEx = new AggregateException("Test exception", new NotImplementedException("InnerEx1"), new NotImplementedException("InnerEx2"), new NotImplementedException("InnerEx3"));
            //var errMessages = GetAllExceptions(agrEx).Distinct().ToList();

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
                var errors = GetAllExceptions(ex).Distinct().ToList();
                return Json(new { Errors = errors });
                //return StatusCode(HttpStatusCode.InternalServerError);
            }
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
