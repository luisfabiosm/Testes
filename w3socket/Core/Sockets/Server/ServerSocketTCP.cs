using W3Socket.Core.Models.SPA;
using W3Socket.Core.Interfaces;
using System.Threading.Tasks;
using W3Socket.Extensions;
using W3Socket.Core.Base;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System;

namespace W3Socket.Core.Sockets.Server
{
    public class ServerSocketTcp : BaseSocketTcp, IServerSocketTCP
    {
        #region Campos de atribuicao

        public Socket _server;
        protected Socket _handler;
        private readonly byte[] _buffer;
        private bool _listenning = false;

        #endregion

        #region Properties

        public IPAddress IP { get; internal set; }
        public int Port { get; internal set; }
        public int Timeout { get; internal set; }

        #endregion

        public ServerSocketTcp(IServiceProvider serviceProvider, int port, int sizebuffer = 1024) : base(serviceProvider, sizebuffer)
        {
            this.Port = port;
            this.IP = IPAddress.Parse("127.0.0.1");
            this._buffer = new byte[sizebuffer];

            StartServer();
        }

        public ServerSocketTcp(IServiceProvider serviceProvider, IPAddress ip, int port, int sizebuffer = 1024) : base(serviceProvider, sizebuffer)
        {

            this.Port = port;
            this.IP = ip;
            this._buffer = new byte[sizebuffer];

            StartServer();
        }

        #region Events

        public event Func<TcpServerBufferArgs, Task> OnDataArrival;

        #endregion

        public bool IsListenning()
        {
            return _listenning;
        }

        public Socket AcceptClientToResponse()
        {
            return this._handler;
        }

        public async Task StartListening()
        {
            try
            {
                while (!_listenning)
                {
                    StartServer();
                }

                _server.Listen();
                this._listenning = true;

                Console.WriteLine($"Server started at Port {this.Port}. Listening for incoming connections...");

                while (true)
                {
                    this._handler = await _server.AcceptAsync();
                    Console.WriteLine($"Client at Port {_handler.RemoteEndPoint}.");

                    // Handle the connection asynchronously
                    _ = HandleConnectionAsync(_handler);
                }
            }
            catch (SocketException ex)
            {
                this._listenning = false;
                Console.WriteLine("SocketException occurred while starting the server: " + ex.Message);
                throw ExceptionExtension.handleException(ex, "StartListening");

            }
            catch (Exception ex)
            {
                this._listenning = false;
                Console.WriteLine("Error occurred while starting the server: " + ex.Message);
                throw ExceptionExtension.handleException(ex, "StartListening");
            }
        }

        private void StartServer()
        {
            if (!_listenning)
            {
                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Bind(new IPEndPoint(IPAddress.Any, this.Port));
                this._listenning = true;
            }
        }

        private async Task HandleConnectionAsync(Socket handler)
        {
            try
            {
                TcpServerBufferArgs bufferArgs;

                while (true)
                {
                    int readBytes;
                    try
                    {
                        readBytes = await handler.ReceiveAsync(_buffer, SocketFlags.None, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Handle timeout
                        Console.WriteLine("Read timeout occurred. Closing connection.");
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        break;
                    }

                    if (readBytes > 0)
                    {
                        //Conversão
                        bufferArgs = new TcpServerBufferArgs(readBytes);
                        bufferArgs.Message = new StringBuilder().Append(Encoding.ASCII.GetString(_buffer, 0, readBytes)).ToString();
                        Array.Copy(_buffer, bufferArgs.BufferMessage, bufferArgs.Message.Length);
                        await OnDataArrival(bufferArgs);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException occurred while handling connection: {ex.Message} Stacktrace {ex.StackTrace}");
                throw ExceptionExtension.handleException(ex, "HandleConnectionAsync");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while handling connection: {ex.Message} Stacktrace {ex.StackTrace}");
                throw ExceptionExtension.handleException(ex, "HandleConnectionAsync");
            }
            finally
            {
                handler.Dispose();
            }
        }

        protected virtual async Task OnDataReceived(string message, Socket client, int receivedPort = 0)
        {
            if (OnDataArrival != null)
            {
                using (var args = new TcpServerBufferArgs(client, message, receivedPort))
                {
                    await OnDataArrival.Invoke(args);
                }
            }
        }

        protected virtual async Task OnDataReceived(byte[] msgBuf, Socket client, int receivedPort = 0)
        {
            if (OnDataArrival != null)
            {
                using (var args = new TcpServerBufferArgs(client, msgBuf, receivedPort))
                {
                    await OnDataArrival.Invoke(args);
                }
            }
        }

        public string GetServerIP()
        {
            return this._server.LocalEndPoint.ToString();
        }

        public int GetServerPort()
        {
            return this.Port;
        }
    }
}
