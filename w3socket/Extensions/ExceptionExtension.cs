using W3Socket.Core.Models.Excpetions;
using System.Net.Sockets;
using System;

namespace W3Socket.Extensions
{
    public static class ExceptionExtension
    {
        private static string _message;

        public static string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public static W3SocketException handleException(Exception err, string ErrorMethod, int ErrorCode = -1)
        {
            SocketException sckErr;
            W3SocketException w3Err;

            //Socket Exception
            if (err.GetType().FullName == "SocketException")
            {
                sckErr = (SocketException)err;
                w3Err = handleSocketException(sckErr);
                w3Err.Source = ErrorMethod;
            }
            else
            {
                w3Err = new W3SocketException(ErrorCode, err.Message);
                w3Err.Source = ErrorMethod;
            }

            return w3Err;
        }

        private static W3SocketException handleSocketException(SocketException sckErr)
        {
            string _errorMessage = "";

            switch (sckErr.ErrorCode)
            {
                case 10050:
                case 10051:
                case 10052:
                case 10053:
                    _errorMessage = "Sem conexão com a rede ou conexão abortada.";
                    break;

                case 10054:
                    _errorMessage = "Servico cliente desconectado.";
                    break;

                case 10057:
                    _errorMessage = "Servico sem conexão.";
                    break;

                case 10060:
                    _errorMessage = "TEMPO DE ESPERA ESGOTADO (RESPOSTA).";
                    break;

                case 10061:
                    _errorMessage = "Conexão recusada.";
                    break;

                case 10064:
                case 10065:
                    _errorMessage = "Problemas no servidor remoto.";
                    break;

                case 11001:
                    _errorMessage = "Host não encontrado.";
                    break;
                default:
                    _errorMessage = "Erro de comunicação.";
                    break;
            }

            return new W3SocketException(sckErr.ErrorCode, _errorMessage);
        }
    }
}
