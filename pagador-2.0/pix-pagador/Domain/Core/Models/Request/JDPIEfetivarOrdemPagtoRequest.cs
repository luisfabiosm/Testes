namespace Domain.Core.Models.Request
{
    public record JDPIEfetivarOrdemPagtoRequest
    {
        public string idReqSistemaCliente { get; set; }

        public string idReqJdPi { get; set; }

        public string endToEndId { get; set; }

        public string dtHrReqJdPi { get; set; }

        public string agendamentoID { get; set; }

        public JDPIEfetivarOrdemPagtoRequest()
        {

        }
    }
}
