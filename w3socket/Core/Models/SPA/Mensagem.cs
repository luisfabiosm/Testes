using System;

namespace W3Socket.Core.Models.SPA
{
    public record Mensagem
    {
        public byte[] MensagemBuffer;
        public long BytesRecebidos = 0;
        public long BytesProcessados = 0;

        public Mensagem() { }

        public void CriarBuffer(byte[] bytBuffer, long totalRecebido)
        {
            var itotal = (int)totalRecebido;

            if (this.MensagemBuffer is null || this.MensagemBuffer.Length == 0)
            {
                this.MensagemBuffer = new byte[itotal];
                Buffer.BlockCopy(bytBuffer, 0, this.MensagemBuffer, 0, itotal);
            }
            else
            {
                byte[] _memBuffer = new byte[this.MensagemBuffer.Length + itotal];
                Buffer.BlockCopy(this.MensagemBuffer, 0, _memBuffer, 0, this.MensagemBuffer.Length);
                Buffer.BlockCopy(bytBuffer, 0, _memBuffer, this.MensagemBuffer.Length, itotal);

                this.MensagemBuffer = new byte[_memBuffer.Length];
                Buffer.BlockCopy(_memBuffer, 0, this.MensagemBuffer, 0, _memBuffer.Length);
            }

            this.BytesProcessados = 0;
        }

        public void PrepararBuffer(long totalProcessado)
        {
            this.BytesProcessados = this.BytesProcessados + totalProcessado;
            this.BytesRecebidos = 0;
        }

        public void AtualizarBuffer(long totalrecebido, long totalProcessado)
        {
            byte[] _menBufffer = new byte[totalrecebido - this.BytesProcessados];

            Array.Copy(this.MensagemBuffer, this.BytesProcessados, _menBufffer, 0, totalrecebido - this.BytesProcessados);
            Array.Resize(ref this.MensagemBuffer, _menBufffer.Length);
            Array.Copy(_menBufffer, 0, this.MensagemBuffer, 0, totalrecebido - this.BytesProcessados);

            this.BytesProcessados = this.BytesProcessados + this.MensagemBuffer.Length;
            this.BytesRecebidos = 0;
        }

        public void Clear(long totalprocessado = 0)
        {
            if (this.MensagemBuffer.Length == totalprocessado)
            {
                this.MensagemBuffer = new byte[0];
            }
            this.BytesRecebidos = 0;
            this.BytesProcessados = 0;
        }

        ~Mensagem() { }
    }
}
