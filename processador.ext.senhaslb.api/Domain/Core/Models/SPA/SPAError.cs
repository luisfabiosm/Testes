using System.Data.SqlClient;
using Domain.Core.Enums;

namespace Domain.Core.Models.SPA
{
    public struct SPAError
    {
        public string Mensagem { get; set; }
        public int? Codigo { get; set; }
        public EnumSPATipoErroInterno Tipo { get; set; }

        public SPAError(string error)
        {
            Mensagem = error;
            Codigo = 50000;
            Tipo = EnumSPATipoErroInterno.InternoSQL;
        }

        public SPAError(Exception ex)
        {
            Mensagem = ex.Message;
            Codigo = 50000;
            Tipo = EnumSPATipoErroInterno.InternoSQL;
        }

        public SPAError(SqlException ex)
        {
            Mensagem = ex.Message;

            if (ex.Number >= 50000 && ex.Number <= 69999)
            {
                Codigo = ex.Number;
                Tipo = EnumSPATipoErroInterno.Negocio;
            }
            else if (ex.ErrorCode == -2146232060)
            {
                Codigo = -2146232060;
                Tipo = EnumSPATipoErroInterno.InternoSQL;
            }
            else if (ex.ErrorCode == 50000)
            {
                Codigo = 50000;
                Tipo = EnumSPATipoErroInterno.InternoSQL;
            }
            else if (ex.Number == -2)
            {
                Mensagem = $"TEMPO EXCEDIDO (BANCO DE DADOS) {ex.Message}";
                Codigo = -2;
                Tipo = EnumSPATipoErroInterno.BancoDados;
            }
            else
            {
                Codigo = -999999;
                Tipo = EnumSPATipoErroInterno.Geral;
            }
        }
    }
}