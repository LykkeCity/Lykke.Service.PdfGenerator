using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PdfGenerator.Contract;

namespace Lykke.Service.PdfGenerator.Client
{
    public class PdfGeneratorClient : IPdfGeneratorClient, IDisposable
    {
        private readonly ILog _log;

        public PdfGeneratorClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }

        public Task<PdfDataResponse> GetData(string blobName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsExist(string blobName)
        {
            throw new NotImplementedException();
        }
    }
}
