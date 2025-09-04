using Adapters.Inbound.HttpAdapters.Mapping;
using Adapters.Inbound.HttpAdapters.Routes;
using Domain.Core.Ports.Inbound;

namespace Adapters.Inbound.HttpAdapters.Configuration
{
    public static class HttpAdapterConfiguration
    {
        public static IServiceCollection AddRoutesEndpoints(this IServiceCollection services)
        {
            services.AddScoped<IMappingToDomainPort, MappingToDomain>();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            
            return services;
        }

        public static void UseAPIExtensions(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.AddProcessadorSPAEndpoint();
        }
    }

}

