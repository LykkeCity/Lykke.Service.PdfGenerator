using AzureStorage;
using Common;
using Common.Log;
using Lykke.Service.PdfGenerator.Core.Repositories;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Lykke.Service.PdfGenerator.AzureRepositories
{
    public class PdfGeneratorRepository : IPdfGeneratorRepository
    {
        private readonly IBlobStorage _blobStorage;
        private readonly string _containerName;

        public PdfGeneratorRepository(
            IBlobStorage blobStorage,
            string containerName)
        {
            _blobStorage = blobStorage;
            _containerName = containerName;
        }
        
        public async Task StoreDataAsync(byte[] data, string blobName, string fileName)
        {
            var metadata = new Dictionary<string, string> {{nameof(fileName), fileName}};
            await _blobStorage.SaveBlobAsync(_containerName, blobName, data, metadata);
        }
    }
}
