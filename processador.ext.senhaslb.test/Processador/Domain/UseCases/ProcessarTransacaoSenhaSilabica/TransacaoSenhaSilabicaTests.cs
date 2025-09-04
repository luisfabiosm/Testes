using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using W3Socket.Core.Models.SPA;
using Domain.Core.Models.SPA;
using System.Diagnostics;
using Domain.Core.Enums;
using System.Text;

namespace Processador.Domain.UseCases.ProcessarTransacaoSenhaSilabica
{
    public class TransacaoSenhaSilabicaTests
    {
        static TransacaoSenhaSilabicaTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void Construtor_Padrao_DeveInstanciar()
        {
            // Act
            var transacao = new TransacaoSenhaSilabica();

            // Assert
            Assert.NotNull(transacao);
        }

        [Fact]
        public void Construtor_ComMensagem_DevePopularPropriedades()
        {
            // Arrange
            var mockCabecalho = new tSPACabecalho
            {
                cracha = 123,
                timeOut = 456
            };

            // Act
            var transacao = new TransacaoSenhaSilabica("TRANSACAO|TESTE");
            transacao.CabecalhoSPA = mockCabecalho;

            // Assert
            Assert.Equal(123, transacao.Cracha);
            Assert.Equal(456, transacao.OrigemTimeout);
        }

        [Fact]
        public void Propriedades_Personalizadas_DeveSetarECapturarCorretamente()
        {
            // Arrange
            var transacao = new TransacaoSenhaSilabica
            {
                CorrelationId = "abc-123",
                TranActivity = new Activity("Teste")
            };

            // Act & Assert
            Assert.Equal("abc-123", transacao.CorrelationId);
            Assert.NotNull(transacao.TranActivity);
        }

        [Fact]
        public void BufferMessage_DeveSetarEMontarMensagemIN()
        {
            // Arrange
            var transacao = new TransacaoSenhaSilabica();
            var msgTexto = "TRAN0000074010\v00000035\v\vW3-DEV-14\v3\v740\vR\v25/06/2025 00:00:00\v15\v1\v48\v48\v01/01/1900 00:00:00\v0\v1\v\v0\v0\v19\v0\v1\v3\v25/06/2025 00:00:00\v0\v00000035||W3-DEV-14|3|740|P|01/01/1900 00:00:00|15|1|0|0|01/01/1900 00:00:00|15|1||0|1|0|0|0|3|25/06/2025 00:00:00|0|||6372332000000012=19126061270000000000||0|0|||||||||||||||||0||I||||||||||||||||||||||||0||||||\v\v6372332000000012=19126061270000000000\v6372332000000012\v0\v1\v0\v0\v0\v0\v0\v0000000000\v\v0\v0\v0000\v0000\v \v0\v\v0\v\v0\v0\vI\v\v\v\v0\v \v \v \v \v\v \v \v\v\v\v\v\v0\v \v\v\v\v \v0\v0\v\v0\v\v \v\v";
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var buffer = Encoding.GetEncoding("Windows-1252").GetBytes(msgTexto);

            // Act
            transacao.BufferMessage = buffer;

            // Assert
            Assert.NotNull(transacao.MensagemIN.ToString());
            Assert.Contains("TRAN0000074010", transacao.MensagemIN.ToString());
            Assert.NotNull(transacao.SPAMensagemIN);
        }

        [Fact]
        public void CabecalhoSPA_DeveAtualizarCrachaETimeout()
        {
            // Arrange
            var transacao = new TransacaoSenhaSilabica();
            var cabecalho = new tSPACabecalho
            {
                cracha = 789,
                timeOut = 321
            };

            // Act
            transacao.CabecalhoSPA = cabecalho;

            // Assert
            Assert.Equal(789, transacao.Cracha);
            Assert.Equal(321, transacao.OrigemTimeout);
        }

        [Fact]
        public void MSG_TRAN_SEPCAMPOS_DeveConterCaractereASCII11()
        {
            // Arrange & Act
            var separador = TransacaoSenhaSilabica.MSG_TRAN_SEPCAMPOS;

            // Assert
            Assert.Equal(((char)11).ToString(), separador);
        }

        [Fact]
        public void SPAMensagemOUT_DeveRetornarObjetoDefinido()
        {
            // Arrange
            var transacao = new TransacaoSenhaSilabica();
            var mensagemOut = new MAgentMensagem("TRAN0000074010\v00000035\v\vW3-DEV-14\v3\v740\vR\v25/06/2025 00:00:00\v15\v1\v48\v48\v01/01/1900 00:00:00\v0\v1\v\v0\v0\v19\v0\v1\v3\v25/06/2025 00:00:00\v0\v00000035||W3-DEV-14|3|740|P|01/01/1900 00:00:00|15|1|0|0|01/01/1900 00:00:00|15|1||0|1|0|0|0|3|25/06/2025 00:00:00|0|||6372332000000012=19126061270000000000||0|0|||||||||||||||||0||I||||||||||||||||||||||||0||||||\v\v6372332000000012=19126061270000000000\v6372332000000012\v0\v1\v0\v0\v0\v0\v0\v0000000000\v\v0\v0\v0000\v0000\v \v0\v\v0\v\v0\v0\vI\v\v\v\v0\v \v \v \v \v\v \v \v\v\v\v\v\v0\v \v\v\v\v \v0\v0\v\v0\v\v \v\v");

            // Usa reflexão para setar valor interno
            typeof(TransacaoSenhaSilabica)
                .GetProperty(nameof(TransacaoSenhaSilabica.SPAMensagemOUT))!
                .SetValue(transacao, mensagemOut);

            // Act
            var resultado = transacao.SPAMensagemOUT;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(mensagemOut, resultado);
        }

        [Fact]
        public void MetodoAcao_DeveRetornarValorEsperado()
        {
            // Arrange
            var transacao = new TransacaoSenhaSilabica();
            var mensagem = "TRAN0000074010\v00000035\v\vW3-DEV-14\v3\v740\vR\v25/06/2025 00:00:00\v15\v1\v48\v48\v01/01/1900 00:00:00\v0\v1\v\v0\v0\v19\v0\v1\v3\v25/06/2025 00:00:00\v0\v00000035||W3-DEV-14|3|740|P|01/01/1900 00:00:00|15|1|0|0|01/01/1900 00:00:00|15|1||0|1|0|0|0|3|25/06/2025 00:00:00|0|||6372332000000012=19126061270000000000||0|0|||||||||||||||||0||I||||||||||||||||||||||||0||||||\v\v6372332000000012=19126061270000000000\v6372332000000012\v0\v1\v0\v0\v0\v0\v0\v0000000000\v\v0\v0\v0000\v0000\v \v0\v\v0\v\v0\v0\vI\v\v\v\v0\v \v \v \v \v\v \v \v\v\v\v\v\v0\v \v\v\v\v \v0\v0\v\v0\v\v \v\v"; // conteudo mínimo para parse
            var buffer = Encoding.GetEncoding("Windows-1252").GetBytes(mensagem);
            transacao.BufferMessage = buffer;

            // Act
            var metodo = transacao.MetodoAcao;

            // Assert
            Assert.True(Enum.IsDefined(typeof(EnumMetodoAcao), metodo));
        }

        [Fact]
        public void BufferMessage_Get_DeveRetornarBufferDefinido()
        {
            // Arrange
            var transacao = new TransacaoSenhaSilabica();
            var dados = Encoding.GetEncoding("Windows-1252").GetBytes("TRAN0000074010\v00000035\v\vW3-DEV-14\v3\v740\vR\v25/06/2025 00:00:00\v15\v1\v48\v48\v01/01/1900 00:00:00\v0\v1\v\v0\v0\v19\v0\v1\v3\v25/06/2025 00:00:00\v0\v00000035||W3-DEV-14|3|740|P|01/01/1900 00:00:00|15|1|0|0|01/01/1900 00:00:00|15|1||0|1|0|0|0|3|25/06/2025 00:00:00|0|||6372332000000012=19126061270000000000||0|0|||||||||||||||||0||I||||||||||||||||||||||||0||||||\v\v6372332000000012=19126061270000000000\v6372332000000012\v0\v1\v0\v0\v0\v0\v0\v0000000000\v\v0\v0\v0000\v0000\v \v0\v\v0\v\v0\v0\vI\v\v\v\v0\v \v \v \v \v\v \v \v\v\v\v\v\v0\v \v\v\v\v \v0\v0\v\v0\v\v \v\v");
            transacao.BufferMessage = dados;

            // Act
            var resultado = transacao.BufferMessage;

            // Assert
            Assert.Equal(dados, resultado.ToArray());
        }
    }
}
