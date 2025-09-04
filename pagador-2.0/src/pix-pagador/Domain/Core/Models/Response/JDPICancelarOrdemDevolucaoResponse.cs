using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record JDPICancelarOrdemDevolucaoResponse : BaseTransactionResponse
    {
        public JDPICancelarOrdemDevolucaoResponse()
        {

        }

        public JDPICancelarOrdemDevolucaoResponse(string result)
        {
            var _result = result.FromJsonOptimized<JDPICancelarOrdemDevolucaoResponse>(JsonOptions.Default);
            if (_result == null) return;

            chvAutorizador = _result.chvAutorizador;

        }
    }
}
