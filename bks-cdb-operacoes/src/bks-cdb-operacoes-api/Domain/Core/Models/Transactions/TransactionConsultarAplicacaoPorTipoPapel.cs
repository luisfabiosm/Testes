using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;

namespace Domain.Core.Models.Transactions;


public sealed record TransactionConsultarAplicacaoPorTipoPapel : BaseTransaction<BaseReturn<ResponseAplicacaoPorTipoPapel>>
{
   

    public TransactionConsultarAplicacaoPorTipoPapel() : base()
    {
        SetTipoLista(EnumTipoLista.LIST_APLIC_DO_CLIP_POR_TIP_PAP);
    }

    public override string getTransactionSerialization()
    {
        return this.ToJsonOptimized(JsonOptions.Default);
    }
}
