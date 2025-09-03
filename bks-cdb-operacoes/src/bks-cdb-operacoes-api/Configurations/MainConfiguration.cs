
using Adapters.Inbound.WebApi.Extensions;
using Domain.Core.Common.Serialization;
using Domain.Core.Settings;
using System.Text.Json.Serialization;
using System.Text.Json;



namespace Configurations
{
    public static class MainConfiguration
    {

        public static IServiceCollection ConfigureMicroservice(this IServiceCollection services, IConfiguration configuration)
        {
            AppSettings appSettings = new();
            configuration.GetSection("AppSettings").Bind(appSettings);


            services.ConfigureInboundAdapters(configuration);
            services.ConfigureOutboundAdapters(configuration);
            services.ConfigureDomainAdapters(configuration);
            services.ConfigureSerializeJsonOptions();

            return services;
        }

        public static void UseMicroserviceExtensions(this WebApplication app)
        {
            app.UseAPIExtensions();

        }
    }
}
