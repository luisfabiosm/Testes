using System.Runtime.InteropServices;
using W3Socket.Core.Models.SPA;
using System.Text.Json;

namespace Adapters.Outbound.TCPAdapter.Mapping
{
    public static class MappingSPAMensagem
    {
        public static tSPACabecalho MappingCabecalhoDestino(tSPACabecalho cabecalhoOrigem, short tamanhoMensagem)
        {
            return new tSPACabecalho
            {
                versao = cabecalhoOrigem.versao,
                timeToLive = cabecalhoOrigem.timeToLive,
                destinoOK = cabecalhoOrigem.destinoOK,
                compactada = 0,
                tamanhoCab = cabecalhoOrigem.tamanhoCab,
                tamanhoMsg = tamanhoMensagem,
                cracha = cabecalhoOrigem.cracha,
                timeOut = cabecalhoOrigem.timeOut,
                agenciaOrigem = cabecalhoOrigem.agenciaDestino,
                postoOrigem = cabecalhoOrigem.postoDestino,
                operadorOrigem = cabecalhoOrigem.operadorDestino,
                idOrigem = cabecalhoOrigem.idDestino,
                ipOrigem1 = cabecalhoOrigem.ipDestino1,
                ipOrigem2 = cabecalhoOrigem.ipDestino2,
                ipOrigem3 = cabecalhoOrigem.ipDestino3,
                ipOrigem4 = cabecalhoOrigem.ipDestino4,
                portaOrigem = cabecalhoOrigem.portaDestino,
                agenciaDestino = cabecalhoOrigem.agenciaOrigem,
                postoDestino = cabecalhoOrigem.postoOrigem,
                operadorDestino = cabecalhoOrigem.operadorOrigem,
                idDestino = cabecalhoOrigem.idOrigem,
                ipDestino1 = cabecalhoOrigem.ipOrigem1,
                ipDestino2 = cabecalhoOrigem.ipOrigem2,
                ipDestino3 = cabecalhoOrigem.ipOrigem3,
                ipDestino4 = cabecalhoOrigem.ipOrigem4,
                portaDestino = cabecalhoOrigem.portaOrigem,
                produtoConta = cabecalhoOrigem.produtoContaAg,
                produtoContaAg = cabecalhoOrigem.produtoContaAg,
                produtoContaNo = cabecalhoOrigem.produtoContaNo
            };
        }

        internal static byte[] copiaStructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[size];

            Span<byte> byteSpan = bytes.AsSpan();
            MemoryMarshal.Write(byteSpan, in structure);

            return bytes;
        }

        internal static byte[] copiaRecordToBytes<T>(T record) where T : class
        {
            return JsonSerializer.SerializeToUtf8Bytes(record);
        }

        internal static T DeserializeBytesToRecord<T>(byte[] bytes) where T : class
        {
            return JsonSerializer.Deserialize<T>(bytes)!;
        }
    }
}
