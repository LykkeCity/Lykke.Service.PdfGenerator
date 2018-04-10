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
        private readonly ILog _log;

        public PdfGeneratorRepository(
            IBlobStorage blobStorage,
            string containerName,
            ILog log)
        {
            _blobStorage = blobStorage;
            _containerName = containerName;
            _log = log.CreateComponentScope(nameof(PdfGeneratorRepository));
        }
        
        public async Task StoreDataAsync(byte[] data, string blobName, string fileName)
        {
            var metadata = new Dictionary<string, string> {{nameof(fileName), fileName}};
            await _blobStorage.SaveBlobAsync(_containerName, blobName, data, metadata);
        }

        public async Task<(byte[] Data, string FileName)> GetDataAsync(string blobName)
        {
            byte[] data = null;

            try
            {
                using (var stream = await _blobStorage.GetAsync(_containerName, blobName))
                {
                    data = await stream.ToBytesAsync();
                }
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                _log.WriteInfo(nameof(GetDataAsync), new { _containerName, blobName }, $"Cannot get data from storage: {ex.Message}");
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(GetDataAsync), (new { _containerName, blobName }), ex);
            }

            if (data == null)
            {
                return (Data: null, FileName: string.Empty);
            }

            var fileName = await _blobStorage.GetMetadataAsync(_containerName, blobName, "fileName");

            var result = (Data: data, FileName: fileName);
            return result;
        }

        public async Task<bool> IsExistAsync(string blobName)
        {
            try
            {
                return await _blobStorage.HasBlobAsync(_containerName, blobName);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(IsExistAsync), new { _containerName, blobName }, ex);
                return false;
            }
        }
    }
}
