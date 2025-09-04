using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Adapters.Outbound.OtlpAdapter;
using Domain.Core.Ports.UseCases;
using System.Threading.Channels;
using System.Diagnostics;
using Domain.Core.Base;

namespace Adapters.Inbound.ProcessadorFilaAdapter.Processador
{
    public class ProcChannelBackgroundService : BaseBackgroundService
    {
        #region variaveis

        private readonly ILogger<ProcChannelBackgroundService> _logger;
        private readonly ChannelReader<TransacaoSenhaSilabica> _filaReader;
        private readonly IServiceScopeFactory _scopeFactory;

        #endregion

        public ProcChannelBackgroundService(IServiceProvider serviceProvider, ILogger<ProcChannelBackgroundService> logger, IServiceScopeFactory scopeFactory) : base(serviceProvider)
        {
            _logger = logger;
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

            #pragma warning disable S3928
            _filaReader = serviceProvider.GetRequiredService<Channel<TransacaoSenhaSilabica>>() ?? throw new ArgumentNullException(nameof(_filaReader));
            #pragma warning restore S3928
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("ProcChannelBackgroundService service is running");

            await foreach (var message in _filaReader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    using var _activity = OtlpActivityService.GenerateActivitySource.StartActivity(
                        "ProcChannelBackgroundService: ProcessarTransacao",
                        ActivityKind.Internal,
                        message.TranActivity?.Context ?? default);

                    _activity?.SetTag("Mensagem para fila", message);
                    _activity?.SetTag("recebida_fila_timestamp", DateTime.Now);

                    await ProcessMessageAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    await LogProcessingErrorAsync(ex, message);
                }
            }
        }

        private async Task ProcessMessageAsync(TransacaoSenhaSilabica message, CancellationToken stoppingToken)
        {
            if (_scopeFactory == null)
            {
                throw new InvalidOperationException("ServiceScopeFactory não foi iniciado de forma adequada");
            }

            Console.WriteLine($"ProcChannelBackgroundService: ProcessMessageAsync - {message.CorrelationId} ");
            await using var scope = _scopeFactory.CreateAsyncScope();
            var _processadorSPA = scope.ServiceProvider.GetRequiredService<IProcessadorSenhaSilabicaPort>();
            await _processadorSPA.ProcessarTransacao(message, message.TranActivity!, stoppingToken);
        }

        private Task LogProcessingErrorAsync(Exception ex, TransacaoSenhaSilabica message)
        {
            using var errorActivity = OtlpActivityService.GenerateActivitySource.StartActivity(
                "ProcChannelBackgroundService: Error",
                ActivityKind.Internal);

            errorActivity?.SetTag("error", true);
            errorActivity?.SetTag("error.message", ex.Message);
            errorActivity?.SetTag("error.stack_trace", ex.StackTrace);

            _logger.LogError(ex, "ERRO: Ocorreu um erro no processamento da fila, item: {Message}", message);

            return Task.CompletedTask;
        }
    }
}
