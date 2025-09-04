using W3Socket.Core.Models.SPA;
using W3Socket.Core.Interfaces;
using System.Threading.Tasks;
using W3Socket.Core.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using W3Socket.Core.Base;
using System.Threading;
using System.Net;
using System.IO;
using System;

namespace W3Socket.Core.Sockets.Client
{
    public class ClientSocketTcp : BaseSocketTcp, IClientSocketTCP
    {
        private TcpClient _client;
        private byte[] _sendBuffer;

        #region Properties

        public IPAddress RemoteIPAddress { get; internal set; }
        public string RemoteIP { get; internal set; }
        public int RemotePort { get; internal set; }
        public int Timeout { get; internal set; }

        public void SetTimeout(int timeout)
        {
            this.Timeout = timeout;
        }

        public void SetRemoteIP(string host)
        {
            this.RemoteIPAddress = IPAddress.Parse(host);
        }

        public void SetRemotPort(int port)
        {
            this.RemotePort = port;
        }

        public int GetClientPort(int port)
        {
            return ((IPEndPoint)this._client.Client.LocalEndPoint).Port;
        }

        public string GetRemoteIP()
        {
            return ((IPEndPoint)this._client.Client.RemoteEndPoint).Address.ToString();
        }

        public bool IsConnected()
        {
            return _client.Connected;
        }

        public void Inicialize()
        {
            ConnectHost();
        }

        #endregion

        public ClientSocketTcp(IServiceProvider serviceProvider, IPAddress ip, int port, int timeoutConnect = 60, int sizebuffer = 1024) : base(serviceProvider, sizebuffer)
        {
            _Activity = new ActivitySource(nameof(ClientSocketTcp));

            this.RemotePort = port;
            this.RemoteIPAddress = ip;
            this.RemoteIP = this.RemoteIPAddress.ToString();
            this.Timeout = timeoutConnect;

            _client = new TcpClient();
        }

        public ClientSocketTcp(IServiceProvider serviceProvider, string hostIp, int port, int timeoutConnect = 60, int sizebuffer = 1024) : base(serviceProvider, sizebuffer)
        {
            _Activity = new ActivitySource(nameof(ClientSocketTcp));

            this.RemotePort = port;
            this.RemoteIPAddress = IPAddress.Parse(hostIp);
            this.RemoteIP = hostIp;
            this.Timeout = timeoutConnect;

            _client = new TcpClient();
        }

        #region Events

        public event EventHandler<SpaTcpBufferArgs> OnDataArrival;

        #endregion

        #region Interface functions

        public string GetEndpoint()
        {
            return $"{this.RemoteIPAddress.ToString()}:{this.RemotePort.ToString()}";
        }

        public async Task ConnectHostAsync(int timeOut = 10)
        {
            try
            {
                Timeout = timeOut;

                if (_client?.Client == null)
                    RenewTCPClient();
                else
                    await _client.ConnectAsync(new IPEndPoint(RemoteIPAddress, RemotePort));

                if (!_client.Connected)
                    throw new IOException($"Falha na conexão {RemoteIP}:{RemotePort}.");
                else
                {
                    await ReceiveAsyncMessage();
                }
            }
            catch (Exception ex)
            {
                LogError($"ConnectHost", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        #pragma warning disable CS4014
        public virtual void ConnectHost(int timeOut = 30)
        {
            try
            {
                Timeout = timeOut;

                if (_client?.Client == null)
                    RenewTCPClient();

                var result = _client.BeginConnect(RemoteIPAddress, RemotePort, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(Timeout));

                if (!_client.Connected)
                    throw new IOException($"Falha na conexão {RemoteIP}:{RemotePort}.");
                else
                {
                    ReceiveAsyncMessage();
                }

                ////we have connected
                _client.EndConnect(result);
                RenewTCPClient();
            }
            catch (Exception ex)
            {
                LogError($"ConnectHost", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                RenewTCPClient();
            }
        }
        #pragma warning restore CS4014

        public byte[] SendSyncData(byte[] bytMessage, int timeOut, int sizeBuffer)
        {
            try
            {
                sizeBuffer = 2048;

                SetSizeBuffer(sizeBuffer);
                _sendBuffer = new byte[SizeBuffer];

                _client.Client.SendTimeout = timeOut;
                _client.Client.Send(bytMessage, 0, SizeBuffer, SocketFlags.None);

                _sendBuffer = ReceiveSyncMessage(timeOut);
                return _sendBuffer;
            }
            catch
            {
                throw;
            }
        }

        public void SendAsyncData(byte[] bytMessage, int timeOut, int sizeBuffer)
        {
            try
            {
                try
                {
                    if (!_client.Connected)
                        ConnectHost();
                }
                catch
                {
                    throw new IOException($"Falha na conexão {RemoteIP}:{RemotePort}.");
                }

                SetSizeBuffer(bytMessage.Length);
                _sendBuffer = new byte[SizeBuffer];

                _client.Client.SendTimeout = timeOut;
                _client.Client.ReceiveTimeout = timeOut;
                _client.Client.Send(bytMessage, 0, bytMessage.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                LogError($"SendAsyncData]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        public virtual void OnClientDataArrival(SpaTcpBufferArgs e)
        {
            try
            {
                BackgroundTask dataArrivalTask = new BackgroundTask(e, new DataProcessCallback(GetMessage));
                Thread threadDataArrival = new Thread(new ThreadStart(dataArrivalTask.ThreadProcessed));
                threadDataArrival.Name = threadDataArrival.ManagedThreadId.ToString();
                threadDataArrival.Start();

                threadDataArrival.Join();
            }
            catch
            {
                throw;
            }
        }

        private void GetMessage(SpaTcpBufferArgs e)
        {
            try
            {
                OnDataArrival?.Invoke(this, e);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Private functions

        internal void RenewTCPClient()
        {
            try
            {
                _client?.Dispose();
                _client = new TcpClient(RemoteIP, RemotePort);
                _client.NoDelay = true;

                var socket = _client.Client;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.NoDelay = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] ReceiveSyncMessage(int timeOut)
        {
            int totalRecebido = 0;
            byte[] bytMessage = new byte[2048];
            byte[] bytBuffer = new byte[2048];
            bool bolMensagemCompleta = false;
            byte[] bytResposta = null;

            DateTime datTimeoOut = DateTime.Now.AddSeconds(timeOut);
            _client.Client.SendTimeout = timeOut;

            do
            {
                if (DateTime.Compare(DateTime.Now, datTimeoOut) > 0)
                {
                    SocketException exSck1 = new SocketException(10060);
                    throw exSck1;
                }
                try
                {
                    totalRecebido = _client.Client.Receive(bytMessage);
                    Array.Resize(ref bytBuffer, totalRecebido);
                    Array.Copy(bytMessage, 0, bytBuffer, 0, totalRecebido);
                }
                catch (SocketException exSck)
                {
                    if (exSck.SocketErrorCode == SocketError.WouldBlock ||
                        exSck.SocketErrorCode == SocketError.IOPending ||
                        exSck.SocketErrorCode == SocketError.NoBufferSpaceAvailable ||
                        exSck.SocketErrorCode == SocketError.TimedOut)
                    {
                        Thread.Sleep(30);
                    }
                    else
                    {
                        throw;
                    }
                }

            } while (!bolMensagemCompleta);

            bytResposta = bytBuffer;
            return bytResposta;
        }

        private async Task ReceiveAsyncMessage()
        {
            try
            {
                byte[] buffer = new byte[4096];
                SpaTcpBufferArgs bufferArgs;

                using (var netStream = _client.GetStream())
                {
                    while ((await ReadNetStreamAsync(netStream, buffer, cts.Token)) > 0)
                    {
                        //Conversão
                        bufferArgs = new SpaTcpBufferArgs(buffer);
                        OnClientDataArrival(bufferArgs);
                    }
                }
            }
            catch (Exception) 
            {
                Console.WriteLine("Problema encontrado!");
            }
        }

        private static async Task<int> ReadNetStreamAsync(NetworkStream netStream, byte[] buffer, CancellationToken token)
        {
            try
            {
                return await netStream.ReadAsync(buffer, token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
