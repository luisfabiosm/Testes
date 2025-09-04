using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Devolucao.CancelarOrdemDevolucao
{
    public sealed record TransactionCancelarOrdemDevolucao : BaseTransaction<BaseReturn<JDPICancelarOrdemDevolucaoResponse>>
    {

        public string idReqSistemaCliente { get; init; }

        public TransactionCancelarOrdemDevolucao()
        {

        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Minimal);
        }

    }
}
