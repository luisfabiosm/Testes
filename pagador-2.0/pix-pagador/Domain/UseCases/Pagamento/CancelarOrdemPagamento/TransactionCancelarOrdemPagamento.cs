using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Pagamento.CancelarOrdemPagamento
{
    public sealed record TransactionCancelarOrdemPagamento : BaseTransaction<BaseReturn<JDPICancelarOrdemPagamentoResponse>>
    {


        public string idReqSistemaCliente { get; init; }
        public string agendamentoID { get; init; }
        public string motivo { get; init; }
        public EnumTipoErro tipoErro { get; init; }


        public TransactionCancelarOrdemPagamento()
        {

        }


        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Minimal);

        }
    }
}
