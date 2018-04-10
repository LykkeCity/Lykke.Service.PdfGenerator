using System.Threading.Tasks;
using Lykke.Service.PdfGenerator.Contract;

namespace Lykke.Service.PdfGenerator.Core.Services
{
    public interface IPdfGeneratorService
    {
        /// <summary>
        /// Generate pdf from html and save it in Azure Blob Storage,
        /// returns BlobName in case of success
        /// </summary>
        /// <param name="htmlSource">HTML which should be converted to PDF</param>
        /// <param name="blobName">Name of the blob file in the storage</param>
        /// <param name="fileName">Additional metadata name</param>
        Task<string> Generate(string htmlSource, string blobName, string fileName);

        Task<PdfDataResponse> GetData(string blobName);

        Task<bool> IsExist(string blobName);
    }
}
