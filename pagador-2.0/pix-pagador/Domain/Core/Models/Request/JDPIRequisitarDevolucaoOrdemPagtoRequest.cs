using Domain.Core.Models.JDPI;

namespace Domain.Core.Models.Request;


public record JDPIRequisitarDevolucaoOrdemPagtoRequest
{
    public string idReqSistemaCliente { get; set; }
    public string endToEndIdOriginal { get; set; }
    public string endToEndIdDevolucao { get; set; }
    public string codigoDevolucao { get; set; }
    public string motivoDevolucao { get; set; }
    public double valorDevolucao { get; set; }
    public JDPIDadosConta pagador { get; set; }
    public JDPIDadosConta recebedor { get; set; }
}
