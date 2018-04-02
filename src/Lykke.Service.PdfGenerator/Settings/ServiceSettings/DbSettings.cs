using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PdfGenerator.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
