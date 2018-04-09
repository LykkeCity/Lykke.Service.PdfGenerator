using JetBrains.Annotations;

namespace Lykke.Service.PdfGenerator.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PdfGeneratorSettings
    {
        public DbSettings Db { get; set; }
        public string PdfBlobContainer { get; set; }
    }
}
