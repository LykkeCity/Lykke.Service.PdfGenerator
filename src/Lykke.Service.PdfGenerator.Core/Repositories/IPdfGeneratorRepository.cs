using System.Threading.Tasks;

namespace Lykke.Service.PdfGenerator.Core.Repositories
{
    public interface IPdfGeneratorRepository
    {
        Task StoreDataAsync(byte[] data, string blobName, string fileName);
        Task<(byte[] Data, string FileName)> GetDataAsync(string blobName);
        Task<bool> IsExistAsync(string blobName);
    }
}
