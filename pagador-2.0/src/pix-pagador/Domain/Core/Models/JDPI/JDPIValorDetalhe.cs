using Domain.Core.Enum;

namespace Domain.Core.Models.JDPI
{
    public record JDPIValorDetalhe
    {
        public decimal vlrTarifaDinheiroCompra { get; set; }


        public EnumTipoDetalhe tipo { get; set; }
    }
}
