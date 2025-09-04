using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record JDPIEfetivarOrdemDevolucaoResponse : BaseTransactionResponse
    {

        public JDPIEfetivarOrdemDevolucaoResponse()
        {

        }


        public JDPIEfetivarOrdemDevolucaoResponse(string result)
        {
            var _result = result.FromJsonOptimized<JDPIEfetivarOrdemDevolucaoResponse>(JsonOptions.Default);
            if (_result == null) return;

            chvAutorizador = _result.chvAutorizador;

        }
    }
}
