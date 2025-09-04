using Adapters.Outbound.OtlpAdapter.Extensions;
using Adapters.Outbound.DBAdapter.Model;
using Adapters.Outbound.OtlpAdapter;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using Domain.Core.Ports.UseCases;
using W3Socket.Core.Models.SPA;
using Domain.Core.Exceptions;
using Domain.Core.Models.SPA;
using System.Globalization;
using System.Diagnostics;
using Domain.Core.Enums;
using Domain.Core.Base;
using Newtonsoft.Json;
using System.Text;

namespace Domain.UseCases.ProcessarTransacaoSenhaSilabica
{
    public class ProcessadorSenhaSilabica : BaseService, IProcessadorSenhaSilabicaPort
    {
        #region variaveis

        private readonly ISPATcpClientServicePort _spaTCPService;
        private readonly ISPAOperadorService _spaOperadorService;
        private readonly IOptions<GCSrvSettings> _gcSettings;
        private readonly ISAServicePort _senhaAlfaServicePort;

        #endregion

        public ProcessadorSenhaSilabica(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<ProcessadorSenhaSilabica>>();
            _spaTCPService = serviceProvider.GetRequiredService<ISPATcpClientServicePort>();
            _spaOperadorService = serviceProvider.GetRequiredService<ISPAOperadorService>();
            _gcSettings = serviceProvider.GetRequiredService<IOptions<GCSrvSettings>>();
            _senhaAlfaServicePort = serviceProvider.GetRequiredService<ISAServicePort>();
        }

        public async Task ProcessarTransacao(TransacaoSenhaSilabica transacao, Activity parentActivity, CancellationToken cancellationToken)
        {
            Activity? _activity = null;

            _activity = OtlpActivityService.GenerateActivitySource.StartActivity(
                $"ProcessadorSPA: ProcessarTransacao {transacao.SPAMensagemIN.Transacao}",
                ActivityKind.Internal,
                parentActivity?.Context ?? default);

            Console.WriteLine($"ProcessadorSPA: ProcessarTransacao Mensagem - {transacao.CorrelationId} ");
            _activity?.SetTag("inicio_processamento_timestamp", DateTime.UtcNow);


            if ((ValidarGarbage(transacao.CabecalhoSPA) == true) && (transacao.MetodoAcao != EnumMetodoAcao.ACAO_CANCELAR && transacao.MetodoAcao != EnumMetodoAcao.ACAO_CONFIRMAR))
            {
                Console.WriteLine($"[{DateTime.Now}] #####################################################################");
                Console.WriteLine($"[{DateTime.Now}] MENSAGEM DESCARTADA (GARBAGE), Cracha {transacao.CabecalhoSPA.cracha}");
                Console.WriteLine($"[{DateTime.Now}] #####################################################################");
                LogError($"[ProcessadorSPA: ProcessarTransacao]", $"[{DateTime.Now}] MENSAGEM DESCARTADA (GARBAGE), Cracha {transacao.CabecalhoSPA.cracha}");
                return;
            }

            try
            {
                _activity?.SetStatus(ActivityStatusCode.Ok);

                _activity?.AddTags(new Dictionary<string, object>
                    {
                        {    "Codigo", transacao.SPAMensagemIN.Transacao             },
                        {    "Operador", transacao.SPAMensagemIN.DadosSPA[0]         },
                        {    "Canal", transacao.SPAMensagemIN.DadosSPA[3]            },
                        {    "AgenciaOrigem", transacao.SPAMensagemIN.DadosSPA[7]    },
                        {    "PostoOrigem", transacao.SPAMensagemIN.DadosSPA[8]      },
                        {    "DataContabil", transacao.SPAMensagemIN.DadosSPA[21]    },
                        {    "NSU", transacao.SPAMensagemIN.DadosSPA[9]              },
                        {    "NSUGrupo", transacao.SPAMensagemIN.DadosSPA[10]        }
                    });

                var ret = await _spaOperadorService.IniciarTransacao(transacao, transacao.SPAMensagemIN.DadosSPA);
                TraceFiltro(_activity!, _spaOperadorService.GetTransacaoAtiva().ListParametros!);

                if (ret.Status != EnumStatus.SUCESSO)
                    throw mapBaseReturnToException(ret);

                var _acao = _spaOperadorService.RecuperarAcao();

                #pragma warning disable

                if ((_acao & (EnumAcao.ACAO_CANCELAR | EnumAcao.ACAO_CONFIRMAR)) != 0 &&
                   (transacao.SPAMensagemIN.Situacao0 == EnumSPASituacaoTransacao.Iniciada ||
                    transacao.SPAMensagemIN.Situacao0 == EnumSPASituacaoTransacao.Executada))
                {
                    await _spaOperadorService.RecuperaSituacao();
                }

                #pragma warning restore

                await ExecuteAcao(transacao.MetodoAcao);

                var _response = _spaOperadorService.GetRetornoSPA();
                _activity?.SetTag("Response", _response);


                using (var _handleRetActivity = OtlpActivityService.GenerateActivitySource.StartActivity("ProcessadorSPA: handleSPARetorno", ActivityKind.Internal))
                {
                    _activity?.SetTag("preparando_retorno_timestamp", DateTime.UtcNow);
                    _activity?.SetTag($"Retorno", _spaOperadorService.GetRetornoSPA());

                    await HandleSPARetorno(transacao.MetodoAcao, _spaOperadorService.GetRetornoSPA(), transacao.CabecalhoSPA);
                }

                _activity?.SetTag("fim_processamento_timestamp", DateTime.UtcNow);
            }
            catch (OperationCanceledException exc)
            {
                await HandleException(_activity, transacao, exc);
            }
            catch (Exception ex)
            {
                await HandleException(_activity, transacao, ex);
            }
        }

        private async Task HandleException(Activity? activity, TransacaoSenhaSilabica transacao, Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddTags(new Dictionary<string, object>
            {
                { "Erro", ex.Message },
                { "Stacktrace", ex.StackTrace! }
            });

            var baseReturn = MapExceptionToBaseReturn(transacao, ex);
            await HandleSPARetornoErro("ProcessadorSPA: ProcessarTransacao", baseReturn, transacao.CabecalhoSPA, _spaOperadorService.GetTransacaoAtiva().IsGarbage);
        }

        private async Task ResponderTransacao(SPARetorno notification)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity($"ProcessadorSPA: ResponderTransacao", ActivityKind.Internal))
            {
                try
                {
                    _activity?.SetTag($"Destino", _spaTCPService.GetEndpoint());
                    _activity?.SetTag("Mensagem", notification.MensagemRetorno);
                    _activity?.SetTag("respondendo_sparouter_timestamp", DateTime.Now);

                    Console.WriteLine($"SPAOperadorService: ResponderTransacao - {_spaTCPService.GetEndpoint()} ");
                    Console.WriteLine($"ByteRetorno: {notification.ByteRetorno} ");
                    Console.WriteLine($"Mensagem: {notification.MensagemRetorno} ");

                    await _spaTCPService.SendResponse(notification.CabecalhoRetorno, notification.ByteRetorno);
                }
                catch (Exception ex)
                {
                    _activity?.SetStatus(ActivityStatusCode.Error);
                    _activity?.SetTag("Erro", $"{ex.Message}");
                    _activity?.SetTag("Stacktrace", $"{ex.StackTrace}");

                    LogError($"[ProcessadorSPA: ResponderTransacao]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                }
            }
        }

        public async Task ExecuteAcao(EnumMetodoAcao metodo)
        {
            Console.WriteLine($"ProcessadorSPA: ExecuteAcao - {metodo} ");

            if (metodo == EnumMetodoAcao.ACAO_VALIDAR)
                await _spaOperadorService.ValidarTransacao();

            else if (metodo == EnumMetodoAcao.ACAO_EXECUTAR)
                await Executar();

            else if (metodo == EnumMetodoAcao.ACAO_CONFIRMAR)
                await _spaOperadorService.ConfirmarTransacao();

            else if (metodo == EnumMetodoAcao.ACAO_CANCELAR)
                await _spaOperadorService.CancelarTransacao();
        }

        public async Task<string> Executar()
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### PROCESSADORSLB: INICIANDO EXECUÇÃO DO FLUXO PRINCIPAL ####", ActivityKind.Internal))
            {
                var _transacao = _spaOperadorService.GetTransacaoAtiva();

                _activity?.SetTag("Transação disparada", _transacao.Codigo);

                try
                {
                    _activity?.AddEvent(new ActivityEvent($"#### INICIANDO EXECUÇÃO DA TRANSAÇÃO {_transacao.Codigo} NO BANCO DE DADOS ####"));
                    await _spaOperadorService.ExecutarTransacao();

                    var _msgOut = _transacao.ListParametros!.Find(item => item._oSQLParameter!.ParameterName == "@pvchMsgOUT")?.Valor?.ToString() ?? "0";
                    var _parametrosConcatenados = ConcatenarParametros(_transacao);

                    _activity?.SetTag("Parâmetro recuperado - pvchMsgOUT", _msgOut);

                    return _transacao.Codigo switch
                    {
                        740 => await ExecutarFluxo740(_msgOut, _parametrosConcatenados),
                        741 => await ExecutarFluxo741(_msgOut, _parametrosConcatenados),
                        _ => throw new InvalidOperationException($"Código de transação inválido: {_transacao.Codigo}")
                    };
                }
                catch (Exception ex)
                {
                    _activity?.SetStatus(ActivityStatusCode.Error);
                    _activity?.SetTag("Erro", ex.Message);
                    _activity?.SetTag("Stacktrace", ex.StackTrace);

                    throw handleError(ex, "Executar");
                }
            }
        }

        public virtual async Task<string> ExecutarFluxo740(string _msgOut, string _dados)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### EXECUTANDO FLUXO DA TRANSAÇÃO 740 ####", ActivityKind.Internal))
            {
                var _retSA = await _senhaAlfaServicePort.ExecutarApiExtGerarSaidaSenha(_msgOut);
                if ((EnumStatus)_retSA.returnCode != EnumStatus.SUCESSO)
                    throw handleError(new Exception(), "Erro na execução da API externa");

                var _parteSplit = _msgOut.Split('|');
                var _resposta = _parteSplit[5];

                var _entradaSPX = new ParametrosSPX(_dados, _resposta, _retSA.seq1!, _retSA.seq2!, _retSA.seq3!, _retSA.grupo8!, _retSA.dataHora)
                {
                    Dados = _dados,
                    Resposta = _resposta,
                    Seq1 = _retSA.seq1,
                    Seq2 = _retSA.seq2,
                    Seq3 = _retSA.seq3,
                    Grupo = _retSA.grupo8,
                    Token = _retSA.dataHora
                };

                var _retSPX = await _spaOperadorService.MontarChamadaSPX(_entradaSPX);
                _activity?.SetTag("Retorno - ExecutarSPX: ", _retSPX.ToString());

                return _retSPX.FirstOrDefault()?.vchParam!;
            }
        }

        public virtual async Task<string> ExecutarFluxo741(string _msgOut, string _dados)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### EXECUTANDO FLUXO DA TRANSAÇÃO 741 ####", ActivityKind.Internal))
            {
                var _retSA = await _senhaAlfaServicePort.ExecutarApiExtTestarSenha(_msgOut);
                if (_retSA.returnCode != EnumStatus.SUCESSO)
                    throw handleError(new Exception(), "Erro na execução da API externa");

                var _parteSplit = _msgOut.Split("|");
                var _pvchMsgIN = _parteSplit[9];
                var _pvchMsgOUT = _parteSplit[10];

                var _entradaSPX = new ParametrosSPX(_dados, _pvchMsgIN, _pvchMsgOUT, _retSA.validationResult)
                {
                    Dados = _dados,
                    PvchMsgIN = _pvchMsgIN,
                    PvchMsgOUT = _pvchMsgOUT,
                    PIntRetDLL = _retSA.validationResult,
                };

                var _retSPX = await _spaOperadorService.MontarChamadaSPX(_entradaSPX);
                _activity?.SetTag("Retorno - Executar: ", _retSPX.ToString());

                return _retSPX.FirstOrDefault()?.vchParam!;
            }
        }

        public async Task HandleSPARetorno(EnumMetodoAcao metodo, string retorno, tSPACabecalho cabecalho)
        {
            if (metodo == EnumMetodoAcao.ACAO_CANCELAR || metodo == EnumMetodoAcao.ACAO_CONFIRMAR)
                return;

            if (ValidarGarbage(cabecalho) == true)
            {
                LogError($"[handleSPARetorno]", $"[{DateTime.Now}] MENSAGEM DESCARTADA (GARBAGE), Cracha {cabecalho.cracha}");
                return;
            }

            var _retorno = new SPARetorno(retorno, cabecalho);

            await ResponderTransacao(_retorno);
        }

        public string ConcatenarParametros(SPATransacao transacaoAtiva)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### INICIANDO A CONCATENAÇÃO DOS PARÂMETROS DA TRANSAÇÃO ####", ActivityKind.Internal))
            {
                var parametros = new ParametrosOutput();

                foreach (var prop in typeof(ParametrosOutput).GetProperties())
                {
                    var nomeParametro = "@" + prop.Name;
                    var valorParametro = transacaoAtiva.ListParametros!.Find(item => item._oSQLParameter!.ParameterName == nomeParametro)?.Valor;

                    if (valorParametro == null)
                    {
                        prop.SetValue(parametros, prop.PropertyType == typeof(int) ? 0 : "0");
                    }
                    else
                    {
                        if (prop.PropertyType == typeof(int))
                        {
                            int.TryParse(valorParametro.ToString(), out int intValue);
                            prop.SetValue(parametros, intValue);
                        }
                        else
                        {
                            prop.SetValue(parametros, valorParametro.ToString());
                        }
                    }
                }

                var valoresConcatenados = string.Join("|",
                    typeof(ParametrosOutput)
                        .GetProperties()
                        .Select(p => p.GetValue(parametros)?.ToString() ?? "0")
                );

                _activity?.AddEvent(new ActivityEvent("#### PARÂMETROS CONCATENADOS COM SUCESSO ####"));
                _activity?.SetTag("Parâmetros concatenados", valoresConcatenados);

                return valoresConcatenados;
            }
        }

        public async Task HandleSPARetornoErro(string operation, BaseReturn baseReturn, tSPACabecalho cabecalho, bool garbage = false)
        {
            LogError($"[{operation}]", baseReturn);

            var _retorno = new SPARetorno(baseReturn.Mensagem!, cabecalho);

            if ((garbage) && (_gcSettings.Value.Ativo))
            {
                LogError($"[handleSPARetornoErro]", $"[{DateTime.Now}] MENSAGEM DESCARTADA (GARBAGE), Cracha {cabecalho.cracha}");
                return;
            }

            await ResponderTransacao(_retorno);
        }

        public bool ValidarGarbage(tSPACabecalho cabecalho)
        {

            if (_gcSettings.Value.Ativo && cabecalho.IsGarbage(_gcSettings.Value.AddHoursToUTC))
                return true;

            return false;
        }

        public BaseReturn MapExceptionToBaseReturn(BaseTransacao transacao, Exception ex)
        {
            var _mensagemINString = new string(transacao.MensagemIN.Span);
            if (ex is SPAException spaEx)
            {
                return new BaseReturn(DateTime.UtcNow.ToString(), spaEx.Error.Mensagem, spaEx.Error.Tipo == EnumSPATipoErroInterno.Negocio ? EnumStatus.NEGOCIO : EnumStatus.SISTEMA, spaEx.Error)
                {
                    Mensagem = $"{_mensagemINString}{(char)12}{((int)spaEx.Error.Tipo).ToString("00")}|{spaEx.Error.Codigo}|{RemoverAcentos(spaEx.Error.Mensagem)}|{spaEx.Origem}|{_mensagemINString.Split(MSG_TRAN_SEPCAMPOS)[17]}"
                };
            }
            else
            {
                var spaError = new SPAError(ex);

                return new BaseReturn(DateTime.UtcNow.ToString(), spaError.Mensagem, EnumStatus.SISTEMA, spaError)
                {
                    Mensagem = $"{_mensagemINString}{(char)12}{((int)spaError.Tipo).ToString("00")}|{spaError.Codigo}|{RemoverAcentos(spaError.Mensagem)}|{SPA_OPENSHIFT_SOURCE_IN_ERROR}|{_mensagemINString.Split(MSG_TRAN_SEPCAMPOS)[17]}"
                };
            }
        }

        public void TraceFiltro(Activity activity, List<SPAParametro> SPAParametros)
        {
            var agconta = SPAParametros.Find(item => item.Nome == "@psmlContaAg");
            var conta = SPAParametros.Find(item => item.Nome == "@pintConta") ?? SPAParametros.Find(item => item.Nome == "@pnumConta");
            var valor = SPAParametros.Find(item => item.Nome == "@pnumValor");

            if (agconta != null)
                activity?.SetTag("AgConta", agconta.Valor);

            if (conta != null)
                activity?.SetTag("Conta", conta.Valor);

            if (valor != null)
                activity?.SetTag("Valor", valor.Valor);
        }

        public string RemoverAcentos(string texto)
        {
            var textoNormalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var caractere in textoNormalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);
                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(caractere);
                }
            }

            return sb.ToString();
        }

        public Exception mapBaseReturnToException(BaseReturn baseReturn)
        {
            var _ex = new Exception(baseReturn.Mensagem);
            _ex.Data.Add("BaseReturn", JsonConvert.SerializeObject(baseReturn));
            return _ex;
        }

        ~ProcessadorSenhaSilabica() { }
    }
}
