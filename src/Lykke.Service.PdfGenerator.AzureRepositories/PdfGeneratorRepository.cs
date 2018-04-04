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
        private readonly CloudStorageAccount _storageAccount;
        private readonly IBlobStorage _blobStorage;
        private readonly ILog _log;
        private readonly string _containerName;
        private const string PdfMimeType = ".pdf";

        public PdfGeneratorRepository(
            string blobConnectionString,
            IBlobStorage blobStorage,
            ILog log,
            string containerName)
        {
            _storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            _blobStorage = blobStorage;
            _log = log;
            _containerName = containerName;
        }

        public async Task StoreDataAsync(byte[] data, string blobName, string fileName = null)
        {
            if (blobName.LastIndexOf(PdfMimeType, StringComparison.InvariantCultureIgnoreCase) == -1)
                blobName += PdfMimeType;

            //TODO: extract to AzureStorage
            var metadata = new Dictionary<string, string> {{nameof(fileName), fileName ?? blobName}};
            await SaveBlobAsync(_containerName, blobName, data, metadata);
            //await _blobStorage.SaveBlobAsync(_containerName, blobName, data);
        }

        /// <param name="metadata">Add this parameter to AzureStorage</param>
        private async Task SaveBlobAsync(string container, string key, byte[] blob, IReadOnlyDictionary<string, string> metadata = null)
        {
            var blockBlob = await GetBlockBlobReferenceAsync(container, key, createIfNotExists: true);

            #region add this code to AzureStorage

            if (metadata != null)
            {
                foreach (var keyValuePair in metadata)
                {
                    blockBlob.Metadata[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            #endregion

            blockBlob.Properties.ContentType = "application/pdf";
            await blockBlob.UploadFromByteArrayAsync(blob, 0, blob.Length, null, GetRequestOptions(), null);
        }

        #region AzureStorage internal methods

        private BlobRequestOptions GetRequestOptions()
        {
            return new BlobRequestOptions
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(30)
            };
        }

        private async Task<CloudBlockBlob> GetBlockBlobReferenceAsync(string container, string key, bool anonymousAccess = false, bool createIfNotExists = false)
        {
            NameValidator.ValidateBlobName(key);

            var containerRef = GetContainerReference(container);

            if (createIfNotExists)
            {
                await containerRef.CreateIfNotExistsAsync(GetRequestOptions(), null);
            }
            if (anonymousAccess)
            {
                var permissions = await containerRef.GetPermissionsAsync(null, GetRequestOptions(), null);
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                await containerRef.SetPermissionsAsync(permissions, null, GetRequestOptions(), null);
            }

            return containerRef.GetBlockBlobReference(key);
        }

        private CloudBlobContainer GetContainerReference(string container)
        {
            NameValidator.ValidateContainerName(container);

            var blobClient = _storageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(container);
        }

        #endregion
    }
}
