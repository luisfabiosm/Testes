using Domain.Core.Common.Mediator;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Ports.Domain;
using Domain.Services;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Configurations
{
    public static class DomainConfiguration
    {

        public static IServiceCollection ConfigureDomainAdapters(this IServiceCollection services, IConfiguration configuration)
        {

            #region Performance Services 

            // CorrelationId Generator otimizado
            services.AddSingleton<CorrelationIdGenerator>();

            // Configuração de logging otimizado
            services.Configure<LoggerFilterOptions>(options =>
            {
                // Reduz overhead de logging em produção
                options.MinLevel = LogLevel.Information;
            });

            #endregion


            #region Domain MediatoR  e Handlers - Auto Discovery

            services.AddScoped<BSMediator>();
            services.AddHandlers();

            #endregion


            #region Domain Services

            services.AddSingleton<CorrelationIdGenerator>();
            services.AddScoped<ITransactionFactory, TransactionFactory>();
            services.AddScoped<ContextAccessorService>();


            #endregion


            return services;
        }

        private static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBSRequestHandler<,>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBSRequestHandler<,>));

                services.AddScoped(interfaceType, handlerType);
            }

            return services;
        }
    }
}
