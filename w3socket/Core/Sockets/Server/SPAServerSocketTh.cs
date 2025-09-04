using Microsoft.Extensions.Logging;
using W3Socket.Core.Models.SPA;
using System.Diagnostics;
using W3Socket.Core.Base;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;

namespace W3Socket.Core.Sockets.Server
{
    public class SpaServerSocketTh : BaseServerSocketTCP
    {
        #region Campos de atribuicao

        private const int TAMANHO_CAB = 68;
        internal SpaMensagem _oSPAMensagem { get; set; }

        internal SpaTcpBufferArgs oSpaArgs;
        internal Mensagem _oMensagem { get; set; }

        int _countReq;

        #endregion

        #region Events

        public delegate void delReceberMensagem(SpaTcpBufferArgs args, Socket client);
        public event delReceberMensagem OnReceberMensagem;

        public delegate void delMensagemAberta(string msg);

        public delegate void delClient(Socket client);
        public event delClient OnConnectedClient;
        public event delClient OnDisconnectedClient;

        #endregion

        public SpaServerSocketTh(int port, ILogger<BaseServerSocketTCP> logger, delClient evConnectedClient, delClient evDisconnectedClient, delReceberMensagem evMessageReceived, delMensagemAberta evMessageAberta) : base(port, logger)
        {
            _oSPAMensagem = new SpaMensagem();
            _oMensagem = new Mensagem();
            oSpaArgs = new SpaTcpBufferArgs();
            _Activity = new ActivitySource(nameof(SpaServerSocketTh));

            if (evConnectedClient != null)
            {
                LogInformation($"OnConnectedClient", $"Evento Cliente conectado ativado");
                OnConnectedClient = evConnectedClient;
            }

            if (evDisconnectedClient != null)
            {
                LogInformation($"OnDisconnectedClient", $"Evento Cliente desconectado ativado");
                OnDisconnectedClient = evDisconnectedClient;
            }

            LogInformation($"OnMensagemAberta", $"Evento para Mensagem aberta ativado");
            LogInformation($"OnReceberMensagem", $"Evento para Mensagem recebida ativado");
        }

        #region Props

        public bool IsListenning()
        {
            return _listenning;
        }

        public string GetAddress()
        {
            return this.IP + ":" + this.Port;
        }

        public static string GetAddress(EndPoint endPoint)
        {
            IPEndPoint ipaddr = ((IPEndPoint)endPoint);
            return ipaddr.Address + ":" + ipaddr.Port;
        }

        public static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (Exception) 
            { 
                return false; 
            }
        }

        #endregion

        #region PUBLIC

        public void StartListening(int ReceiveBufferSize, int ReceiveTimeout, int SendBufferSize, int SendTimeout, short TTL)
        {
            try
            {
                if (_listenning == true)
                    return;

                _listenning = false;

                _tmrThreadClient = null;
                _tmrThreadClient = new System.Timers.Timer();
                _tmrThreadClient.Elapsed += (s, e) =>
                {
                    _tmrThreadClient.Stop();
                    CreateThreadClientSocket();
                };
                _tmrThreadClient.AutoReset = false;

                LogInformation($"StartListening", $"Iniciando Socket Server ....");
                IPEndPoint ipAddress = new IPEndPoint(this.IP, this.Port);

                this.SizeBuffer = ReceiveBufferSize;

                _server = new TcpListener(ipAddress);
                _server.Server.SendTimeout = SendTimeout;
                _server.Server.NoDelay = true;
                _server.Server.Blocking = false;
                _server.Start();

                _listenning = true;

                LogInformation("StartListening", $"Aguardando conexão no endereço {ipAddress}.");

                _tmrThreadClient.Start();
            }
            catch (Exception ex)
            {
                LogError($"[StartListening]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                _listenning = false;
            }
        }

        public void StopListening()
        {
            try
            {
                _listenning = false;

                if (_server != null)
                    _server.Stop();
            }
            catch (Exception ex)
            {
                LogError($"[StopListening]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
            LogInformation($"StopListening", $"Socket Server parado.");
        }

        #endregion

        #region PRIVATE

        void CreateThreadClientSocket()
        {
            try
            {
                while (_listenning && _server != null)
                {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();
                    _server.BeginAcceptSocket(new AsyncCallback(AcceptSocketCallBack), _server);

                    // Wait until a connection is made before continuing.  
                    _allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                LogError($"[CreateThreadClientSocket]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        void AcceptSocketCallBack(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.  
                _allDone.Set();

                Socket socket = _server.EndAcceptSocket(ar);

                if (socket != null)
                {
                    string endpoint = GetAddress(socket.RemoteEndPoint).ToString();

                    if (IsConnected(socket))
                    {
                        LogInformation("AcceptSocketCallBack", $"[{endpoint}] Cliente conectado");
                        _clients[endpoint] = socket;

                        OnConnectedClient?.Invoke(_clients[endpoint]);

                        #pragma warning disable
                        while (socket != null && IsConnected(socket))
                        {
                            Receive(socket);
                        }
                        #pragma warning restore
                    }

                    Socket c;
                    _clients.TryRemove(endpoint, out c);

                    if (OnDisconnectedClient != null && c != null)
                    {
                        OnDisconnectedClient(c);
                    }
                }

            }
            catch (Exception ex)
            {
                LogError($"[AcceptSocketCallBack]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        void Receive(Socket socket)
        {
            try
            {
                string endpoint = GetAddress(socket.RemoteEndPoint).ToString();
                byte[] data = new byte[this.SizeBuffer];
                int totalRecebido = socket.Receive(data);
                var sw = new Stopwatch();

                if (totalRecebido > 0)
                {
                    _countReq++;
                    sw.Start();

                    byte[] bufferRecebido = ExtrairBufferRecebido(data, totalRecebido);
                    oSpaArgs = new SpaTcpBufferArgs(oSpaArgs, bufferRecebido);

                    ProcessarMensagens(socket, totalRecebido);

                    sw.Stop();
                    LogInformation($"[Receive]", $"Tempo processamento para liberar nova Thread: {sw.ElapsedMilliseconds / 1000} segundos. ");
                    LogInformation($"[Receive]", $"Total: {_countReq} req recebidos. ");
                }
            }
            catch (SocketException)
            {
                // Log opcional
            }
            catch (Exception ex)
            {
                LogError($"[Receive]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
            finally
            {
                ResetarBufferSeNecessario();
            }
        }

        private byte[] ExtrairBufferRecebido(byte[] data, int totalRecebido)
        {
            byte[] buffer = new byte[totalRecebido];
            Buffer.BlockCopy(data, 0, buffer, 0, totalRecebido);
            return buffer;
        }

        private void ProcessarMensagens(Socket socket, int totalRecebido)
        {
            int totalProcessado = 0;

            while (totalProcessado < totalRecebido)
            {
                var argsExtraidos = ExtractMessage(oSpaArgs.ArgsLastBuffer, oSpaArgs.ArgsLastBuffer.Length);

                if (argsExtraidos.ArgsSpaValido)
                {
                    oSpaArgs = argsExtraidos;
                    IniciarThreadProcessamentoMensagem(argsExtraidos, socket);
                }

                int tamanhoSpaBuffer = (oSpaArgs.ArgsSpaBuffer is null)
                    ? oSpaArgs.ArgsLastBuffer.Length
                    : oSpaArgs.ArgsSpaBuffer.Length;

                totalProcessado += tamanhoSpaBuffer;
            }
        }

        private void IniciarThreadProcessamentoMensagem(SpaTcpBufferArgs args, Socket socket)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    OnReceberMensagem?.Invoke(args, socket);
                }
                catch (Exception ex)
                {
                    LogError("[Receive] OnReceberMensagem", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                }
            });

            thread.Name = "Thread Socket - " + thread.ManagedThreadId;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        private void ResetarBufferSeNecessario()
        {
            if (oSpaArgs.ArgsLastBuffer.Length == 0)
                oSpaArgs = new SpaTcpBufferArgs();
        }

        #endregion

        #region PROCESSAMENTO ALTERNATIVO TRATAMENTO MENSAGEM SPA

        private SpaTcpBufferArgs ExtractMessage(byte[] bytBuffer, long totalRecebido)
        {
            SpaTcpBufferArgs _spaArgs = new SpaTcpBufferArgs(bytBuffer);

            try
            {
                byte[] _cabecalhoBytes;
                if (totalRecebido < TAMANHO_CAB)
                    return _spaArgs;

                _cabecalhoBytes = new byte[TAMANHO_CAB];
                Buffer.BlockCopy(bytBuffer, 0, _cabecalhoBytes, 0, _cabecalhoBytes.Length);

                short tamanhoMsg = BitConverter.ToInt16(_cabecalhoBytes, 6);

                if (totalRecebido >= tamanhoMsg)
                {
                    byte[] _bytMensagem = new byte[tamanhoMsg];
                    Buffer.BlockCopy(bytBuffer, 0, _bytMensagem, 0, tamanhoMsg);

                    _spaArgs = new SpaTcpBufferArgs(bytBuffer, _bytMensagem);
                }

                return _spaArgs;

            }
            catch (Exception ex)
            {
                LogError($"[ExtractMessage]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");

            }

            return _spaArgs;
        }

        #endregion
    }
}