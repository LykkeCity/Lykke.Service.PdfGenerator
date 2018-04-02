using System.Threading.Tasks;

namespace Lykke.Service.PdfGenerator.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
