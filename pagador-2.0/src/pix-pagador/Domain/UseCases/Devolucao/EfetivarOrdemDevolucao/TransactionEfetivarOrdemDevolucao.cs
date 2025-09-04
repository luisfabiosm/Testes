using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Devolucao.EfetivarOrdemDevolucao
{
    public sealed record TransactionEfetivarOrdemDevolucao : BaseTransaction<BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>>
    {
        public string idReqSistemaCliente { get; init; }

        public string idReqJdPi { get; init; }

        public string endToEndIdOriginal { get; init; }

        public string endToEndIdDevolucao { get; init; }

        public string dtHrReqJdPi { get; init; }

        public TransactionEfetivarOrdemDevolucao()
        {

        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Minimal);

        }
    }
}
