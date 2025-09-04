using W3Socket.Core.Models.SPA;
using System.Text;

namespace Domain.Core.Models.SPA
{
    public record SPARetorno
    {
        public tSPACabecalho CabecalhoRetorno { get; internal set; }
        public byte[] ByteRetorno { get; internal set; }
        public string MensagemRetorno { get; internal set; }

        public SPARetorno(string mensagemRetorno, tSPACabecalho cabecalho )
        {
            this.CabecalhoRetorno = cabecalho;
            this.ByteRetorno = Encoding.GetEncoding("Windows-1252").GetBytes(mensagemRetorno);
            this.MensagemRetorno = mensagemRetorno;
        }
    }
}

