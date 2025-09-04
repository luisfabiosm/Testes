using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Pagamento.EfetivarOrdemPagamento
{
    public sealed record TransactionEfetivarOrdemPagamento : BaseTransaction<BaseReturn<JDPIEfetivarOrdemPagamentoResponse>>
    {

        public string idReqSistemaCliente { get; init; }

        public string idReqJdPi { get; init; }

        public string endToEndId { get; init; }

        public string dtHrReqJdPi { get; init; }

        public string agendamentoID { get; init; }


        public TransactionEfetivarOrdemPagamento()
        {

        }
        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Minimal);

        }
    }
}
