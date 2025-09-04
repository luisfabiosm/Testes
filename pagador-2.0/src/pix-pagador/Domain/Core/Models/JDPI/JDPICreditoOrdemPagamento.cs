namespace Domain.Core.Models.JDPI
{
    public record JDPICreditoOrdemPagamento
    {
        public string endToEndId { get; set; }

        public string idConciliacaoRecebedor { get; set; }

        public string chave { get; set; }

    }
}
