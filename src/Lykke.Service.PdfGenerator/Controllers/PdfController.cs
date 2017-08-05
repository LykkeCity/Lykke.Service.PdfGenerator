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
//using SelectPdf;


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
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    // instantiate a html to pdf converter object
                //    HtmlToPdf converter = new HtmlToPdf();

                //    // set converter options
                //    converter.Options.PdfPageSize = PdfPageSize.A4;
                //    converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                //    // create a new pdf document converting an html string
                //    PdfDocument doc = converter.ConvertHtmlString(model.HtmlSource);

                //    // save pdf document
                //    doc.Save(ms);

                //    // close pdf document
                //    doc.Close();
                //    data = ms.ToArray();
                //}

                data = GeneratePdfByItext(model.HtmlSource);

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

        private byte[] GeneratePdfByItext(string htmlSource)
        {//Create a byte array that will eventually hold our final PDF
            Byte[] bytes;

            //Boilerplate iTextSharp setup here
            //Create a stream that we can write to, in this case a MemoryStream
            using (var ms = new MemoryStream())
            {

                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                using (var doc = new Document())
                {

                    //Create a writer that's bound to our PDF abstraction and our stream
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {

                        //Open the document for writing
                        doc.Open();

                        ////Our sample HTML and CSS
                        //var example_html = @"<p>This <em>is </em><span class=""headline"" style=""text-decoration: underline;"">some</span> <strong>sample <em> text</em></strong><span style=""color: red;"">!!!</span></p>";
                        //var example_css = @".headline{font-size:200%}";


                        /**************************************************
                         * Example #2                                     *
                         *                                                *
                         * Use the XMLWorker to parse the HTML.           *
                         * Only inline CSS and absolutely linked          *
                         * CSS is supported                               *
                         * ************************************************/

                        //XMLWorker also reads from a TextReader and not directly from a string
                        using (var srHtml = new StringReader(htmlSource))
                        {

                            //Parse the HTML
                            iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                        }

                        ///**************************************************
                        // * Example #3                                     *
                        // *                                                *
                        // * Use the XMLWorker to parse HTML and CSS        *
                        // * ************************************************/

                        ////In order to read CSS as a string we need to switch to a different constructor
                        ////that takes Streams instead of TextReaders.
                        ////Below we convert the strings into UTF8 byte array and wrap those in MemoryStreams
                        //using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(example_css)))
                        //{
                        //    using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(example_html)))
                        //    {

                        //        //Parse the HTML
                        //        iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msHtml, msCss);
                        //    }
                        //}


                        doc.Close();
                    }
                }

                //After all of the PDF "stuff" above is done and closed but **before** we
                //close the MemoryStream, grab all of the active bytes from the stream
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
