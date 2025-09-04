using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Domain.Core.Ports.Outbound;
using Domain.Core.Ports.UseCases;
using Domain.Operador;

namespace Adapters.Inbound.Configuration
{
    public static class UseCaseConfiguration
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {      
            services.AddScoped<ISPAOperadorService, SPAOperadorService>();
            services.AddScoped<IProcessadorSenhaSilabicaPort, ProcessadorSenhaSilabica>();
          
            return services;
        }
    }
}
