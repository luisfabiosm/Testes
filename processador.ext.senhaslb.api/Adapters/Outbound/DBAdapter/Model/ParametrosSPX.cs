namespace Adapters.Outbound.DBAdapter.Model
{
    public class ParametrosSPX
    {
        public string? Dados { get; set; }
        public string? Resposta { get; set; }
        public string? Seq1 { get; set; }
        public string? Seq2 { get; set; }
        public string? Seq3 { get; set; }
        public string? Grupo { get; set; }
        public int Token { get; set; }

        public string? PvchMsgIN { get; set; }
        public string? PvchMsgOUT { get; set; }
        public int PIntRetDLL { get; set; }

        public ParametrosSPX(string dados, string resposta, string seq1, string seq2, string seq3, string grupo, int token)
        {
            this.Dados = dados;
            this.Resposta = resposta;
            this.Seq1 = seq1;
            this.Seq2 = seq2;
            this.Seq3 = seq3;
            this.Grupo = grupo;
            this.Token = token;
        }

        public ParametrosSPX(string dados, string pcvhMsgIN, string pvchMsgOUT, int pintRetDLL)
        {
            this.Dados = dados;
            this.PvchMsgIN = pcvhMsgIN;
            this.PvchMsgOUT = pvchMsgOUT;
            this.PIntRetDLL = pintRetDLL;
        }
    }

    public class RetornoSPX
    {
        public string? vchParam { get; set; }
    }
}
