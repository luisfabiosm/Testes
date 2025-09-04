using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record JDPICancelarOrdemPagamentoResponse : BaseTransactionResponse
    {
        public JDPICancelarOrdemPagamentoResponse()
        {

        }

        public JDPICancelarOrdemPagamentoResponse(string result)
        {
            var _result = result.FromJsonOptimized<JDPICancelarOrdemPagamentoResponse>(JsonOptions.Default);
            if (_result == null) return;

            chvAutorizador = _result.chvAutorizador;

        }
    }
}
