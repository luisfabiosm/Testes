using Adapters.Outbound.SenhaAlfaAdapter.Models;
using Adapters.Outbound.OtlpAdapter;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using System.Diagnostics;
using Domain.Core.Enums;
using Domain.Core.Base;
using Refit;

namespace Adapters.Outbound.SenhaAlfaAdapter
{
    public class SAService : BaseService, ISAServicePort
    {
        private readonly IOptions<IntegracaoSettings> _configSettings;
        private readonly ISPAOperadorService _spaOperadorService;
        private readonly HttpClient _httpClient;

        public SAService(IServiceProvider serviceProvider, HttpClient httpClient) : base(serviceProvider)
        {
            _configSettings = serviceProvider.GetRequiredService<IOptions<IntegracaoSettings>>();
            _spaOperadorService = serviceProvider.GetRequiredService<ISPAOperadorService>();
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<GerarSaidaSenhaResponse> ExecutarApiExtGerarSaidaSenha(string msgOut)
        {
            using var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### INICIANDO CHAMADA À API EXTERNA ####", ActivityKind.Internal);

            _activity?.AddEvent(new ActivityEvent("Rota: srvsuins/senha-alfa-api/GerarSaidaSenha"));

            var _request = ExtrairDadosRetornoSPA(msgOut);
            var tipoSaque = _request.tipoSaque;
            var agencia = _request.agencia;
            var conta = _request.conta;

            var _url = _configSettings.Value.SA.Url;
            _httpClient.BaseAddress = new Uri(_url); 

            var _clientAPI = RestService.For<IRefitClientSA>(_httpClient);

            _activity?.SetTag("URL de conexão", _url);
            _activity?.SetTag("Request - tipoSaque", tipoSaque);
            _activity?.SetTag("Request - agencia", agencia);
            _activity?.SetTag("Request - conta", conta);

            try
            {
                _activity?.AddEvent(new ActivityEvent("#### CHAMANDO MÉTODO EXTERNO DE GERAÇÃO DE SAÍDA DA SENHA ####"));

                var _retSA = await _clientAPI.GerarSaidaSenha(new SenhaAlfaRequest(tipoSaque, agencia, conta));

                if ((EnumStatus)_retSA.returnCode != EnumStatus.SUCESSO)
                    throw new InvalidOperationException("Erro ao gerar saída.");

                _activity?.SetTag("Retorno - ExecutarAPI: ", $"{_retSA.dataHora}, {_retSA.seq1}, {_retSA.seq2}, {_retSA.seq3}, {_retSA.grupo8}, {_retSA.returnCode}");

                return _retSA;
            }
            catch (Exception ex)
            {
                _activity?.SetStatus(ActivityStatusCode.Error);
                _activity?.SetTag("Erro", ex.Message);
                _activity?.SetTag("Stacktrace", ex.StackTrace);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<TestarSenhaResponse> ExecutarApiExtTestarSenha(string msgOut)
        {
            using var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### INICIANDO CHAMADA À API EXTERNA ####", ActivityKind.Internal);

            var _request = ExtrairDadosRetornoSPA(msgOut);

            var tipoSaque = _request.tipoSaque;
            var agencia = _request.agencia;
            var conta = _request.conta;
            var dataHora = _request.dataHora;
            var senhaBase = _request.senhaBase;
            var seqBotoes = _request.seqBotoes;

            var _url = _configSettings.Value.SA.Url;
            _httpClient.BaseAddress = new Uri(_url);

            var _clientAPI = RestService.For<IRefitClientSA>(_httpClient);

            _activity?.SetTag("URL de conexão", _url);
            _activity?.SetTag("Request - tipoSaque", tipoSaque);
            _activity?.SetTag("Request - agencia", agencia);
            _activity?.SetTag("Request - conta", conta);
            _activity?.SetTag("Request - dataHora", dataHora);
            _activity?.SetTag("Request - senhaBase", senhaBase);
            _activity?.SetTag("Request - seqBotoes", seqBotoes);

            try
            {
                _activity?.AddEvent(new ActivityEvent("#### CHAMANDO MÉTODO EXTERNO DE TESTE DE SENHA ####"));

                var _retSA = await _clientAPI.TestarSenha(new SenhaAlfaRequest(tipoSaque, agencia, conta, dataHora, senhaBase, seqBotoes));

                if (_retSA.returnCode != EnumStatus.SUCESSO)
                    throw new InvalidOperationException("Erro ao testar senha.");

                return _retSA;
            }
            catch (Exception ex)
            {
                _activity?.SetStatus(ActivityStatusCode.Error);
                _activity?.SetTag("Erro", ex.Message);
                _activity?.SetTag("Stacktrace", ex.StackTrace);
                throw new InvalidOperationException(ex.Message);
            }
        }

        private SenhaAlfaRequest ExtrairDadosRetornoSPA(string retornoSPA)
        {
            using var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### EXTRAINDO DADOS DE RETORNO DA SPA ####", ActivityKind.Internal);

            var transacao = _spaOperadorService.GetTransacaoAtiva();

            if (string.IsNullOrEmpty(retornoSPA))
                throw new ArgumentException("O parâmetro de retorno está vazio.", nameof(retornoSPA));

            var _parteSplit = retornoSPA.Split('|');

            if (_parteSplit.Length < 6)
                throw new FormatException("O formato do parâmetro de retorno é inválido!");

            if (transacao.Codigo == 740)
            {
                return new SenhaAlfaRequest(int.Parse(_parteSplit[1]), _parteSplit[2], _parteSplit[3]);
            }
            else if (transacao.Codigo == 741)
            {
                return new SenhaAlfaRequest(
                    int.Parse(_parteSplit[1]),
                    _parteSplit[2],
                    _parteSplit[3],
                    int.Parse(_parteSplit[4]),
                    _parteSplit[5],
                    _parteSplit[6]);
            }
            else
            {
                throw new InvalidOperationException($"Código de transação inválido: {transacao.Codigo}");
            }
        }
    }
}
