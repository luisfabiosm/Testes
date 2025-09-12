using Domain.Core.Enum;
using Domain.Core.Models.JDPI;

namespace Domain.Core.Models.Request
{
    public record JDPIRegistrarOrdemPagtoRequest
    {

        public string idReqSistemaCliente { get; set; }
        public EnumTpIniciacao tpIniciacao { get; set; }
        public JDPIDadosConta pagador { get; set; }
        public JDPIDadosConta recebedor { get; set; }
        public double valor { get; set; }
        public string chave { get; set; }
        public string dtEnvioPag { get; set; }
        public string endToEndId { get; set; }
        public string idConciliacaoRecebedor { get; set; }
        public string infEntreClientes { get; set; }
        public string? cnpjIniciadorPagamento { get; set; }
        public string? consentId { get; set; }
        public EnumPrioridadePagamento? prioridadePagamento { get; set; }
        public EnumTpPrioridadePagamento? tpPrioridadePagamento { get; set; }
        public EnumTipoFinalidade? finalidade { get; set; }
        public EnumModalidadeAgente? modalidadeAgente { get; set; }
        public int? ispbPss { get; set; }
        public List<JDPIValorDetalhe>? vlrDetalhe { get; set; }
        public string qrCode { get; set; }
        public string agendamentoID { get; set; }

        public JDPIRegistrarOrdemPagtoRequest()
        {

        }

    }
}
