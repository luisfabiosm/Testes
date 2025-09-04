using Domain.Core.Enums;

namespace Domain.Core.Models.SPA
{
    public record MAgentMensagem
    {
        /* LAYOUT MENSAGEM SPA
       * TIPO:        CHAR(04)  "TRAN"
       * CODIGO       CHAR(08)  com zeros a esquerda
       * METODO AÇÃO: CHAR(01)  
       * SIT. REMOTA  CHAR(01)
       * SEP. CAMPOS  CHAR(01)  vertical tab "|"
       * DADOS SPA    VARIANTE  montado pelo unbind
       * */

        public int Transacao { get; }
        public string[] DadosSPA { get; }
        public EnumSPASituacaoTransacao Situacao0 { get; }
        public EnumMetodoAcao MetodoAcao { get; }

        public MAgentMensagem(ReadOnlySpan<char> mensagem)
        {
            int separatorIndex = mensagem.IndexOf((char)11);

            if (separatorIndex < 0 || separatorIndex < 14)
                throw new InvalidOperationException($"Mensagem corrompida: {mensagem}");

            ReadOnlySpan<char> header = mensagem.Slice(0, separatorIndex).Trim('\0');
            if (!int.TryParse(header.Slice(4, 8), out int transacao) ||
                !int.TryParse(header.Slice(13, 1), out int situacao) ||
                !int.TryParse(header.Slice(12, 1), out int metodoAcao))
            {
                throw new InvalidOperationException("Falha ao converter dados da mensagem.");
            }

            Transacao = transacao;
            Situacao0 = (EnumSPASituacaoTransacao)situacao;
            MetodoAcao = (EnumMetodoAcao)metodoAcao;

            string[] splitMensagem = mensagem.ToString().Split((char)11);
            DadosSPA = new string[splitMensagem.Length - 1];
            Array.Copy(splitMensagem, 1, DadosSPA, 0, DadosSPA.Length);

            // Área SPA
            DadosSPA[23] = string.Empty;
        }
    }
}