using JetBrains.Annotations;
using Lykke.Service.PdfGenerator.Settings.ServiceSettings;
using Lykke.Service.PdfGenerator.Settings.SlackNotifications;

namespace Lykke.Service.PdfGenerator.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public PdfGeneratorSettings PdfGeneratorService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
