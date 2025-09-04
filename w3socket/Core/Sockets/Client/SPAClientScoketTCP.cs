using Microsoft.Extensions.Logging;
using W3Socket.Core.Interfaces;
using W3Socket.Core.Models.SPA;
using System.Threading.Tasks;
using W3Socket.Extensions;
using System.Diagnostics;
using System.Net.Sockets;
using W3Socket.Core.Base;
using System.Threading;
using System.IO;
using System;

namespace W3Socket.Core.Sockets.Client
{
    public sealed class SpaClientScoketTcp : BaseClientSocketTCP, ISPAClientSocketTCP, IDisposable
    {
        #region Campos de atribuicao

        private TcpClient _tcpClient;
        private readonly SemaphoreSlim messageProcessingSemaphore;

        #endregion

        #region Events

        public delegate void DelReceberMensagem(SpaTcpBufferArgs args);
        public event DelReceberMensagem OnDataArrival;
        public delegate void DelClient(Socket server);

        #endregion

        public SpaClientScoketTcp(string remoteIP, int port, ILogger<SpaClientScoketTcp> logger, DelClient evConnected, DelClient evDisconnected,
                                  DelReceberMensagem evMessageReceived, int threads = 5) : base(remoteIP, port, logger)
        {
            messageProcessingSemaphore = new SemaphoreSlim(threads);

            _Activity = new ActivitySource(nameof(SpaClientScoketTcp));

            if (evConnected != null)
            {
                LogInformation($"OnConnected", $"Evento Cliente conectado ativado");
            }

            if (evDisconnected != null)
            {
                LogInformation($"OnDisconnected", $"Evento Cliente desconectado ativado");
            }

            LogInformation($"OnDataArrival", $"Evento para Mensagem recebida ativado");
            OnDataArrival = evMessageReceived;

            Task.Run(ReceiveMessagesAsync);
        }

        event Interfaces.DelReceberMensagem ISPAClientSocketTCP.OnDataArrival
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        #region PUBLIC

        public void ConnectHost(int ConnectTimeOut = 10, int SendTimeout = 10, int ReceiveBufferSize = 1024)
        {
            try
            {
                if (_tcpClient != null)
                {
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }

                _tcpClient = new TcpClient(RemoteIP, Port);

                SizeBuffer = ReceiveBufferSize;
                _tcpClient.ReceiveBufferSize = ReceiveBufferSize;
                _tcpClient.NoDelay = true;

                var result = _tcpClient.BeginConnect(RemoteIP, Port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(Timeout));

                if (!_tcpClient.Connected)
                    throw new IOException($"Falha na conexão {RemoteIP}:{Port}.");
            }
            catch (Exception ex)
            {
                LogError($"[ConnectHost]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                throw ExceptionExtension.handleException(ex, "ConnectHost");
            }
        }

        public bool IsConnected()
        {
            return _tcpClient.Connected;
        }

        public void SendAsyncData(byte[] bytMessage)
        {
            try
            {
                if (!_tcpClient.Connected)
                    throw new IOException($"Falha na conexão {RemoteIP}:{Port}.");

                _tcpClient.Client.Send(bytMessage, 0, bytMessage.Length, SocketFlags.None);
            }

            catch (Exception ex)
            {
                LogError($"[SendAsyncData]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                throw ExceptionExtension.handleException(ex, "SendAsyncData");
            }
        }

        #endregion

        #region PRIVATE 

        private void getMessage(SpaTcpBufferArgs args)
        {
            OnDataArrival?.Invoke(args);
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                var bufferArgs = new byte[_tcpClient.ReceiveBufferSize];

                using (var netStream = _tcpClient.GetStream())
                {
                    while (true)
                    {
                        var bytesRead = await netStream.ReadAsync(bufferArgs, 0, bufferArgs.Length);
                        if (bytesRead == 0) return; // Stream was closed

                        var _oSpaArgs = new SpaTcpBufferArgs(bufferArgs);

                        //Raise event
                        getMessage(_oSpaArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"[ReceiveMessagesAsync]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                throw ExceptionExtension.handleException(ex, "Event: OnClientDataArrival");
            }

        }

        public void Dispose()
        {
            _tcpClient?.Dispose();
            messageProcessingSemaphore?.Dispose();
        }

        #endregion
    }
}
