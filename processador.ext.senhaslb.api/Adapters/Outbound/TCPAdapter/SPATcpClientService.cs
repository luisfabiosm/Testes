using Adapters.Outbound.TCPAdapter.Mapping;
using Domain.Core.Ports.Outbound;
using W3Socket.Core.Interfaces;
using W3Socket.Core.Models.SPA;

namespace Adapters.Outbound.TCPAdapter
{
    public class SPATcpClientService : ISPATcpClientServicePort
    {
        #region variáveis

        private readonly IClientSocketTCP _tcpClient;
        private readonly ILogger<SPATcpClientService> _loggerBase;

        #endregion

        public SPATcpClientService(IServiceProvider serviceProvider)
        {
            _tcpClient = serviceProvider.GetRequiredService<IClientSocketTCP>();
            _loggerBase = serviceProvider.GetRequiredService<ILogger<SPATcpClientService>>();

            if (!_tcpClient.IsConnected())
                ConnectHost();
        }

        public void ConnectHost()
        {
            _tcpClient.ConnectHost();
        }

        public string GetEndpoint()
        {
            return _tcpClient.GetEndpoint();
        }

        #pragma warning disable CS1998
        public async Task SendResponse(tSPACabecalho msgCab, byte[] bufMessage, int timeout = 30)
        {
            try
            {
                if (!_tcpClient.IsConnected())
                    ConnectHost();

                int totalMessageLength = msgCab.tamanhoCab + bufMessage.Length;
                var _bufResponseMessage = new byte[totalMessageLength];
                var _msgCab = MappingSPAMensagem.MappingCabecalhoDestino(msgCab, (short)(totalMessageLength));

                byte[] arrayCab = MappingSPAMensagem.copiaStructToBytes(_msgCab);
                Buffer.BlockCopy(arrayCab, 0, _bufResponseMessage, 0, msgCab.tamanhoCab);
                Buffer.BlockCopy(bufMessage, 0, _bufResponseMessage, msgCab.tamanhoCab, bufMessage.Length);

                _tcpClient.SendAsyncData(_bufResponseMessage, timeout, totalMessageLength);
                _loggerBase.LogInformation($"[{Thread.CurrentThread.ManagedThreadId.ToString("D6")}] [{DateTime.Now}] Mensagem respondida para {_tcpClient.GetEndpoint()}");
            }
            catch (Exception ex)
            {
                _loggerBase.LogError($"[SendResponse] Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                Console.WriteLine($"[SendResponse] Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }
        #pragma warning restore CS1998
    }
}
