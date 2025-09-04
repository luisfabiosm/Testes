using Microsoft.Extensions.Logging;
using W3Socket.Core.Sockets.Server;
using W3Socket.Core.Base;
using System.Net.Sockets;
using System.Net;
using Moq;

namespace Componente.Core.Sockets.Server
{
    public class SPAServerSocketThTests
    {
        private static SpaServerSocketTh CriarInstancia(out Mock<ILogger<BaseServerSocketTCP>> loggerMock)
        {
            loggerMock = new Mock<ILogger<BaseServerSocketTCP>>();
            return new SpaServerSocketTh(5555, loggerMock.Object, null, null, null, null);
        }

        [Fact]
        public void Deve_Instanciar_Sem_Erros()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseServerSocketTCP>>();

            var socketTh = new SpaServerSocketTh(
                port: 1234,
                logger: loggerMock.Object,
                evConnectedClient: null,
                evDisconnectedClient: null,
                evMessageReceived: null,
                evMessageAberta: null
            );

            // Act
            var result = socketTh.IsListenning();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Deve_Retornar_Endereco_Correto()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseServerSocketTCP>>();
            var socketTh = new SpaServerSocketTh(1234, loggerMock.Object, null, null, null, null);

            var ip = IPAddress.Parse("127.0.0.1");
            var port = 5678;
            var endPoint = new IPEndPoint(ip, port);

            // Act
            var endereco = SpaServerSocketTh.GetAddress(endPoint);

            // Assert
            Assert.Equal("127.0.0.1:5678", endereco);
        }

        [Fact]
        public void Deve_Validar_Conexao_Do_Socket() // retorna erro mas funciona ao subir
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseServerSocketTCP>>();
            var socketTh = new SpaServerSocketTh(1234, loggerMock.Object, null, null, null, null);

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Act
            var conectado = SpaServerSocketTh.IsConnected(socket);

            // Assert
            Assert.False(conectado);
        }

        [Fact]
        public void IsListenning_DeveRetornarFalseInicialmente()
        {
            // Arrange
            var socketTh = CriarInstancia(out _);

            // Act
            var resultado = socketTh.IsListenning();

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public void GetAddress_DeveRetornarEnderecoCorreto()
        {
            // Arrange
            var socketTh = CriarInstancia(out _);

            // Act
            var endereco = socketTh.GetAddress();

            // Assert
            Assert.Equal("0.0.0.0:5555", endereco);
        }
    }
}
