using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;


namespace Domain.Core.Models.Transactions
{
    public sealed record TransactionConsultaPapelDispAplic : BaseTransaction<BaseReturn<ResponsePapelDispAplic>>
    {

        public TransactionConsultaPapelDispAplic(): base()
        {
            SetTipoLista(EnumTipoLista.LISTA_PAP_DISP_APLIC);
        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Default);
        }
    }
}
