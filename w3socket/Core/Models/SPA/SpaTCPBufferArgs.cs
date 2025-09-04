using System.Text;
using System;

namespace W3Socket.Core.Models.SPA
{
    public record SpaTcpBufferArgs
    {
        public bool ArgsSpaValido { get; set; }
        public byte[] ArgsSpaBuffer { get; set; }
        public SpaMensagem ArgsSpaMensagem { get; set; }
        public string ArgsSpaASCII { get; set; }
        public byte[] ArgsLastBuffer { get; set; }

        public SpaTcpBufferArgs(int tamanho)
        {
            ArgsSpaASCII = "";
            ArgsLastBuffer = new byte[tamanho];
        }

        public SpaTcpBufferArgs(byte[] bytBuf, int tamanho)
        {
            ArgsLastBuffer = new byte[tamanho];
            ArgsSpaASCII = Encoding.ASCII.GetString(bytBuf);
        }

        public SpaTcpBufferArgs()
        {
            ArgsSpaValido = false;
            ArgsSpaBuffer = new byte[0];
            ArgsSpaMensagem = null;
            ArgsSpaASCII = null;
            ArgsLastBuffer = new byte[0];
        }

        public SpaTcpBufferArgs(SpaMensagem spamessage)
        {
            this.ArgsSpaValido = true;
            this.ArgsSpaBuffer = Encoding.ASCII.GetBytes(spamessage.MensagemASCII);
            this.ArgsSpaMensagem = spamessage;
            this.ArgsSpaASCII = spamessage.MensagemASCII;
            this.ArgsLastBuffer = new byte[0];
        }

        public SpaTcpBufferArgs(SpaTcpBufferArgs oArgs, byte[] ByteBuffer)
        {
            this.ArgsSpaValido = false;
            this.ArgsSpaBuffer = new byte[0];
            this.ArgsLastBuffer = ByteBuffer;
            this.ArgsSpaMensagem = null;
            this.ArgsSpaASCII = "";

            this.ArgsLastBuffer = new byte[oArgs.ArgsLastBuffer.Length + ByteBuffer.Length];
            Buffer.BlockCopy(oArgs.ArgsLastBuffer, 0, this.ArgsLastBuffer, 0, oArgs.ArgsLastBuffer.Length);
            Buffer.BlockCopy(ByteBuffer, 0, this.ArgsLastBuffer, oArgs.ArgsLastBuffer.Length, ByteBuffer.Length);
        }

        public SpaTcpBufferArgs(bool isbufferspa, byte[] ByteBuffer)
        {
            this.ArgsSpaValido = isbufferspa;
            this.ArgsSpaBuffer = ByteBuffer;
            this.ArgsLastBuffer = new byte[0];
            this.ArgsSpaMensagem = null;
            this.ArgsSpaASCII = "";
        }

        public SpaTcpBufferArgs(byte[] ByteBuffer)
        {
            this.ArgsSpaValido = false;
            this.ArgsSpaBuffer = new byte[0];
            this.ArgsLastBuffer = ByteBuffer;
            this.ArgsSpaMensagem = null;
            this.ArgsSpaASCII = "";
        }

        public SpaTcpBufferArgs(byte[] ByteBuffer, byte[] ByteMessage)
        {
            this.ArgsSpaValido = true;
            this.ArgsSpaBuffer = ByteMessage;

            this.ArgsLastBuffer = new byte[ByteBuffer.Length - ByteMessage.Length];
            Buffer.BlockCopy(ByteBuffer, ByteMessage.Length, this.ArgsLastBuffer, 0, this.ArgsLastBuffer.Length);

            this.ArgsSpaMensagem = null;
            this.ArgsSpaASCII = "";
        }
    }
}
