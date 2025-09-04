namespace Adapters.Outbound.SenhaAlfaAdapter.Models
{
    public class SenhaAlfaRequest
    {
        public int tipoSaque { get; set; }
        public string? agencia { get; set; }
        public string? conta { get; set; }
        public int dataHora { get; set; }
        public string? senhaBase { get; set; }
        public string? seqBotoes { get; set; }

        public SenhaAlfaRequest(int tipoSaque, string? agencia, string? conta)
        {
            this.tipoSaque = tipoSaque;
            this.agencia = agencia;
            this.conta = conta;
        }

        public SenhaAlfaRequest(int tipoSaque, string? agencia, string? conta, int dataHora, string? senhaBase, string? seqBotoes)
        {
            this.tipoSaque = tipoSaque;
            this.agencia = agencia;
            this.conta = conta;
            this.dataHora = dataHora;
            this.senhaBase = senhaBase;
            this.seqBotoes = seqBotoes;
        }
    }
}
