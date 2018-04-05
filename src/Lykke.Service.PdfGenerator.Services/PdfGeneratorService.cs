using Common.Log;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Lykke.Service.PdfGenerator.Core.Repositories;
using Lykke.Service.PdfGenerator.Core.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.PdfGenerator.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IPdfGeneratorRepository _pdfGeneratorRepository;
        private readonly ILog _log;
        private const string PdfMimeType = ".pdf";

        public PdfGeneratorService(
            IPdfGeneratorRepository pdfGeneratorRepository,
            ILog log)
        {
            _pdfGeneratorRepository = pdfGeneratorRepository;
            _log = log.CreateComponentScope(nameof(PdfGeneratorService));
        }

        /// <inheritdoc />
        public async Task<string> Generate(string htmlSource, string blobName, string fileName)
        {
            if (string.IsNullOrEmpty(blobName))
                blobName = Guid.NewGuid().ToString();

            if (blobName.LastIndexOf(PdfMimeType, StringComparison.InvariantCultureIgnoreCase) == -1)
                blobName += PdfMimeType;

            if (string.IsNullOrEmpty(fileName))
                fileName = blobName;

            _log.WriteInfo(nameof(GeneratePdfByItext7), new {blobName, fileName} , "Started");
            var sw = new Stopwatch();
            sw.Start();
            
            var data = GeneratePdfByItext7(htmlSource);
            await _pdfGeneratorRepository.StoreDataAsync(data, blobName, fileName);
            
            sw.Stop();
            _log.WriteInfo(nameof(GeneratePdfByItext7), new { blobName, fileName, ElapsedTime = sw.ElapsedMilliseconds }, "Finished");

            return blobName;
        }

        private static byte[] GeneratePdfByItext7(string htmlSource)
        {
            byte[] bytes;
            
            // create a stream that we can write to, in this case a MemoryStream
            using (var ms = new MemoryStream())
            {
                // use a PdfWriter instance to write to the stream
                using (var pdfWriter = new PdfWriter(ms))
                {
                    HtmlConverter.ConvertToPdf(htmlSource, pdfWriter);
                }

                bytes = ms.ToArray();
            }

            return bytes;
        }
    }
}
