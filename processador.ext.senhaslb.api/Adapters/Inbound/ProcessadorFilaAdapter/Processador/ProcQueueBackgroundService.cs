using Adapters.Outbound.OtlpAdapter;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Processo;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.UseCases;
using System.Diagnostics;
using Domain.Core.Base;

namespace Adapters.Inbound.ProcessadorFilaAdapter.Processador
{
    public class ProcQueueBackgroundService : BaseBackgroundService
    {
        #region variáveis

        private readonly ILogger<ProcQueueBackgroundService> _logger;
        private readonly ConcurrentQueue<TransacaoWorkItem> _filaProcesso;
        private readonly IProcessadorSenhaSilabicaPort _processadorSPA;
        private readonly IOptions<SPASettings> _spaSettings;

        #endregion

        public ProcQueueBackgroundService(IServiceProvider serviceProvider, ILogger<ProcQueueBackgroundService> logger) : base(serviceProvider)
        {
            _logger = logger;
            _filaProcesso = serviceProvider.GetRequiredService<ConcurrentQueue<TransacaoWorkItem>>();
            _processadorSPA = serviceProvider.GetRequiredService<IProcessadorSenhaSilabicaPort>();
            _spaSettings = serviceProvider.GetRequiredService<IOptions<SPASettings>>();
        }

        public void QueueTransacao(TransacaoWorkItem processo)
        {
            _filaProcesso.Enqueue(processo);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("ProcQueueBackgroundService service is running");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_filaProcesso.TryDequeue(out var _processo))
                {
                    Console.WriteLine("ProcQueueBackgroundService TryDequeue");
                    var activityLinks = new List<ActivityLink>();

                    if (ActivityContext.TryParse(_processo.ActivityId, null, out var context))
                    {
                        activityLinks.Add(new ActivityLink(context));
                    }

                    using var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("ProcQueueBackgroundService: ProcessarTransacao",
                        ActivityKind.Internal,
                        parentContext: default,
                        links: activityLinks);

                    _activity?.SetTag("Mensagem lida", _processo.Transacao.SPAMensagemIN);
                    _activity?.SetTag("processamento_fila_timestamp", DateTime.Now);

                    try
                    {
                        await _processadorSPA.ProcessarTransacao(_processo.Transacao, _activity!, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        using var errorActivity = OtlpActivityService.GenerateActivitySource.StartActivity("ProcQueueBackgroundService: Error", ActivityKind.Internal);

                        errorActivity?.SetTag("error", true);
                        errorActivity?.SetTag("error.message", ex.Message);
                        errorActivity?.SetTag("error.stack_trace", ex.StackTrace);

                        LogError($"[ProcQueueBackgroundService]", ex);
                        _logger.LogError(ex, $"ERRO: Ocorreu um erro no processamento da fila, item: {_processo}");
                    }
                }
                else
                {
                    if (_filaProcesso.Count > 0)
                    {
                        _logger.LogInformation($"###########################################");
                        _logger.LogInformation($"[{DateTime.UtcNow}][{Thread.CurrentThread.ManagedThreadId.ToString("D6")}] MENSAGENS NA FILA: {_filaProcesso.Count}");
                        _logger.LogInformation($"###########################################");
                    }

                    // Wait before checking queue again to prevent tight looping
                    await Task.Delay(_spaSettings.Value.QueueDelay, stoppingToken);
                }
            }
        }
    }
}
