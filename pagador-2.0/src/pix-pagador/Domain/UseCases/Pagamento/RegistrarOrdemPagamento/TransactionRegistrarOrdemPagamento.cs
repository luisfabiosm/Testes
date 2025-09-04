using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Pagamento.RegistrarOrdemPagamento
{
    public sealed record TransactionRegistrarOrdemPagamento : BaseTransaction<BaseReturn<JDPIRegistrarOrdemPagamentoResponse>>
    {
        public string idReqSistemaCliente { get; init; }
        public EnumTpIniciacao tpIniciacao { get; init; }
        public JDPIDadosConta pagador { get; init; }
        public JDPIDadosConta recebedor { get; init; }
        public double valor { get; init; }
        public string chave { get; init; }
        public string dtEnvioPag { get; init; }
        public string endToEndId { get; init; }
        public string idConciliacaoRecebedor { get; init; }
        public string infEntreClientes { get; init; }
        public string? cnpjIniciadorPagamento { get; init; }
        public string? consentId { get; init; }



        public EnumPrioridadePagamento? prioridadePagamento { get; init; }



        public EnumTpPrioridadePagamento? tpPrioridadePagamento { get; init; }


        public EnumTipoFinalidade? finalidade { get; init; }


        public EnumModalidadeAgente? modalidadeAgente { get; init; }

        public int? ispbPss { get; init; }
        public List<JDPIValorDetalhe>? vlrDetalhe { get; init; }
        public string qrCode { get; init; }
        public string agendamentoID { get; init; }



        public TransactionRegistrarOrdemPagamento()
        {

        }

        public override string getTransactionSerialization()
        {


            return this.ToJsonOptimized(JsonOptions.Default);

        }
    }
}
