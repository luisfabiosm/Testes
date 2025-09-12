using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;

namespace Domain.UseCases.Devolucao.RegistrarOrdemDevolucao
{
    public sealed record TransactionRegistrarOrdemDevolucao : BaseTransaction<BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>
    {
        public string idReqSistemaCliente { get; init; }

        public string endToEndIdOriginal { get; init; }

        public string endToEndIdDevolucao { get; init; }

        public string codigoDevolucao { get; init; }

        public string motivoDevolucao { get; init; }

        public double valorDevolucao { get; init; }

        public TransactionRegistrarOrdemDevolucao()
        {

        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Minimal);

        }
    }
}
