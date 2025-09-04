using Domain.Core.Enums;
using System.Data;

namespace Domain.Core.Models.SPA
{
    public record SPATransacao
    {
        public long Codigo { get; set; }
        public string? Descricao { get; set; }
        public string? ProcedureSQL { get; set; }
        public DateTime DataContabil { get; set; }
        public int Timeout { get; set; }
        public DateTime? FormatoDatas { get; set; }
        public string? FormatoValores { get; set; }
        public int NumeroParametros { get; set; }

        public SPAParametrosFixos? ParametrosFixos;
        public List<SPAParametro>? ListParametros { get; set; }
        public bool TransacaoGravaLog { get; set; }
        public EnumSPASituacaoTransacao Situacao0 { get; set; }
        public EnumSPASituacaoTransacao Situacao1 { get; set; }
        public int Cracha { get; set; }
        public bool IsGarbage { get; set; }

        public SPATransacao()
        {
            this.Codigo = 0;
            this.Descricao = "";
            this.ProcedureSQL = null;
            this.DataContabil = new DateTime(1900, 1, 1);
            this.FormatoDatas = new DateTime(1900, 2, 1);
            this.FormatoValores = "0.00";
            this.NumeroParametros = 0;
            this.TransacaoGravaLog = false;
            this.ParametrosFixos = new SPAParametrosFixos();
            this.ListParametros = new List<SPAParametro>();
            this.Situacao0 = EnumSPASituacaoTransacao.ErroConfiguracao;
            this.Situacao1 = EnumSPASituacaoTransacao.ErroConfiguracao;
            this.IsGarbage = false;
            this.Cracha = 0;
        }

        public void ConfigSPATransacao(int codigo, string[] dadosTransacao)
        {
            this.NumeroParametros = dadosTransacao.Length;
            this.ParametrosFixos = new SPAParametrosFixos(dadosTransacao);
            this.Situacao0 = (EnumSPASituacaoTransacao)Convert.ToInt16(dadosTransacao[18]);
            this.Situacao1 = (EnumSPASituacaoTransacao)Convert.ToInt16(dadosTransacao[19]);

            foreach (var item in ListParametros!)
            {
                if (item.Indice >= 0 && item.Indice < dadosTransacao.Length)
                {
                    item.Valor = dadosTransacao[item.Indice];
                }
            }
        }

        public string SPAExec()
        {
            var _ordenado = this.ListParametros!.OrderBy(x => x.Indice).ToList();    
            return string.Join("|", _ordenado.Select(item => item.Valor));
        }
     
        public virtual string[] ParamsToLog()
        {
            var _spaexec = SPAExec();
            return SplitString(_spaexec, 255);
        }

        private string[] SplitString(string input, int chunkSize)
        {
            int numChunks = (int)Math.Ceiling((double)input.Length / chunkSize);
            string[] chunks = new string[numChunks];

            for (int i = 0; i < numChunks; i++)
            {
                int startIndex = i * chunkSize;
                int length = Math.Min(chunkSize, input.Length - startIndex);
                chunks[i] = input.Substring(startIndex, length);
            }

            return chunks;
        }

        ~SPATransacao()
        {
            this.ParametrosFixos = null;
            this.ListParametros!.Clear();
            this.ListParametros = null;
        }
    }
}
