using System;
using Common.Log;

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
    }
}
