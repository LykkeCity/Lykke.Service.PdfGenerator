using System;
using System.Reflection;
using System.Web.Http;

namespace Lykke.Service.PersonalData.Web.Controllers
{
    /// <summary>
    /// Controller to test service is alive
    /// </summary>
    [RoutePrefix("api/isalive")]
    public class IsAliveController : ApiController
    {
        /// <summary>
        /// Checks service is alive
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IsAliveResponse Get()
        {
            return new IsAliveResponse
            {
                Version = this.GetType().Assembly.GetName().Version.ToString(),
                Env = Environment.GetEnvironmentVariable("Env")
            };
        }

        public class IsAliveResponse
        {
            public string Version { get; set; }
            public string Env { get; set; }
        }
    }
}
