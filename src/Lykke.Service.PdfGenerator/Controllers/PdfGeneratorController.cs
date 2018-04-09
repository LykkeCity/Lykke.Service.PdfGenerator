using Common;
using Common.Log;
using Lykke.Service.PdfGenerator.Contract;
using Lykke.Service.PdfGenerator.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.PdfGenerator.Settings.ServiceSettings;

namespace Lykke.Service.PdfGenerator.Controllers
{
    [Route("api/pdf")]
    [Produces("application/json")]
    public class PdfGeneratorController : Controller
    {
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly PdfGeneratorSettings _settings;
        private readonly ILog _log;

        public PdfGeneratorController(
            IPdfGeneratorService pdfGeneratorService,
            ILog log,
            PdfGeneratorSettings settings)
        {
            _pdfGeneratorService = pdfGeneratorService;
            _settings = settings;
            _log = log.CreateComponentScope(nameof(PdfGeneratorController));
        }

        /// <summary>
        /// Generate PDF from provided HTML and save it to Azure Blob Storage
        /// </summary>
        /// <param name="model">
        /// TODO: make nuget of contract
        /// </param>
        [HttpPost("generate")]
        [SwaggerOperation("Generate")]
        [ProducesResponseType(typeof(PdfGenerateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PdfGenerateErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Generate([FromBody] PdfGenerateModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.HtmlSource))
                return BadRequest("Please provide HtmlSource data.");
            
            try
            {
                var blobName = await _pdfGeneratorService.Generate(model.HtmlSource, model.BlobName, model.FileName);
                return Ok(new PdfGenerateResponse
                {
                    BlobContainer = _settings.PdfBlobContainer,
                    BlobName = blobName
                });
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(Generate), model, ex);
                var errors = GetAllExceptions(ex).Distinct().ToList();
                return StatusCode((int) HttpStatusCode.InternalServerError,
                    new PdfGenerateErrorResponse {Errors = errors}.ToJson());
            }
        }

        private static IEnumerable<string> GetAllExceptions(Exception ex, List<string> strExceptions = null)
        {
            if (ex == null)
                return strExceptions;

            if (strExceptions == null)
                strExceptions = new List<string>();

            if (ex is AggregateException aggregateException)
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
    }
}
