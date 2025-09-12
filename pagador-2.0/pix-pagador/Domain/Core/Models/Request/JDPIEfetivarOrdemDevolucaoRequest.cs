namespace Domain.Core.Models.Request
{
    public record JDPIEfetivarOrdemDevolucaoRequest
    {
        public string idReqSistemaCliente { get; set; }

        public string idReqJdPi { get; set; }

        public string endToEndIdOriginal { get; set; }

        public string endToEndIdDevolucao { get; set; }

        public string dtHrReqJdPi { get; set; }
    }
}
