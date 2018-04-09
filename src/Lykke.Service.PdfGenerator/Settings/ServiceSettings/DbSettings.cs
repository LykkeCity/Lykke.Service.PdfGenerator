using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PdfGenerator.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureBlobCheck]
        public string BlobConnectionString { get; set; }
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
