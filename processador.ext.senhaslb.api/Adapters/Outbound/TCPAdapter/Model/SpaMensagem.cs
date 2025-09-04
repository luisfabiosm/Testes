namespace Adapters.Outbound.TCPAdapter.Model
{
    public record SpaMensagem
    {
        public tSPAProtocoloMAP uSPAProtocolo = new tSPAProtocoloMAP();
        public string cabecalhoOrigem = string.Empty;
        public string mensagem = string.Empty;
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
    }
}
