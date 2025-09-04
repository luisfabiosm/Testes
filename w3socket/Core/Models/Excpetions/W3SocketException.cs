using System;

namespace W3Socket.Core.Models.Excpetions
{
    /** Tipos de erro */
    public enum eErroTipoExterno
    {
        OK = 0,
        Negocio = 1,
        Interno = 2,
        BancoDados = 3,
        Comunicacao = 4,
    }

    public class W3SocketException : Exception
    {
        #region Props

        private int? tipoErro = 0;
        private long? numeroErro = 0;

        public eErroTipoExterno Tipo
        {
            get { return (eErroTipoExterno)tipoErro; }
            set { tipoErro = Convert.ToInt32(value); }
        }

        public long Numero
        {
            get { return numeroErro ?? 0; }
            set { numeroErro = value; }
        }

        public override string Source { get => base.Source; set => base.Source = base.Source + "[" + value + "]"; }

        #endregion

        #region Construtor

        public W3SocketException(long numero, string message)
        : base(message)
        {
            Numero = numero;
            Tipo = eErroTipoExterno.Negocio;
        }

        public W3SocketException(long numero, eErroTipoExterno tipo, string message, Exception inner)
            : base(message, inner)
        {
            Numero = numero;
            Tipo = tipo;
        }

        public W3SocketException(long numero, eErroTipoExterno tipo, string message)
        {
            Numero = numero;
            Tipo = tipo;

        }

        #endregion

        ~W3SocketException()
        {
            tipoErro = null;
            numeroErro = null;
        }
    }
}
