using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Adapters.Inbound.HttpAdapters.VM;
using Adapters.Outbound.OtlpAdapter;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.UseCases;
using Domain.Core.Ports.Inbound;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Domain.Core.Enums;
using Domain.Core.Base;

namespace Adapters.Inbound.HttpAdapters.Routes
{
    public static class ProcessadorSpaRoute
    {
        public static void AddProcessadorSPAEndpoint(this WebApplication app)
        {
            app.MapPost("api/spa/v1/processador", ProcRequest)
             .WithTags("Processador SPA")
             .Accepts<TransacaoSpaRequest>("application/json")
             .Produces<string>(StatusCodes.Status200OK)
             .Produces<string>(StatusCodes.Status202Accepted)
             .Produces<BaseError>(StatusCodes.Status400BadRequest)
             .Produces<BaseError>(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> ProcRequest(
        [FromServices] IOptions<SPASettings> _config,
        [FromServices] IMappingToDomainPort _mapping,
        [FromServices] IProcessadorSenhaSilabicaPort _procService,
        [FromServices] Channel<TransacaoSenhaSilabica> _filaProcessador,
        [FromBody] TransacaoSpaRequest request)
        {
            string _correlationId = Guid.NewGuid().ToString();
            Activity? _activity = null;

            try
            {
                _activity = OtlpActivityService.GenerateActivitySource.StartActivity(
                   "ProcessadorSPARoute: ProcRequest",
                   ActivityKind.Server,
                   Activity.Current?.Context ?? default);

                try
                {
                    _activity?.SetTag("TransacaoSPARequest", request);
                    _activity?.SetTag("correlation_id", _correlationId);

                    var _transacaoSPA = _mapping.ToTransacaoSPA(request);
                    _transacaoSPA.CorrelationId = _correlationId;
              
                    Console.WriteLine($"ProcessadorSPARoute:ProcRequest Mensagem - {_correlationId} ");

                    if (!_config.Value.WithQueue)
                    {
                        ////Com TASK
                        Console.WriteLine($"##########################################");
                        Console.WriteLine($"ProcessadorSPARoute:ProcRequest Fire TASK");
                        await Task.Run(async () =>
                        {
                            try
                            {
                                using var _processActivity = OtlpActivityService.GenerateActivitySource.StartActivity(
                                          "ProcessadorSPARoute: ProcessarTransacao", ActivityKind.Internal, _activity?.Context ?? default);

                                await _procService.ProcessarTransacao(_transacaoSPA, _processActivity!, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                _activity?.SetTag("error", true);
                                _activity?.SetTag("error.message", ex.Message);
                                _activity?.SetTag("error.stack_trace", ex.StackTrace);
                            }
                        }, CancellationToken.None).ConfigureAwait(false);
                    }
                    else
                    {
                        Console.WriteLine($"##########################################");
                        Console.WriteLine($"ProcessadorSPARoute:ProcRequest Fire CHANNEL");

                        try
                        {
                            using var _processActivity = OtlpActivityService.GenerateActivitySource.StartActivity(
                                      "ProcessadorSPARoute: ProcessarTransacao",
                                      ActivityKind.Internal,
                                      _activity?.Context ?? default);

                             _transacaoSPA.TranActivity = _processActivity;

                             await _filaProcessador.Writer.WriteAsync(_transacaoSPA);
                        }
                        catch (Exception ex)
                        {
                            _activity?.SetTag("error", true);
                            _activity?.SetTag("error.message", ex.Message);
                            _activity?.SetTag("error.stack_trace", ex.StackTrace);
                        }                        
                    }

                    _activity?.SetTag("Respose imediato :", DateTime.Now);
                    Console.WriteLine($"Respose imediato : {DateTime.Now}");
                    Console.WriteLine($"Fire and Forget ==========================================================");

                    return new BaseReturn().RetornoAccepted(DateTime.Now);
                }
                catch (Exception ex)
                {
                    _activity?.SetTag("error", true);
                    _activity?.SetTag("error.message", ex.Message);
                    _activity?.SetTag("error.stack_trace", ex.StackTrace);

                    return new BaseReturn(ex, EnumReturnType.SYSTEM).RetornoERRO();
                }
            }
            catch (Exception ex)
            {
                if (_activity != null)
                {
                    _activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                }

                Console.WriteLine($"Error in ProcRequest {_correlationId}: {ex.Message}");
                return new BaseReturn(ex, EnumReturnType.SYSTEM).RetornoERRO();
            }
        }
    }
}
