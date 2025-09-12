using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record JDPIRegistrarOrdemPagamentoResponse : BaseTransactionResponse
    {

        public double valorCheqEspUtilizado { get; set; }
        public string agendamentoID { get; set; }
        public string comprovante { get; set; }

        public JDPIRegistrarOrdemPagamentoResponse()
        {

        }

        public JDPIRegistrarOrdemPagamentoResponse(string result)
        {
            var _result = result.FromJsonOptimized<JDPIRegistrarOrdemPagamentoResponse>(JsonOptions.Default);
            if (_result == null) return;

            valorCheqEspUtilizado = _result.valorCheqEspUtilizado;
            agendamentoID = _result.agendamentoID;
            comprovante = _result.comprovante;
            chvAutorizador = _result.chvAutorizador;
        }
    }
}
