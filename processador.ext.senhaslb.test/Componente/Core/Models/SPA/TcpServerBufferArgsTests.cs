using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using W3Socket.Core.Models.SPA;

namespace Componente.Core.Models.SPA
{
    public class TcpServerBufferArgsTests
    {
        [Fact]
        public void Construtor_ComTamanho_DeveInicializarBufferEVazio()
        {
            var args = new TcpServerBufferArgs(128);

            Assert.NotNull(args.BufferMessage);
            Assert.Equal(128, args.BufferMessage.Length);
            Assert.Equal(string.Empty, args.Message);
        }

        [Fact]
        public void Construtor_ComSocketEMessage_DeveInicializarPropriedadesCorretamente()
        {
            var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string message = "mensagem teste";

            var args = new TcpServerBufferArgs(fakeSocket, message, 5000);

            Assert.Equal(message, args.Message);
            Assert.Equal(Encoding.ASCII.GetBytes(message), args.BufferMessage);
            Assert.Equal(fakeSocket, args.SocketHandler);
            Assert.Equal(5000, args.ServerPort);
        }

        [Fact]
        public void Construtor_ComSocketEBuffer_DeveInicializarPropriedadesCorretamente()
        {
            var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byte[] buffer = Encoding.ASCII.GetBytes("teste buffer");

            var args = new TcpServerBufferArgs(fakeSocket, buffer, 5500);

            Assert.Equal("teste buffer", args.Message);
            Assert.Equal(buffer, args.BufferMessage);
            Assert.Equal(fakeSocket, args.SocketHandler);
            Assert.Equal(5500, args.ServerPort);
        }

        [Fact]
        public void SetMessage_ComString_DeveAtualizarPropriedades()
        {
            var args = new TcpServerBufferArgs(10);
            string novaMensagem = "nova msg";

            args.SetMessage(novaMensagem);

            Assert.Equal(novaMensagem, args.Message);
            Assert.Equal(Encoding.ASCII.GetBytes(novaMensagem), args.BufferMessage);
        }

        [Fact]
        public void SetMessage_ComBuffer_DeveAtualizarPropriedades()
        {
            var args = new TcpServerBufferArgs(10);
            byte[] buffer = Encoding.ASCII.GetBytes("buffer msg");

            args.SetMessage(buffer);

            Assert.Equal("buffer msg", args.Message);
            Assert.Equal(buffer, args.BufferMessage);
        }

        [Fact]
        public void SetClient_DeveAtualizarSocketHandler()
        {
            var args = new TcpServerBufferArgs(5);
            var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            args.SetClient(fakeSocket);

            Assert.Equal(fakeSocket, args.SocketHandler);
        }

        [Fact]
        public void Dispose_DeveLimparRecursos()
        {
            var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var args = new TcpServerBufferArgs(fakeSocket, "teste", 1234);

            args.Dispose();

            Assert.Null(args.Message);
            Assert.Null(args.BufferMessage);
            Assert.Null(args.SocketHandler);
        }
    }
}
