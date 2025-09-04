using System.Net.Sockets;
using System.Text;
using System;

namespace W3Socket.Core.Models.SPA
{
    public sealed record TcpServerBufferArgs : IDisposable
    {
        public int ServerPort { get; set; }
        public string Message { get; internal set; }
        public byte[] BufferMessage { get; internal set; }
        public Socket SocketHandler { get; internal set; }

        public TcpServerBufferArgs(int tamanho)
        {
            Message = "";
            BufferMessage = new byte[tamanho];
        }

        public TcpServerBufferArgs(Socket handler, string message, int serverPort = 0)
        {
            BufferMessage = new byte[Encoding.ASCII.GetBytes(message).Length];
            BufferMessage = Encoding.ASCII.GetBytes(message);
            Message = message;
            SocketHandler = handler;
            ServerPort = serverPort;
        }

        public TcpServerBufferArgs(Socket handler, byte[] msgBuf, int serverPort = 0)
        {
            BufferMessage = msgBuf;
            Message = Encoding.ASCII.GetString(msgBuf);
            SocketHandler = handler;
            ServerPort = serverPort;
        }

        public void SetMessage(string message)
        {
            Message = message;
            BufferMessage = Encoding.ASCII.GetBytes(message);
        }

        public void SetMessage(byte[] buffer)
        {
            Message = Encoding.ASCII.GetString(buffer);
            BufferMessage = buffer;
        }

        public void SetClient(Socket client)
        {
            SocketHandler = client;
        }

        public void Dispose()
        {
            BufferMessage = null;
            Message = null;
            SocketHandler = null;
        }
    }
}
