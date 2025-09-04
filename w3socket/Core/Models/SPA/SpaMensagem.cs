using System;

namespace W3Socket.Core.Models.SPA
{
    public record SpaMensagem : Mensagem
    {
        public bool isMessageComplete = false;
        public byte[] MessageBufferComplete;

        public tSPAProtocoloMAP uSPAProtocolo = new tSPAProtocoloMAP();
        public string cabecalhoOrigem = "";
        public string MensagemASCII = "";


        public void Clear()
        {
            base.Clear();
            this.cabecalhoOrigem = "";
        }

        ~SpaMensagem()
        {
            MessageBufferComplete = null;
        }
    }

    public struct tSPAProtocoloMAP
    {
        public tSPACabecalho msgCab;
        public byte[] msgBuf;
        public long bytesRec;

    }

    public struct tSPACabecalho
    {

        public byte versao;
        public byte timeToLive;
        public byte destinoOK;
        public byte compactada;
        public byte tamanhoCab;
        public short tamanhoMsg;
        public int cracha;
        public short timeOut;
        public short agenciaOrigem;
        public byte postoOrigem;
        public int operadorOrigem;
        public short idOrigem;
        public byte ipOrigem1;
        public byte ipOrigem2;
        public byte ipOrigem3;
        public byte ipOrigem4;
        public int portaOrigem;
        public short agenciaDestino;
        public byte postoDestino;
        public int operadorDestino;
        public short idDestino;
        public byte ipDestino1;
        public byte ipDestino2;
        public byte ipDestino3;
        public byte ipDestino4;
        public int portaDestino;
        public short produtoConta;
        public short produtoContaAg;
        public long produtoContaNo;

        public tSPACabecalho()
        {
                
        }


        public tSPACabecalho preparaCabVolta(short tamanhoMensagem)
        {
            return new tSPACabecalho
            {
                versao = versao,
                timeToLive = timeToLive,
                destinoOK = destinoOK,
                compactada = 0,
                tamanhoCab = tamanhoCab,
                tamanhoMsg = tamanhoMensagem,
                cracha = cracha,
                timeOut = timeOut,
                agenciaOrigem = agenciaDestino,
                postoOrigem = postoDestino,
                operadorOrigem = operadorDestino,
                idOrigem = idDestino,
                ipOrigem1 = ipDestino1,
                ipOrigem2 = ipDestino2,
                ipOrigem3 = ipDestino3,
                ipOrigem4 = ipDestino4,
                portaOrigem = portaDestino,
                agenciaDestino = agenciaOrigem,
                postoDestino = postoOrigem,
                operadorDestino = operadorOrigem,
                idDestino = idOrigem,
                ipDestino1 = ipOrigem1,
                ipDestino2 = ipOrigem2,
                ipDestino3 = ipOrigem3,
                ipDestino4 = ipOrigem4,
                portaDestino = portaOrigem,
                produtoConta = produtoContaAg,
                produtoContaAg = produtoContaAg,
                produtoContaNo = produtoContaNo
            };
        }

        public bool IsGarbage(int addHours = 0)
        {
            DateTime now = DateTime.Now;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is not "Local")
                now = now.AddHours(addHours);

            DateTime combinedDateTime = CreateDateTime(now, this.cracha);

            var _atraso = (now - combinedDateTime).TotalSeconds;
            var _ret = _atraso > this.timeOut ? true : false;

            if (_ret && this.timeOut>0)
            {
                Console.WriteLine($"[{DateTime.Now}] now: {now} ");
                Console.WriteLine($"[{DateTime.Now}] combinedDateTime: {combinedDateTime}  ");
                Console.WriteLine($"[{DateTime.Now}] atraso: {_atraso} segundos. Timeout {this.timeOut}");
            }

            return _ret;
        }

        internal DateTime CreateDateTime(DateTime date, int hhMMss)
        {

            int hour = hhMMss / 10000;
            int minute = (hhMMss / 100) % 100;
            int second = hhMMss % 100;

            // Create a new DateTime with the specified time
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second);
        }
    }


}
