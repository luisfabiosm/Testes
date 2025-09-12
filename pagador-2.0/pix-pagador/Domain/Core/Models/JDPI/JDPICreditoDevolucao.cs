namespace Domain.Core.Models.JDPI
{
    public record JDPICreditoDevolucao
    {
        public string endToEndIdOriginal { get; set; }
        public string endToEndIdDevolucao { get; set; }
        public string codigoDevolucao { get; set; }
        public string motivoDevolucao { get; set; }

    }
}
