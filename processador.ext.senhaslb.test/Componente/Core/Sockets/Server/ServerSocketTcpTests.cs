using Microsoft.Extensions.Logging;
using W3Socket.Core.Sockets.Server;
using System.Net.Sockets;
using W3Socket.Core.Base;
using System.Text;
using System.Net;
using Moq;

namespace Componente.Core.Sockets.Server
{
    public class ServerSocketTcpTests : IDisposable
    {
        private readonly ServerSocketTcp _server;
        private readonly int _port;
        private TcpClient? _client;

        public ServerSocketTcpTests()
        {
            var loggerMock = new Mock<ILogger<BaseSocketTcp>>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock
                .Setup(p => p.GetService(typeof(ILogger<BaseSocketTcp>)))
                .Returns(loggerMock.Object);

            _server = new ServerSocketTcp(providerMock.Object, 0);
            _port = ((IPEndPoint)_server._server.LocalEndPoint!).Port;
        }

        [Fact]
        public void Deve_Inicializar_Socket_Com_IP_E_Porta_Corretos()
        {
            Assert.Equal(0, _server.Port);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), _server.IP);
            Assert.True(_server.IsListenning());

            var realPort = ((IPEndPoint)_server._server.LocalEndPoint!).Port;
            Assert.NotEqual(0, realPort);
        }

        [Fact]
        public void GetServerPort_DeveRetornarPorta()
        {
            Assert.Equal(0, _server.GetServerPort());

            var realPort = ((IPEndPoint)_server._server.LocalEndPoint!).Port;
            Assert.NotEqual(0, realPort);
        }

        [Fact]
        public void GetServerIP_DeveRetornarEndereco()
        {
            var ip = _server.GetServerIP();
            Assert.Contains(_port.ToString(), ip);
        }

        [Fact]
        public void AcceptClientToResponse_DeveRetornarHandler()
        {
            var socket = _server.AcceptClientToResponse();
            Assert.Null(socket);
        }

        [Fact]
        public async Task StartListening_DeveAceitarConexaoEReceberMensagem()
        {
            // Arrange
            string msg = "Ola Servidor";
            var receivedMessage = new TaskCompletionSource<string>();

            _server.OnDataArrival += args =>
            {
                receivedMessage.TrySetResult(args.Message);
                return Task.CompletedTask;
            };

            var listeningTask = _server.StartListening();
            await Task.Delay(200); 

            // Act
            _client = new TcpClient();
            await _client.ConnectAsync(IPAddress.Loopback, _port);
            var stream = _client.GetStream();
            var data = Encoding.ASCII.GetBytes(msg);
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();

            // Assert
            var completed = await Task.WhenAny(receivedMessage.Task, Task.Delay(2000));
            Assert.True(completed == receivedMessage.Task, "Mensagem não recebida a tempo");

            #pragma warning disable xUnit1031
            Assert.Equal(msg, receivedMessage.Task.Result);
            #pragma warning restore xUnit1031
        }

        public void Dispose()
        {
            _client?.Close();
        }
    }
}
