using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;


namespace Domain.Core.Ports.Outbound
{
    public interface ISPARepository 
    {
         ValueTask<string> ExecuteTransaction<TResponse>(BaseTransaction<BaseReturn<TResponse>> transaction)
            where TResponse : BaseTransactionResponse;

    }
}
