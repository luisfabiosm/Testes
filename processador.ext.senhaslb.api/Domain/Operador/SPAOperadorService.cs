using Adapters.Outbound.DBAdapter.Model;
using Adapters.Outbound.OtlpAdapter;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using Domain.Core.Models.SPA;
using System.Diagnostics;
using Domain.Core.Enums;
using Domain.Core.Base;
using System.Text;

namespace Domain.Operador
{
    public class SPAOperadorService : BaseService, ISPAOperadorService
    {
        public SPADependencia? DependenciaAtiva { get; private set; }
        public SPAOperador? OperadorAtivo { get; internal set; }
        public SPATransacao? TransacaoAtiva { get; protected set; }

        public int ACAO_DEFAULT;
        public string? RetornoSPA { get; private set; }

        #region variáveis

        private readonly ISPARepository _repoSPA;
        private readonly IOptions<SPASettings> _spaConfig;
        private ReadOnlyMemory<char> _headerTransacaoAtiva;

        #endregion

        public SPAOperadorService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _spaConfig = serviceProvider.GetRequiredService<IOptions<SPASettings>>();

            this.DependenciaAtiva = new SPADependencia(_spaConfig.Value);
            this.OperadorAtivo = new SPAOperador(_spaConfig.Value);
            this.TransacaoAtiva = new SPATransacao();

            _repoSPA = serviceProvider.GetRequiredService<ISPARepository>();
        }

        public EnumAcao RecuperarAcao()
        {
            return (EnumAcao)this.TransacaoAtiva!.ParametrosFixos!.Acao;
        }

        public async Task<BaseReturn> IniciarTransacao(BaseTransacao transacao, string[]? dadosTransacao = null)
        {
            try
            {
                Console.WriteLine($"SPAOperadorService: IniciarTransacao - {transacao.Codigo} ");

                this.TransacaoAtiva!.Codigo = transacao.Codigo;
                this._headerTransacaoAtiva = transacao.MensagemIN.Slice(0, 14);

                await _repoSPA.IniciarSPATransacao(this.DependenciaAtiva!.Agencia, this.DependenciaAtiva.Posto, this.TransacaoAtiva);
                this.TransacaoAtiva.ConfigSPATransacao(transacao.Codigo, dadosTransacao!);

                ACAO_DEFAULT = (this.TransacaoAtiva.TransacaoGravaLog) ? (int)EnumAcao.ACAO_REGISTRAR : 0;
                return new BaseReturn();
            }
            catch (Exception ex)
            {
                throw handleError(ex, "IniciarTransacao");
            }
        }

        #region NEW

        public async Task ValidarTransacao()
        {
            try
            {
                await ProcessarAcaoDB(this.RecuperarAcao(), TransacaoAtiva!.Situacao0);
            }
            catch (Exception ex)
            {
                throw handleError(ex, "ValidarTransacao");
            }
        }

        public async Task ExecutarTransacao()
        {
            try
            {
                await ProcessarAcaoDB(this.RecuperarAcao(), EnumSPASituacaoTransacao.Executada);
                TransacaoAtiva!.Situacao0 = EnumSPASituacaoTransacao.Executada;
            }
            catch (Exception ex)
            {
                await CancelarTransacao();
                throw handleError(ex, "ExecutarTransacao");
            }
        }

        public async Task ConfirmarTransacao()
        {
            try
            {
                if ((this.TransacaoAtiva!.TransacaoGravaLog) && (this.TransacaoAtiva.Situacao0 == EnumSPASituacaoTransacao.Executada))
                    await ProcessarAcaoDB(this.RecuperarAcao(), EnumSPASituacaoTransacao.Confirmada);

                TransacaoAtiva.Situacao0 = EnumSPASituacaoTransacao.Confirmada;
            }
            catch (Exception ex)
            {
                LogError($"[ConfirmarTransacao]", $"Erro na confirmação, porem ocorreu erro na ultima perna {ex.Message} Stacktrace {ex.StackTrace}");
                throw handleError(ex, "ConfirmarTransacao");
            }
        }

        public async Task CancelarTransacao()
        {
            try
            {
                if (
                    (this.TransacaoAtiva!.TransacaoGravaLog) &&
                    (this.TransacaoAtiva.Situacao0 == EnumSPASituacaoTransacao.Executada) ||
                    (this.TransacaoAtiva.Situacao0 == EnumSPASituacaoTransacao.Confirmada)
                   )
                    await ProcessarAcaoDB(this.RecuperarAcao(), EnumSPASituacaoTransacao.Cancelada);

                TransacaoAtiva.Situacao0 = EnumSPASituacaoTransacao.Cancelada;

            }
            catch (Exception ex)
            {
                LogError($"[CancelarTransacao]", $"Erro no cancelamento, porem ocorreu erro na ultima perna {ex.Message} Stacktrace {ex.StackTrace}");
                throw handleError(ex, "CancelarTransacao");
            }
        }

        public async Task RecuperaSituacao()
        {
            this.TransacaoAtiva!.Situacao0 = await _repoSPA.RecuperarSituacao(TransacaoAtiva);
        }

        public SPATransacao GetTransacaoAtiva()
        {
            return this.TransacaoAtiva!;
        }

        #endregion

        private async Task ProcessarAcaoDB(EnumAcao acao, EnumSPASituacaoTransacao situacao)
        {
            using (var _activity = OtlpActivityService.GenerateActivitySource.StartActivity("#### PROCESSANDO AÇÃO ####", ActivityKind.Internal))
            {
                TransacaoAtiva!.ListParametros!.Find(item => item._oSQLParameter!.ParameterName == "@ptinAcao")!.Valor = acao;
                TransacaoAtiva.ListParametros.Find(item => item._oSQLParameter!.ParameterName == "@ptinEstado0")!.Valor = TransacaoAtiva.Situacao0;
                TransacaoAtiva.ListParametros.Find(item => item._oSQLParameter!.ParameterName == "@ptinEstado1")!.Valor = situacao;

                var _baseReturn = await _repoSPA.ExecutaDB(this.TransacaoAtiva);
            }
        }

        public string GetRetornoSPA()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_headerTransacaoAtiva.Span);
            stringBuilder.Append(((char)11));

            foreach (var item in this.TransacaoAtiva!.ListParametros!)
            {
                stringBuilder.Append(item.Valor);
                stringBuilder.Append(((char)11));
            }

            this.RetornoSPA = stringBuilder.ToString();

            return this.RetornoSPA;
        }

        public async Task<IEnumerable<RetornoSPX>> MontarChamadaSPX(ParametrosSPX parametros)
        {
            return TransacaoAtiva!.Codigo switch
            {
                740 => await _repoSPA.ExecutarSPXIdentificaCartao(parametros),
                741 => await _repoSPA.ExecutarSPXSenhaSilabica(parametros),
                _ => throw new InvalidOperationException($"Código de transação inválido: {TransacaoAtiva.Codigo}")
            };
        }

        ~SPAOperadorService()
        {
            Disposing(false);
        }

        protected void Disposing(bool disposing)
        {
            this.DependenciaAtiva = null;
            this.OperadorAtivo = null;
            this.TransacaoAtiva = null;
            this.RetornoSPA = null;
        }
    }
}