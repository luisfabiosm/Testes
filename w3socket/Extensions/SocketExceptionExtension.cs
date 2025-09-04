using System;
using System.Net.Sockets;

namespace W3Socket.Extensions
{
    public static class SocketExceptionExtension
    {

        private static string _message;

        public static string Message
        {
            get { return _message; }
            set { _message = value; }
        }



        public static Exception handleSocketException(this SocketException err)
        {

            Exception sckErr;

            switch (err.ErrorCode)
            {
                case 10050:
                case 10051:
                case 10052:
                case 10053:
                    sckErr = new Exception("Sem conexão com a rede ou conexão abortada.");
                    break;

                case 10054:
                    sckErr = new Exception("Servico cliente desconectado.");
                    break;

                case 10057:
                    sckErr = new Exception("Servico sem conexão.");
                    break;

                case 10060:
                    sckErr = new Exception("TEMPO DE ESPERA ESGOTADO (RESPOSTA).");
                    break;

                case 10061:
                    sckErr = new Exception("Conexão recusada.");
                    break;

                case 10064:
                case 10065:
                    sckErr = new Exception("Problemas no servidor remoto.");
                    break;

                case 11001:
                    sckErr = new Exception("Host não encontrado.");
                    break;

                case 910009:
                    sckErr = new Exception("Falha no tratamento da mensagem recebida.");
                    break;

                case 910010:
                    sckErr = new Exception("Falha no envio da mensagem.");
                    break;

                case 910011:
                    sckErr = new Exception("Falha na conexão.");
                    break;

                case 910012:
                    sckErr = new Exception("Falha na conexão.");
                    break;
                default:
                    sckErr = new Exception(err.Message);
                    break;
            }

            return sckErr;
        }

    }


}
