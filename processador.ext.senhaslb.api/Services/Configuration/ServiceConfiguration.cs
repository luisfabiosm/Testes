using Adapters.Inbound.ProcessadorFilaAdapter.Configuration;
using Adapters.Outbound.SenhaAlfaAdapter.Configuration;
using Adapters.Inbound.HttpAdapters.Configuration;
using Adapters.Outbound.OtlpAdapter.Configuration;
using Adapters.Outbound.TCPAdapter.Configuration;
using Adapters.Outbound.DBAdapter.Configuration;
using Adapters.Inbound.Configuration;
using Domain.Core.Models.Settings;

namespace Services.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AppSettings appSettings = new();
            configuration.GetSection("AppSettings").Bind(appSettings);

            services.AddProcessadorFilaService(configuration, appSettings.SPA!.WithQueue);
            services.AddTCPClientAdapter(configuration);
            services.AddOtlpAdapter(configuration);
            services.AddSAAdapter(configuration);
            services.AddDatabase(configuration);
            services.AddRoutesEndpoints();
            services.AddUseCases();

            return services;
        }
    }
}
