using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;


namespace Domain.Core.Models.Transactions
{
    public sealed record TransactionConsultaAplicacaoDia : BaseTransaction<BaseReturn<ResponseAplicacaoNoDia>>
    {

        public TransactionConsultaAplicacaoDia(): base()
        {
            SetTipoLista(EnumTipoLista.APLIC_NO_DIA);
        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Default);
        }
    }
}
