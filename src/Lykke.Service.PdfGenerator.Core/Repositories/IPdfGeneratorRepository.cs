using System.Threading.Tasks;

namespace Lykke.Service.PdfGenerator.Core.Repositories
{
    public interface IPdfGeneratorRepository
    {
        Task StoreDataAsync(byte[] data, string blobName, string fileName);
    }
}
