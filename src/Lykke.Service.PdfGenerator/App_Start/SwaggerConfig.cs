using System.Web.Http;
using WebActivatorEx;
using Lykke.Service.PdfGenerator;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Lykke.Service.PdfGenerator
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c => c.SingleApiVersion("v1", "Lykke.Service.PdfGenerator"))
                .EnableSwaggerUi();
        }
    }
}
