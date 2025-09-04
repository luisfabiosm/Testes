using W3Socket.Core.Sockets.Server;
using Microsoft.Extensions.Logging;
using W3Socket.Core.Base;
using System.Reflection;
using System.Text;
using Moq;
using System.Net.Sockets;
using System.Net;
using W3Socket.Core.Models.SPA;

namespace Componente.Core.Sockets.Server
{
    public class SPAServerSocketTkTests : IDisposable
    {
        private readonly SpaServerSocketTk _server;
        private readonly int _testPort = 50100;
        private readonly ILogger<BaseServerSocketTCP> _logger;

        private readonly ManualResetEventSlim _clientConnected = new();
        private readonly ManualResetEventSlim _messageReceived = new();

        private SpaTcpBufferArgs? _receivedArgs;

        public SPAServerSocketTkTests()
        {
            var loggerMock = new Mock<ILogger<BaseServerSocketTCP>>();
            _logger = loggerMock.Object;

            _server = new SpaServerSocketTk(
                _testPort,
                _logger,
                OnClientConnected,
                OnClientDisconnected,
                OnMessageReceived,
                msg => { }, // Mensagem aberta - ignorada aqui
                threads: 1
            );
        }

        private void OnClientConnected(Socket client)
        {
            _clientConnected.Set();
        }

        private void OnClientDisconnected(Socket client)
        {
            // Pode ser usado em testes futuros
        }

        private void OnMessageReceived(SpaTcpBufferArgs args, Socket socket)
        {
            _receivedArgs = args;
            _messageReceived.Set();
        }

        [Fact]
        public async Task DeveConectarEReceberMensagem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseServerSocketTCP>>();
            var mensagemRecebida = new TaskCompletionSource<byte[]>();

            SpaServerSocketTk.delReceberMensagem onReceberMensagem = (args, socket) =>
            {
                mensagemRecebida.TrySetResult(args.ArgsSpaBuffer);
            };

            SpaServerSocketTk.delClient onClientConnected = _ => { };
            SpaServerSocketTk.delClient onClientDisconnected = _ => { };
            SpaServerSocketTk.delMensagemAberta onMensagemAberta = _ => { };

            int port;
            using (var tempListener = new TcpListener(IPAddress.Loopback, 0))
            {
                tempListener.Start();
                port = ((IPEndPoint)tempListener.LocalEndpoint).Port;
                tempListener.Stop();
            }

            var server = new SpaServerSocketTk(port, loggerMock.Object,
                onClientConnected, onClientDisconnected, onReceberMensagem, onMensagemAberta);

            server.StartListening(1024, 1000, 1024, 1000, 64);

            var aguardarInicio = SpinWait.SpinUntil(() => server.IsListenning(), 1000);
            Assert.True(aguardarInicio, "Servidor não iniciou corretamente.");

            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            await Task.Delay(100);
            var stream = client.GetStream();

            byte[] mensagem = new byte[100];
            BitConverter.GetBytes((short)100).CopyTo(mensagem, 6);
            await stream.WriteAsync(mensagem, 0, mensagem.Length);
            await stream.FlushAsync();

            // Act
            var completed = await Task.WhenAny(mensagemRecebida.Task, Task.Delay(2000));

            // Assert
            Assert.True(completed == mensagemRecebida.Task, "Mensagem não foi recebida a tempo");
            #pragma warning disable xUnit1031 
            Assert.Equal(mensagem.Take(100), mensagemRecebida.Task.Result);
            #pragma warning restore xUnit1031
        }

        [Fact]
        public void GetAddress_DeveRetornarEnderecoCorreto()
        {
            // Arrange
            var porta = 1234;
            var server = new SpaServerSocketTk(
                porta,
                _logger,
                null,
                null,
                (args, socket) => { },
                msg => { });

            // Act
            var endereco = server.GetAddress();

            // Assert
            Assert.Contains("0.0.0.0:1234", endereco);
            Assert.Contains(porta.ToString(), endereco);
        }

        [Fact]
        public void GetAddress_ComParametro_DeveRetornarEnderecoFormatado()
        {
            // Arrange
            var porta = 5678;
            var server = new SpaServerSocketTk(
                porta,
                _logger,
                null,
                null,
                (args, socket) => { },
                msg => { });

            var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.10"), porta);

            // Act
            var endereco = SpaServerSocketTk.GetAddress(ipEndPoint);

            // Assert
            Assert.Equal("192.168.0.10:5678", endereco);
        }

        [Fact]
        public async Task IsConnected_DeveRetornarTrue_ParaSocketConectado()
        {
            // Arrange
            var serverSocket = new TcpListener(IPAddress.Loopback, 0);
            serverSocket.Start();
            var port = ((IPEndPoint)serverSocket.LocalEndpoint).Port;

            var spaServer = new SpaServerSocketTk(
                port,
                _logger,
                null,
                null,
                (args, socket) => { },
                msg => { });

            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);
            var socket = serverSocket.AcceptSocket();

            // Act
            var conectado = SpaServerSocketTk.IsConnected(socket);

            // Assert
            Assert.True(conectado);

            // Cleanup
            socket.Dispose();
            client.Dispose();
            serverSocket.Stop();
        }

        [Fact]
        public void IsConnected_DeveRetornarFalse_ParaSocketFechado()
        {
            // Arrange
            var server = new SpaServerSocketTk(
                1234,
                _logger,
                null,
                null,
                (args, socket) => { },
                msg => { });

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Close(); // fecha o socket imediatamente

            // Act
            var conectado = SpaServerSocketTk.IsConnected(socket);

            // Assert
            Assert.False(conectado);
        }

        [Fact]
        public void StopListening_DevePararServidorSemErros()
        {
            // Arrange
            var server = new SpaServerSocketTk(
                1234,
                _logger,
                null,
                null,
                (args, socket) => { },
                msg => { });

            server.StartListening(1024, 1000, 1024, 1000, 64);
            Thread.Sleep(100); // Espera o StartListening ativar

            // Act
            server.StopListening();

            // Assert
            Assert.False(server.IsListenning());
        }

        [Fact]
        public void ExtractMessage_DeveExtrairMensagemValida()
        {
            // Arrange
            var server = new SpaServerSocketTk(
                1234,
                _logger,
                null,
                null,
                (args, socket) => { },
                msg => { });

            byte[] buffer = new byte[100];
            BitConverter.GetBytes((short)100).CopyTo(buffer, 6); // Define tamanho da mensagem

            // Act
            var method = typeof(SpaServerSocketTk).GetMethod("ExtractMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (SpaTcpBufferArgs)method!.Invoke(server, new object[] { buffer, 100L })!;

            // Assert
            Assert.True(result.ArgsSpaValido);
            Assert.Equal(100, result.ArgsSpaBuffer.Length);
        }

        [Fact]
        public void Construtor_DeveConfigurarEventosCorretamente()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseServerSocketTCP>>();
            var mockConectado = new Mock<SpaServerSocketTk.delClient>();
            var mockDesconectado = new Mock<SpaServerSocketTk.delClient>();
            var mockRecebido = new Mock<SpaServerSocketTk.delReceberMensagem>();
            var mockAberta = new Mock<SpaServerSocketTk.delMensagemAberta>();

            // Act
            var server = new SpaServerSocketTk(
                1234,
                mockLogger.Object,
                mockConectado.Object,
                mockDesconectado.Object,
                mockRecebido.Object,
                mockAberta.Object
            );

            // Assert
            Assert.NotNull(server);
            Assert.True(server.IsListenning() == false);
        }

        [Fact]
        public async Task ProcessMessageAsync_DeveDispararEvento_OnReceberMensagem()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseServerSocketTCP>>();
            var mensagemRecebida = false;

            var server = new SpaServerSocketTk(
                1234,
                mockLogger.Object,
                null,
                null,
                (args, socket) => { mensagemRecebida = true; },
                msg => { }
            );

            var fakeMessage = Encoding.UTF8.GetBytes("mensagem teste");

            // Act
            var field = typeof(SpaServerSocketTk).GetMethod("ProcessMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)field!.Invoke(server, new object[] { fakeMessage })!;

            // Assert
            Assert.True(mensagemRecebida);
        }

        [Fact]
        public void GetAddress_ComEndPointNulo_DeveLancarExcecao()
        {
            // Arrange
            EndPoint? endpoint = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
            {
                var _ = SpaServerSocketTk.GetAddress(endpoint!);
            });
        }

        [Fact]
        public void StartListening_QuandoJaEstiverOuvindo_DeveRetornarSemExcecao()
        {
            // Arrange
            var server = new SpaServerSocketTk(1234, _logger, null, null, (args, socket) => { }, msg => { });

            server.StartListening(1024, 1000, 1024, 1000, 64);
            Thread.Sleep(100); // espera iniciar

            // Act
            var ex = Record.Exception(() => server.StartListening(1024, 1000, 1024, 1000, 64));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void StartListening_QuandoLancarExcecao_DeveSerCapturada()
        {
            // Arrange
            var server = new SpaServerSocketTk(0, _logger, null, null, (args, socket) => { }, msg => { });

            // Simula porta inválida para forçar erro
            typeof(SpaServerSocketTk)
                .GetField("_server", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(server, null); // garante reset

            // Act
            var ex = Record.Exception(() => server.StartListening(1024, 1000, 1024, 1000, 64));

            // Assert
            Assert.Null(ex); // exceção é tratada internamente
        }

        [Fact]
        public void StopListening_QuandoLancarExcecao_DeveSerCapturada()
        {
            // Arrange
            var server = new SpaServerSocketTk(1234, _logger, null, null, (args, socket) => { }, msg => { });

            // força erro removendo _server
            typeof(SpaServerSocketTk)
                .GetField("_server", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(server, null);

            // Act
            var ex = Record.Exception(() => server.StopListening());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void AcceptSocketCallBack_QuandoSocketForNull_NaoDeveLancarExcecao()
        {
            // Arrange
            var server = new SpaServerSocketTk(1234, _logger, null, null, (args, socket) => { }, msg => { });

            var method = typeof(SpaServerSocketTk)
                .GetMethod("AcceptSocketCallBack", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var ex = Record.Exception(() =>
            {
                method!.Invoke(server, new object[] { null! });
            });

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void Dispose_DeveCancelarToken()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseServerSocketTCP>>();
            var server = new SpaServerSocketTk(
                1234,
                mockLogger.Object,
                null,
                null,
                (args, socket) => { },
                msg => { }
            );

            // Act
            server.Dispose();

            // Assert
            var ctsField = typeof(SpaServerSocketTk)
                .GetField("_cts", BindingFlags.NonPublic | BindingFlags.Instance);
            var cts = (CancellationTokenSource)ctsField!.GetValue(server)!;

            Assert.True(cts.IsCancellationRequested);
        }

        public void Dispose()
        {
            _server.StopListening();
            _server.Dispose();
        }
    }
}
