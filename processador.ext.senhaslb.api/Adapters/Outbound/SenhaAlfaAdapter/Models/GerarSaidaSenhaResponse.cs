using Domain.Core.Enums;

namespace Adapters.Outbound.SenhaAlfaAdapter.Models
{
    public class GerarSaidaSenhaResponse
    {
        public int returnCode { get; set; } // codRetorno
        public int dataHora { get; set; } //token
        public string? seq1 { get; set; }
        public string? seq2 { get; set; }
        public string? seq3 { get; set;}
        public string? grupo8 { get; set; } // grupo

        public GerarSaidaSenhaResponse(int token, string? seq1, string? seq2, string? seq3, string? grupo, int codRetorno)
        {
            this.dataHora = token;
            this.seq1 = seq1;
            this.seq2 = seq2;
            this.seq3 = seq3;
            this.grupo8 = grupo;
            this.returnCode = codRetorno;
        }

        public GerarSaidaSenhaResponse() { }
    }
}
