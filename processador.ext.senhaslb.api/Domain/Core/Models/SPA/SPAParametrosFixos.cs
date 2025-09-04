namespace Domain.Core.Models.SPA
{
    public record SPAParametrosFixos
    {
        #region PARTE FIXA

        public string? Operador { get; set; }
        public string? Supervisor { get; set; }
        public string? Estacao { get; set; }
        public int Canal { get; set; }
        public int Transacao { get; set; }
        public string? TipoTransacao { get; set; }
        public string? DataContabil1 { get; set; } 
        public int Agencia1 { get; set; }
        public int Posto1 { get; set; }
        public int NSU1 { get; set; }
        public int NSUGrupo1 { get; set; }
        public string? DataContabil2 { get; set; }  
        public int Agencia2 { get; set; }
        public int Posto2 { get; set; }
        public string? Log { get; set; }
        public int Autenticacao { get; set; }
        public bool BitLocal { get; set; }
        public int Acao { get; set; }
        public int Estado0 { get; set; }
        public int Estado1 { get; set; }
        public int Replicacao { get; set; }
        public string? DataContabil { get; set; } 
        public int NSUUltimo { get; set; }
        public string? AreaSPA { get; init; }
        public string? AreaUsuario { get; set; }

        #endregion

        public SPAParametrosFixos() { }

        public SPAParametrosFixos(string[] dadosSeparados)
        {
            Operador = dadosSeparados[0];
            Supervisor = dadosSeparados[1];
            Estacao = dadosSeparados[2];
            Canal = int.Parse(dadosSeparados[3]);
            Transacao = int.Parse(dadosSeparados[4]);
            TipoTransacao = dadosSeparados[5];
            DataContabil1 = dadosSeparados[6];
            Agencia1 = int.Parse(dadosSeparados[7]);
            Posto1 = int.Parse(dadosSeparados[8]);
            NSU1 = int.Parse(dadosSeparados[9]);
            NSUGrupo1 = int.Parse(dadosSeparados[10]);
            DataContabil2 = dadosSeparados[11];
            Agencia2 = int.Parse(dadosSeparados[12]);
            Posto2 = int.Parse(dadosSeparados[13]);
            Log = dadosSeparados[14];
            Autenticacao = int.Parse(dadosSeparados[15]);
            BitLocal = dadosSeparados[16] == "0" ? false : true;
            Acao = int.Parse(dadosSeparados[17]);
            Estado0 = int.Parse(dadosSeparados[18]);
            Estado1 = int.Parse(dadosSeparados[19]);
            Replicacao = int.Parse(dadosSeparados[20]);
            DataContabil = dadosSeparados[21];
            NSUUltimo = int.Parse(dadosSeparados[22]);
            AreaUsuario = dadosSeparados[24];
        }
    }
}
