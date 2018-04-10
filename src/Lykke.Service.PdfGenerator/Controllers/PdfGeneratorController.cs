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
        /// <param name="request">
        /// TODO: make nuget of contract
        /// </param>
        [HttpPost("generate")]
        [SwaggerOperation("Generate")]
        [ProducesResponseType(typeof(PdfGenerateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PdfGenerateErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Generate([FromBody] PdfGenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.HtmlSource))
                return BadRequest("Please provide HtmlSource data.");
            
            try
            {
                var blobName = await _pdfGeneratorService.Generate(request.HtmlSource, request.BlobName, request.FileName);
                return Ok(new PdfGenerateResponse
                {
                    BlobContainer = _settings.PdfBlobContainer,
                    BlobName = blobName
                });
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(Generate), request, ex);
                var errors = GetAllExceptions(ex).Distinct().ToList();
                return StatusCode((int) HttpStatusCode.InternalServerError,
                    new PdfGenerateErrorResponse {Errors = errors}.ToJson());
            }
        }

        [HttpGet]
        [SwaggerOperation("IsExist")]
        [ProducesResponseType(typeof(PdfDataResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetData([FromQuery] string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                return BadRequest("Please provide blobName");

            var result = await _pdfGeneratorService.GetData(blobName);
            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation("GetData")]
        [ProducesResponseType(typeof(PdfDataResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> IsExist([FromQuery] string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                return BadRequest("Please provide blobName");

            var result = await _pdfGeneratorService.IsExist(blobName);
            return Ok(result);
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
