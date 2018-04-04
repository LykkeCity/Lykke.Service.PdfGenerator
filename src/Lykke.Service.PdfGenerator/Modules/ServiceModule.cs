using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Blob;
using Common.Log;
using Lykke.Service.PdfGenerator.AzureRepositories;
using Lykke.Service.PdfGenerator.Core.Repositories;
using Lykke.Service.PdfGenerator.Core.Services;
using Lykke.Service.PdfGenerator.Services;
using Lykke.Service.PdfGenerator.Settings.ServiceSettings;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PdfGenerator.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<PdfGeneratorSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<PdfGeneratorSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            BuildRepositories(builder);

            builder.RegisterType<PdfGeneratorService>().As<IPdfGeneratorService>();

            builder.Populate(_services);
        }

        private void BuildRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IPdfGeneratorRepository>(new PdfGeneratorRepository(
                _settings.CurrentValue.Db.BlobConnectionString,
                AzureBlobStorage.Create(_settings.ConnectionString(x => x.Db.BlobConnectionString)),
                _log,
                _settings.CurrentValue.PdfBlobContainer
            ));
        }
    }
}
