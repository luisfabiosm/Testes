using Adapters.Inbound.ProcessadorFilaAdapter.Processador;
using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Domain.Core.Ports.Inbound;
using System.Threading.Channels;

namespace Adapters.Inbound.ProcessadorFilaAdapter.Configuration
{
    public static class ProcessadorConfiguration
    {
        public static IServiceCollection AddProcessadorFilaService(this IServiceCollection services, IConfiguration configuration, bool queueServiceIsOn = false)
        {
            services.AddSingleton<Channel<TransacaoSenhaSilabica>>(Channel.CreateUnbounded<TransacaoSenhaSilabica>());

            if (queueServiceIsOn)
            {
                //With Channel
                services.AddHostedService<ProcChannelBackgroundService>();
                services.AddSingleton<IBackgroundTaskQueuePort, BackgroundTaskQueue>();
            }

            return services;
        }
    }
}
