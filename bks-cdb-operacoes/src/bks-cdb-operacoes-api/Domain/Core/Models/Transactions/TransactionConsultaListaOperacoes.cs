using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;

namespace Domain.Core.Models.Transactions
{
    public sealed record TransactionConsultaListaOperacoes : BaseTransaction<BaseReturn<ResponseTipoOperacao>>
    {

        public TransactionConsultaListaOperacoes() : base()
        {
            SetTipoLista(EnumTipoLista.LISTA_TIP_OPE_CLI);
        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Default);
        }
    }
}
