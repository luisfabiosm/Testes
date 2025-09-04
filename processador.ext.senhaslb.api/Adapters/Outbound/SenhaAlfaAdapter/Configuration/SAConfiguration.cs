using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;

namespace Adapters.Outbound.SenhaAlfaAdapter.Configuration
{
    public static class SAConfiguration
    {
        public static IServiceCollection AddSAAdapter(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IntegracaoSettings>(options =>
            {
                var _settings = configuration.GetSection("AppSettings").GetSection("Integracao");

                options.SA = new SAConfig
                {
                    Url = _settings.GetValue<string>("SA:Url")!
                    //Url = Environment.GetEnvironmentVariable("SA_URL")!
                };
            });

            services.AddHttpClient<ISAServicePort, SAService>();
            //services.AddScoped<ISAServicePort, SAService>();
            return services;
        }
    }
}
