using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using W3Socket.Core.Models.SPA;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;
using W3Socket.Core.Base;
using System.Threading;
using System.Net;
using System;

namespace W3Socket.Core.Sockets.Server
{
    public sealed class SpaServerSocketTk : BaseServerSocketTCP, IDisposable
    {
        #region Campos de atribuicao

        private const int TAMANHO_CAB = 68;
        internal SpaMensagem _oSPAMensagem { get; set; }
        internal Mensagem _oMensagem { get; set; }

        internal SpaTcpBufferArgs oSpaArgs;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly BufferBlock<byte[]> messageBufferBlock = new BufferBlock<byte[]>();
        private readonly SemaphoreSlim messageProcessingSemaphore;

        #endregion

        #region Events

        public delegate void delReceberMensagem(SpaTcpBufferArgs args, Socket client);
        public event delReceberMensagem OnReceberMensagem;

        public delegate void delMensagemAberta(string msg);
        public delegate void delClient(Socket client);
        public event delClient OnConnectedClient;
        public event delClient OnDisconnectedClient;

        #endregion

        #region Properties

        public bool IsListenning() => _listenning;
        public string GetAddress() => this.IP + ":" + this.Port;
        public static string GetAddress(EndPoint endPoint) => ((IPEndPoint)endPoint).Address + ":" + ((IPEndPoint)endPoint).Port;
        public static bool IsConnected(Socket socket)
        {
            try { return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0); }
            catch { return false; }
        }

        #endregion

        public SpaServerSocketTk(int port, ILogger<BaseServerSocketTCP> logger, 
            delClient evConnectedClient, delClient evDisconnectedClient, delReceberMensagem evMessageReceived, 
            delMensagemAberta evMessageAberta, int threads = 10) : base(port, logger)
        {
            _oSPAMensagem = new SpaMensagem();
            _oMensagem = new Mensagem();
            oSpaArgs = new SpaTcpBufferArgs();

            messageProcessingSemaphore = new SemaphoreSlim(threads);
            _Activity = new ActivitySource(nameof(SpaServerSocketTk));

            ConfigureEvents(evConnectedClient, evDisconnectedClient, evMessageReceived);
            Task.Run(() => ProcessMessagesAsync(_cts.Token));
        }

        private void ConfigureEvents(delClient evConnectedClient, delClient evDisconnectedClient, delReceberMensagem evMessageReceived)
        {
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

            OnReceberMensagem = evMessageReceived;
        }

        #region PUBLIC

        public void StartListening(int receiveBufferSize, int receiveTimeout, int sendBufferSize, int sendTimeout, short ttl)
        {
            if (_listenning) return;

            try
            {
                _listenning = false;

                _tmrThreadClient = new System.Timers.Timer { AutoReset = false };
                _tmrThreadClient.Elapsed += (s, e) => {
                    _tmrThreadClient.Stop();
                    CreateThreadClientSocket();
                };

                LogInformation($"StartListening", $"Iniciando Socket Server ....");
                IPEndPoint ipAddress = new IPEndPoint(this.IP, this.Port);
                this.SizeBuffer = receiveBufferSize;

                _server = new TcpListener(ipAddress)
                {
                    Server =
                    {
                        SendTimeout = sendTimeout,
                        NoDelay = true,
                        Blocking = false
                    }
                };

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
                _server?.Stop();
                LogInformation($"StopListening", $"Socket Server parado.");
            }
            catch (Exception ex)
            {
                LogError($"[StopListening]", $" Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        #endregion

        #region PRIVATE

        void AcceptSocketCallBack(IAsyncResult ar)
        {
            _allDone.Set();

            try
            {
                Socket socket = _server.EndAcceptSocket(ar);
                if (socket == null) return;

                string endpoint = GetAddress(socket.RemoteEndPoint);
                if (IsConnected(socket))
                {
                    LogInformation("AcceptSocketCallBack", $"[{endpoint}] Cliente conectado");
                    _clients[endpoint] = socket;
                    OnConnectedClient?.Invoke(socket);

                    #pragma warning disable
                    while (socket != null && IsConnected(socket))
                        Receive(socket);
                    #pragma warning restore
                }

                if (_clients.TryRemove(endpoint, out Socket removed))
                {
                    LogInformation("AcceptSocketCallBack", $"[{endpoint}] Cliente desconectado");
                    OnDisconnectedClient?.Invoke(removed);
                }
            }
            catch (Exception ex)
            {
                LogError($"[AcceptSocketCallBack]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        private void Receive(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[this.SizeBuffer];
                int received = socket.Receive(buffer);
                int processed = 0;

                if (received <= 0) return;
                var sw = Stopwatch.StartNew();

                byte[] actualData = new byte[received];
                Buffer.BlockCopy(buffer, 0, actualData, 0, received);
                oSpaArgs = new SpaTcpBufferArgs(oSpaArgs, actualData);

                while (processed < received)
                {
                    oSpaArgs = ExtractMessage(oSpaArgs.ArgsLastBuffer, oSpaArgs.ArgsLastBuffer.Length);
                    if (oSpaArgs.ArgsSpaValido)
                        messageBufferBlock.Post(oSpaArgs.ArgsSpaBuffer);

                    int chunkSize = oSpaArgs.ArgsSpaBuffer.Length == 0 ? oSpaArgs.ArgsLastBuffer.Length : oSpaArgs.ArgsSpaBuffer.Length;
                    processed += chunkSize;
                }

                sw.Stop();
                LogInformation("[Receive]", $"Tempo processamento para liberar nova Thread: {sw.ElapsedMilliseconds / 1000} segundos.");
            }
            catch (Exception ex)
            {
                LogError("[Receive]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }
        }

        void CreateThreadClientSocket()
        {
            try
            {
                while (_listenning && _server != null)
                {
                    _allDone.Reset();
                    _server.BeginAcceptSocket(AcceptSocketCallBack, _server);
                    _allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                LogError("[CreateThreadClientSocket]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                throw;
            }
        }

        private SpaTcpBufferArgs ExtractMessage(byte[] buffer, long total)
        {
            SpaTcpBufferArgs spaArgs = new SpaTcpBufferArgs(buffer);

            try
            {
                if (total < TAMANHO_CAB) return spaArgs;

                byte[] header = new byte[TAMANHO_CAB];
                Buffer.BlockCopy(buffer, 0, header, 0, header.Length);
                short messageSize = BitConverter.ToInt16(header, 6);

                if (total >= messageSize)
                {
                    byte[] message = new byte[messageSize];
                    Buffer.BlockCopy(buffer, 0, message, 0, messageSize);
                    spaArgs = new SpaTcpBufferArgs(buffer, message);
                }
            }
            catch (Exception ex)
            {
                LogError("[ExtractMessage]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
            }

            return spaArgs;
        }

        private async Task ProcessMessagesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    byte[] message = await messageBufferBlock.ReceiveAsync(token);
                    await messageProcessingSemaphore.WaitAsync(token);
                    try { await ProcessMessageAsync(message); }
                    finally { messageProcessingSemaphore.Release(); }
                }
                catch (OperationCanceledException)
                {
                    LogInformation("Cancelamento solicitado.", "Ocorreu um problema.");
                    break;
                }
                catch (Exception ex)
                {
                    LogError("[ProcessMessagesAsync]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                }
            }
        }

        private async Task ProcessMessageAsync(byte[] message)
        {
            await Task.Run(() =>
            {
                try
                {
                    var args = new SpaTcpBufferArgs(true, message);
                    OnReceberMensagem?.Invoke(args, null);
                }
                catch (Exception ex)
                {
                    LogError("[ProcessMessageAsync-OnReceberMensagem]", $"Erro: {ex.Message} Stacktrace {ex.StackTrace}");
                }
            });
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            LogInformation("SPAServerSocketTk", "Servidor finalizado com sucesso.");
        }

        #endregion
    }
}
