using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record JDPIEfetivarOrdemPagamentoResponse : BaseTransactionResponse
    {

        public JDPIEfetivarOrdemPagamentoResponse()
        {

        }

        public JDPIEfetivarOrdemPagamentoResponse(string result)
        {

            var _result = result.FromJsonOptimized<JDPIEfetivarOrdemPagamentoResponse>(JsonOptions.Default);
            if (_result == null) return;

            chvAutorizador = _result.chvAutorizador;

        }
    }
}
