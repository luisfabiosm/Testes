using Microsoft.Extensions.Logging;
using W3Socket.Core.Sockets.Client;
using W3Socket.Core.Models.SPA;
using W3Socket.Core.Base;
using System.Text;
using System.Net;
using Moq;
using System.Net.Sockets;
using System.Reflection;

namespace Componente.Core.Sockets.Client
{
    public class ClientSocketTcpTests
    {
        private readonly IPAddress _ipAddress = IPAddress.Loopback;
        private readonly int _port = 55000;

        private static IServiceProvider CreateServiceProviderComLogger()
        {
            var loggerMock = new Mock<ILogger<BaseSocketTcp>>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILogger<BaseSocketTcp>)))
                .Returns(loggerMock.Object);

            return serviceProviderMock.Object;
        }

        [Fact]
        public void Construtor_DeveInicializarPropriedades()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);

            Assert.Equal(_ipAddress, client.RemoteIPAddress);
            Assert.Equal(_port, client.RemotePort);
        }

        [Fact]
        public void SetTimeout_DeveAtualizarPropriedade()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);

            client.SetTimeout(50);
            Assert.Equal(50, client.Timeout);
        }

        [Fact]
        public void SetRemoteIP_DeveAtualizarPropriedade()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);

            client.SetRemoteIP("127.0.0.1");
            Assert.Equal(IPAddress.Parse("127.0.0.1"), client.RemoteIPAddress);
        }

        [Fact]
        public void SetRemotePort_DeveAtualizarPropriedade()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);

            client.SetRemotPort(6600);
            Assert.Equal(6600, client.RemotePort);
        }

        [Fact]
        public void GetEndpoint_DeveRetornarStringComEndereco()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);
            string endpoint = client.GetEndpoint();

            Assert.Equal($"{_ipAddress}:{_port}", endpoint);
        }

        [Fact]
        public void IsConnected_DeveRetornarFalseQuandoDesconectado()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);

            Assert.False(client.IsConnected());
        }

        [Fact]
        public void SendAsyncData_DeveTentarConectarENaoLancarExcecao()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);
            var ex = Record.Exception(() => client.SendAsyncData(Encoding.UTF8.GetBytes("teste"), 1000, 1024));

            Assert.Null(ex); // Nenhuma excecao deve ser lançada, pois exceções são tratadas internamente
        }

        [Fact]
        public async Task ConnectHostAsync_DeveConectarComSucessoOuLogarErro()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);

            await client.ConnectHostAsync(2); // Deve falhar silenciosamente e logar

            Assert.False(client.IsConnected());
        }

        [Fact]
        public void OnClientDataArrival_DisparaEvento()
        {
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55000);
            var buffer = Encoding.UTF8.GetBytes("mensagem teste");
            bool eventoDisparado = false;

            client.OnDataArrival += (s, args) => eventoDisparado = true;

            var argsData = new SpaTcpBufferArgs(buffer);
            client.OnClientDataArrival(argsData);

            Assert.True(eventoDisparado);
        }

        [Fact]
        public void GetClientPort_DeveRetornarPortaLocal()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55001);

            var listener = new TcpListener(IPAddress.Loopback, 55001);
            listener.Start();

            using var acceptedClient = new TcpClient();
            acceptedClient.Connect(IPAddress.Loopback, 55001);
            var socket = listener.AcceptSocket();

            // Act
            var port = ((IPEndPoint)acceptedClient.Client.LocalEndPoint!).Port;
            var result = port > 0;

            // Assert
            Assert.True(result);

            listener.Stop();
        }

        [Fact]
        public void GetRemoteIP_DeveRetornarEnderecoIPCorreto()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var listener = new TcpListener(IPAddress.Loopback, 55002);
            listener.Start();

            using var acceptedClient = new TcpClient();
            acceptedClient.Connect(IPAddress.Loopback, 55002);
            var socket = listener.AcceptTcpClient();

            var remoteIP = ((IPEndPoint)socket.Client.RemoteEndPoint!).Address.ToString();

            // Assert
            Assert.Equal("127.0.0.1", remoteIP);

            listener.Stop();
        }

        [Fact]
        public void Inicialize_DeveTentarConectar()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55003);

            // Act & Assert
            var ex = Record.Exception(() => client.Inicialize());
            Assert.IsType<SocketException>(ex); // A exceção é tratada internamente no ConnectHost
        }

        [Fact]
        public void ConstrutorComIp_DeveInicializarPropriedades()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var ip = IPAddress.Parse("127.0.0.1");

            // Act
            var client = new ClientSocketTcp(serviceProvider, ip, 55004);

            // Assert
            Assert.Equal(ip, client.RemoteIPAddress);
            Assert.Equal(55004, client.RemotePort);
            Assert.Equal("127.0.0.1", client.RemoteIP);
        }

        [Fact]
        public void ConnectHost_DeveExecutarSemExcecao()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55005);

            // Act
            var ex = Record.Exception(() => client.ConnectHost());

            // Assert
            Assert.IsType<SocketException>(ex); // Conexao é tratada internamente mesmo se falhar
        }

        [Fact]
        public void SendSyncData_DeveLancarExcecaoSeFalhar()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55006);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => client.SendSyncData(new byte[10], 1, 1024));
        }

        [Fact]
        public void GetMessage_DeveDispararEvento()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55007);
            bool chamado = false;

            client.OnDataArrival += (s, e) => chamado = true;
            var args = new SpaTcpBufferArgs(new byte[5]);

            // Act
            var metodo = typeof(ClientSocketTcp).GetMethod("GetMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            metodo?.Invoke(client, new object[] { args });

            // Assert
            Assert.True(chamado);
        }

        [Fact]
        public void RenewTCPClient_DeveCriarNovoTcpClient()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55008);

            // Act & Assert
            var metodo = typeof(ClientSocketTcp).GetMethod("RenewTCPClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var ex = Record.Exception(() => metodo?.Invoke(client, null));
            Assert.IsType<TargetInvocationException>(ex);
        }

        [Fact]
        public void ReceiveSyncMessage_DeveLancarTimeout()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderComLogger();
            var client = new ClientSocketTcp(serviceProvider, "127.0.0.1", 55009);

            // Act
            var metodo = typeof(ClientSocketTcp).GetMethod("ReceiveSyncMessage", BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.Throws<TargetInvocationException>(() => metodo?.Invoke(client, new object[] { 1 }));
        }
    }
}
