
using System.Threading.Tasks;
using Lykke.Service.PdfGenerator.Contract;

namespace Lykke.Service.PdfGenerator.Client
{
    public interface IPdfGeneratorClient
    {
        Task<PdfDataResponse> GetData(string blobName);
        Task<bool> IsExist(string blobName);
    }
}
