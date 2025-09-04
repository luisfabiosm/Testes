using System.Text;

namespace W3Socket.Core.Models.SPA
{
    public record TCPClientBufferArgs
    {
        public string Message { get; set; }
        public byte[] BufferMessage { get; set; }

        public TCPClientBufferArgs(int tamanho)
        {
            Message = "";
            BufferMessage = new byte[tamanho];
        }

        public TCPClientBufferArgs(byte[] bytBuf, int tamanho)
        {
            BufferMessage = new byte[tamanho];
            BufferMessage = bytBuf;
            Message = Encoding.ASCII.GetString(bytBuf);
        }
    }
}
