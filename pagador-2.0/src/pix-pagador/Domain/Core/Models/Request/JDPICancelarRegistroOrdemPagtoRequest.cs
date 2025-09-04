using Domain.Core.Enum;

namespace Domain.Core.Models.Request
{
    public record JDPICancelarRegistroOrdemPagtoRequest
    {
        public string idReqSistemaCliente { get; set; }
        public string agendamentoID { get; set; }
        public string motivo { get; set; }
        public EnumTipoErro tipoErro { get; set; }

        public JDPICancelarRegistroOrdemPagtoRequest()
        {

        }

    }
}
