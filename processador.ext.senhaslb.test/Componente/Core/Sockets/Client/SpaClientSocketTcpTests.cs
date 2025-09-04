using W3Socket.Core.Models.Excpetions;
using Microsoft.Extensions.Logging;
using W3Socket.Core.Sockets.Client;
using Moq;

namespace Componente.Core.Sockets.Client
{
    public class SpaClientSocketTcpTests
    {
        private readonly SpaClientScoketTcp _client;
        private readonly Mock<ILogger<SpaClientScoketTcp>> _loggerMock;

        public SpaClientSocketTcpTests()
        {
            _loggerMock = new Mock<ILogger<SpaClientScoketTcp>>();

            _client = new SpaClientScoketTcp(
                "127.0.0.1",
                12345,
                _loggerMock.Object,
                server => { },
                server => { },
                args => { },
                threads: 1
            );
        }

        [Fact]
        public void Constructor_Should_Initialize_Without_Exception()
        {
            Assert.NotNull(_client);
        }

        [Fact]
        public void Dispose_Should_Not_Throw()
        {
            var exception = Record.Exception(() => _client.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void ConnectHost_When_Server_Not_Reachable_Should_Throw()
        {
            Assert.Throws<W3SocketException>(() => _client.ConnectHost());
        }

        [Fact]
        public void IsConnected_When_Not_Connected_Should_Throw()
        {
            var ex = Record.Exception(() => _client.IsConnected());
            Assert.NotNull(ex); // _tcpClient is not initialized properly because ConnectHost falhou
        }

        [Fact]
        public void SendAsyncData_When_Not_Connected_Should_Throw()
        {
            Assert.ThrowsAny<Exception>(() => _client.SendAsyncData(new byte[] { 0x01, 0x02 }));
        }
    }
}
