 using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;

namespace Domain.Core.Models.Transactions
{
    public sealed record  TransactionConsultaCarteiraAplicacao : BaseTransaction<BaseReturn<ResponseCarteira>>
    {

        public TransactionConsultaCarteiraAplicacao() : base()
        {
            SetTipoLista(EnumTipoLista.LISTA_CART_APLI_CLI);
        }

        public override string getTransactionSerialization()
        {
            return this.ToJsonOptimized(JsonOptions.Default);
        }
    }
}
