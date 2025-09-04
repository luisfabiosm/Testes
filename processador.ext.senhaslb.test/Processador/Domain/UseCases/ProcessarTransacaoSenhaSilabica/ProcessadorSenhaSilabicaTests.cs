using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Adapters.Outbound.SenhaAlfaAdapter.Models;
using Adapters.Outbound.DBAdapter.Model;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using W3Socket.Core.Models.SPA;
using Domain.Core.Exceptions;
using Domain.Core.Models.SPA;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Domain.Core.Enums;
using Domain.Core.Base;
using System.Text;
using Moq;

namespace Processador.Domain.UseCases.ProcessarTransacaoSenhaSilabica
{
    public class ProcessadorSenhaSilabicaTests
    {
        static ProcessadorSenhaSilabicaTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private ProcessadorSenhaSilabica CreateService(
            Mock<ISPAOperadorService>? mockOperador = null,
            Mock<ISAServicePort>? mockSaService = null,
            Mock<ISPATcpClientServicePort>? mockTcpClient = null,
            Mock<ILogger<ProcessadorSenhaSilabica>>? mockLogger = null,
            Mock<IOtlpServicePort>? mockOtlpService = null,
            Mock<IOptions<GCSrvSettings>>? mockOptions = null)
        {
            mockOperador ??= new Mock<ISPAOperadorService>();
            mockSaService ??= new Mock<ISAServicePort>();
            mockTcpClient ??= new Mock<ISPATcpClientServicePort>();
            mockLogger ??= new Mock<ILogger<ProcessadorSenhaSilabica>>();
            mockOtlpService ??= new Mock<IOtlpServicePort>();
            mockOptions ??= new Mock<IOptions<GCSrvSettings>>();
            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            serviceProvider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            serviceProvider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            serviceProvider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);

            return new ProcessadorSenhaSilabica(serviceProvider.Object);
        }

        #region UtilsTests

        [Fact]
        public void Construtor_DeveInstanciarClasseSemErros()
        {
            var service = CreateService();
            Assert.NotNull(service);
        }

        [Fact]
        public void ValidarGarbage_QuandoDesativado_DeveRetornarFalse()
        {
            var service = CreateService();
            var cabecalho = new W3Socket.Core.Models.SPA.tSPACabecalho();
            var result = service.ValidarGarbage(cabecalho);
            Assert.False(result);
        }

        [Fact]
        public void RemoverAcentos_DeveRemoverCorretamente()
        {
            var service = CreateService();
            var texto = "áéíóú ç âêô";
            var resultado = service.RemoverAcentos(texto);
            Assert.Equal("aeiou c aeo", resultado);
        }

        [Fact]
        public void ConcatenarParametros_QuandoParametrosNulos_DeveConcatenarComZeros()
        {
            var service = CreateService();

            var parametros = typeof(ParametrosOutput)
                .GetProperties()
                .Select(p => "0")
                .ToArray();

            var transacao = new SPATransacao { ListParametros = new List<SPAParametro>() };
            var resultado = service.ConcatenarParametros(transacao);

            var esperado = string.Join("|", parametros);
            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void TraceFiltro_DeveAdicionarTagsCorretamente()
        {
            var service = CreateService();

            using var activity = new Activity("trace_test");
            activity.Start();

            var parametros = new List<SPAParametro>
            {
                new(new SqlParameter { ParameterName = "@psmlContaAg", Value = "123" }, 1),
                new(new SqlParameter { ParameterName = "@pintConta", Value = "456" }, 2),
                new(new SqlParameter { ParameterName = "@pnumValor", Value = "789" }, 3)
            };

            service.TraceFiltro(activity, parametros);

            Assert.Equal("123", activity.Tags.FirstOrDefault(t => t.Key == "AgConta").Value);
            Assert.Equal("456", activity.Tags.FirstOrDefault(t => t.Key == "Conta").Value);
            Assert.Equal("789", activity.Tags.FirstOrDefault(t => t.Key == "Valor").Value);
        }

        #endregion

        #region MapperTests

        [Fact]
        public void MapBaseReturnToException_DeveIncluirMensagemEBaseReturn()
        {
            var service = CreateService();
            var baseReturn = new BaseReturn { Mensagem = "Erro de teste" };

            var ex = service.mapBaseReturnToException(baseReturn);

            Assert.Equal("Erro de teste", ex.Message);
            Assert.True(ex.Data.Contains("BaseReturn"));
        }

        [Fact]
        public void MapExceptionToBaseReturn_DeveMapearSPAExceptionCorretamente()
        {
            var service = CreateService();
            var erro = new SPAError("teste erro")
            {
                Codigo = 1,
                Tipo = EnumSPATipoErroInterno.Negocio,
                Mensagem = "mensagem acêntuada"
            };

            var spaEx = new SPAException(erro);
            var transacao = new TransacaoSenhaSilabica("TRAN0000000000\vDADO1\v\vDADO2\v\v\v\v\v\v\v\v\v\v\v\v\v\vVALOR17")
            {
                CabecalhoSPA = new W3Socket.Core.Models.SPA.tSPACabecalho()
            };

            var retorno = service.MapExceptionToBaseReturn(transacao, spaEx);

            Assert.Contains("mensagem acentuada", retorno.Mensagem!.Normalize(NormalizationForm.FormD));
            Assert.Equal(EnumStatus.NEGOCIO, retorno.Status);
        }

        [Fact]
        public void MapExceptionToBaseReturn_DeveMapearExceptionGenerica()
        {
            var service = CreateService();
            var ex = new Exception("erro generico");

            var transacao = new TransacaoSenhaSilabica("TRAN0000000000\vDADO1\v\vDADO2\v\v\v\v\v\v\v\v\v\v\v\v\v\vVALOR17")
            {
                CabecalhoSPA = new W3Socket.Core.Models.SPA.tSPACabecalho()
            };

            var retorno = service.MapExceptionToBaseReturn(transacao, ex);

            Assert.Contains("erro generico", retorno.Mensagem);
            Assert.Equal(EnumStatus.SISTEMA, retorno.Status);
        }

        #endregion

        #region 740e741Tests

        [Fact]
        public async Task ExecutarFluxo740_QuandoSucesso_DeveRetornarParametroEsperado()
        {
            // Arrange
            var retornoSaida = new GerarSaidaSenhaResponse
            {
                returnCode = (int)EnumStatus.SUCESSO,
                seq1 = "SEQ1",
                seq2 = "SEQ2",
                seq3 = "SEQ3",
                grupo8 = "GRUPO8",
                dataHora = 123456
            };

            var retornoSPX = new List<RetornoSPX>
            {
                new() { vchParam = "RETORNO_PARAMETRO" }
            };

            var msgOut = "x|1|AG|CONTA|123456|RESPOSTA|y";
            var dadosConcatenados = "DADOS|CONCATENADOS|EXEMPLO";

            var mockSaService = new Mock<ISAServicePort>();
            var mockOperador = new Mock<ISPAOperadorService>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockOtlp = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            mockSaService
                .Setup(x => x.ExecutarApiExtGerarSaidaSenha(msgOut))
                .ReturnsAsync(retornoSaida);

            mockOperador
                .Setup(x => x.MontarChamadaSPX(It.IsAny<ParametrosSPX>()))
                .ReturnsAsync(retornoSPX);

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlp.Object);

            var service = new ProcessadorSenhaSilabica(provider.Object);

            // Act
            var result = await service.ExecutarFluxo740(msgOut, dadosConcatenados);

            // Assert
            Assert.Equal("RETORNO_PARAMETRO", result);
            mockSaService.Verify(x => x.ExecutarApiExtGerarSaidaSenha(msgOut), Times.Once);
            mockOperador.Verify(x => x.MontarChamadaSPX(It.IsAny<ParametrosSPX>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarFluxo741_DeveExecutarComSucessoERetornarParametro()
        {
            // Arrange
            var msgOut = "x|1|AG123|C123456|20240707|senha123|seqbtn123|outros|dados|MSG_IN|MSG_OUT";
            var dadosConcatenados = "dados|concatenados|fluxo741";

            var mockSenhaAlfaService = new Mock<ISAServicePort>();
            var mockSpaOperadorService = new Mock<ISPAOperadorService>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockOtlp = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            var responseSenha = new TestarSenhaResponse
            {
                returnCode = (int)EnumStatus.SUCESSO,
                validationResult = 1234
            };

            mockSenhaAlfaService
                .Setup(x => x.ExecutarApiExtTestarSenha(msgOut))
                .ReturnsAsync(responseSenha);

            var retornoSPX = new List<RetornoSPX>
            {
                new() { vchParam = "RETORNO_741" }
            };

            mockSpaOperadorService
                .Setup(x => x.MontarChamadaSPX(It.IsAny<ParametrosSPX>()))
                .ReturnsAsync(retornoSPX);

            mockSpaOperadorService
                .Setup(x => x.GetTransacaoAtiva())
                .Returns(new SPATransacao { Codigo = 741 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockSpaOperadorService.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSenhaAlfaService.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlp.Object);

            var service = new ProcessadorSenhaSilabica(mockProvider.Object);

            // Act
            var result = await service.ExecutarFluxo741(msgOut, dadosConcatenados);

            // Assert
            Assert.Equal("RETORNO_741", result);
            mockSenhaAlfaService.Verify(x => x.ExecutarApiExtTestarSenha(msgOut), Times.Once);
            mockSpaOperadorService.Verify(x => x.MontarChamadaSPX(It.IsAny<ParametrosSPX>()), Times.Once);
        }

        #endregion

        #region ProcessarTransacaoTests

        private static string MontarMensagemTexto(int transacao, EnumMetodoAcao metodo, EnumSPASituacaoTransacao situacao)
        {
            var tipo = "TRAN";
            var codigo = transacao.ToString("D8");
            var metodoChar = ((int)metodo).ToString();
            var situacaoChar = ((int)situacao).ToString();
            var sep = ((char)11).ToString();
            var header = $"{tipo}{codigo}{metodoChar}{situacaoChar} ";

            var dadosSpa = new List<string>();
            for (int i = 0; i < 24; i++)
                dadosSpa.Add($"DADO{i + 1}");

            return header + sep + string.Join(sep, dadosSpa);
        }

        [Fact]
        public async Task ProcessarTransacao_DeveExecutarFluxoConfirmarComSucesso()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Arrange
            var retornoSpa = "RETORNO_SPA";
            var cabecalho = new tSPACabecalho { cracha = 123456, timeOut = 600 };

            var mensagemTexto = MontarMensagemTexto(741, EnumMetodoAcao.ACAO_CONFIRMAR, EnumSPASituacaoTransacao.Executada);
            var transacao = new TransacaoSenhaSilabica(mensagemTexto)
            {
                CabecalhoSPA = cabecalho
            };

            var mensagem = new MAgentMensagem(mensagemTexto.AsSpan());
            typeof(TransacaoSenhaSilabica)
                .GetField("_mAgentMessageIn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(transacao, mensagem);

            var spaParametro = new SPAParametro(0, "VALOR");
            var spaTransacao = new SPATransacao
            {
                Codigo = 741,
                ListParametros = new List<SPAParametro> { spaParametro },
                ParametrosFixos = new SPAParametrosFixos
                {
                    Acao = (int)EnumAcao.ACAO_CONFIRMAR | (int)EnumAcao.ACAO_CANCELAR
                },
                TransacaoGravaLog = true
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockOtlpService = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            mockOperador.Setup(x => x.IniciarTransacao(It.IsAny<TransacaoSenhaSilabica>(), It.IsAny<string[]>()))
                .ReturnsAsync(new BaseReturn());
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(spaTransacao);
            mockOperador.Setup(x => x.RecuperarAcao()).Returns(EnumAcao.ACAO_CONFIRMAR);
            mockOperador.Setup(x => x.GetRetornoSPA()).Returns(retornoSpa);
            mockOperador.Setup(x => x.RecuperaSituacao()).Returns(Task.CompletedTask);
            mockOperador.Setup(x => x.ConfirmarTransacao()).Returns(Task.CompletedTask);

            mockTcpClient.Setup(x => x.SendResponse(It.IsAny<tSPACabecalho>(), It.IsAny<byte[]>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);

            var processor = new ProcessadorSenhaSilabica(provider.Object);
            var activity = new Activity("test");
            activity.Start();

            // Act
            await processor.ProcessarTransacao(transacao, activity, CancellationToken.None);

            // Assert
            mockOperador.Verify(x => x.IniciarTransacao(It.IsAny<TransacaoSenhaSilabica>(), It.IsAny<string[]>()), Times.Once);
            mockOperador.Verify(x => x.RecuperaSituacao(), Times.Once);
            mockOperador.Verify(x => x.ConfirmarTransacao(), Times.Once);
            mockTcpClient.Verify(x => x.SendResponse(It.IsAny<tSPACabecalho>(), It.IsAny<byte[]>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarTransacao_QuandoActivityNula_DeveExecutarComSucesso()
        {
            // Arrange
            var mensagemTexto = "TRAN0000074010\v00000035\v\vW3-DEV-14\v3\v740\vR\v25/06/2025 00:00:00\v15\v1\v48\v48\v01/01/1900 00:00:00\v0\v1\v\v0\v0\v19\v0\v1\v3\v25/06/2025 00:00:00\v0\v00000035||W3-DEV-14|3|740|P|01/01/1900 00:00:00|15|1|0|0|01/01/1900 00:00:00|15|1||0|1|0|0|0|3|25/06/2025 00:00:00|0|||6372332000000012=19126061270000000000||0|0|||||||||||||||||0||I||||||||||||||||||||||||0||||||\v\v6372332000000012=19126061270000000000\v6372332000000012\v0\v1\v0\v0\v0\v0\v0\v0000000000\v\v0\v0\v0000\v0000\v \v0\v\v0\v\v0\v0\vI\v\v\v\v0\v \v \v \v \v\v \v \v\v\v\v\v\v0\v \v\v\v\v \v0\v0\v\v0\v\v \v\v";

            var transacao = new TransacaoSenhaSilabica(mensagemTexto)
            {
                CabecalhoSPA = new tSPACabecalho { cracha = 123456, timeOut = 600 }
            };

            var spaTransacao = new SPATransacao
            {
                Codigo = 741,
                ListParametros = new List<SPAParametro>(),
                ParametrosFixos = new SPAParametrosFixos { Acao = (int)EnumAcao.ACAO_CONFIRMAR },
                TransacaoGravaLog = true
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockOtlpService = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(spaTransacao);
            mockOperador.Setup(x => x.RecuperarAcao()).Returns(EnumAcao.ACAO_CONFIRMAR);
            mockOperador.Setup(x => x.IniciarTransacao(It.IsAny<TransacaoSenhaSilabica>(), It.IsAny<string[]>()))
                .ReturnsAsync(new BaseReturn());
            mockOperador.Setup(x => x.RecuperaSituacao()).Returns(Task.CompletedTask);
            mockOperador.Setup(x => x.ConfirmarTransacao()).Returns(Task.CompletedTask);

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);

            var processor = new ProcessadorSenhaSilabica(provider.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
                processor.ProcessarTransacao(transacao, parentActivity: null!, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessarTransacao_QuandoOperationCanceledException_DeveTratarExcecao()
        {
            // Arrange
            var mensagemTexto = MontarMensagemTexto(741, EnumMetodoAcao.ACAO_EXECUTAR, EnumSPASituacaoTransacao.Executada);
            var transacao = new TransacaoSenhaSilabica(mensagemTexto)
            {
                CabecalhoSPA = new tSPACabecalho { cracha = 123456, timeOut = 600 }
            };

            var mensagem = new MAgentMensagem(mensagemTexto.AsSpan());
            typeof(TransacaoSenhaSilabica)
                .GetField("_mAgentMessageIn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(transacao, mensagem);

            var mockOperador = new Mock<ISPAOperadorService>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockOtlpService = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Throws(new OperationCanceledException());

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);

            var processor = new ProcessadorSenhaSilabica(provider.Object);

            var activity = new Activity("teste");
            activity.Start();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                processor.ProcessarTransacao(transacao, activity, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessarTransacao_QuandoExceptionGenerica_DeveTratarExcecao()
        {
            // Arrange
            var mensagemTexto = MontarMensagemTexto(741, EnumMetodoAcao.ACAO_EXECUTAR, EnumSPASituacaoTransacao.Executada);
            var transacao = new TransacaoSenhaSilabica(mensagemTexto)
            {
                CabecalhoSPA = new tSPACabecalho { cracha = 123456, timeOut = 600 }
            };

            var mensagem = new MAgentMensagem(mensagemTexto.AsSpan());
            typeof(TransacaoSenhaSilabica)
                .GetField("_mAgentMessageIn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(transacao, mensagem);

            var mockOperador = new Mock<ISPAOperadorService>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockOtlpService = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Throws(new Exception("erro generico"));

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);

            var processor = new ProcessadorSenhaSilabica(provider.Object);
            var activity = new Activity("teste");
            activity.Start();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                processor.ProcessarTransacao(transacao, activity, CancellationToken.None));

            Assert.Equal("erro generico", ex.Message);
        }

        #endregion

        #region ResponderTransacaoTests

        [Fact]
        public async Task HandleSPARetorno_DeveExecutarResponderTransacaoComSucesso()
        {
            // Arrange
            var retorno = "Mensagem de retorno";
            var cabecalho = new tSPACabecalho
            {
                cracha = 123456,
                timeOut = 600,
                tamanhoCab = 10
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockOtlpService = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            mockTcpClient.Setup(x =>
                x.SendResponse(It.IsAny<tSPACabecalho>(), It.IsAny<byte[]>(), It.IsAny<int>())
            ).Returns(Task.CompletedTask).Verifiable();

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);

            var service = new ProcessadorSenhaSilabica(provider.Object);

            // Act
            await service.HandleSPARetorno(EnumMetodoAcao.ACAO_EXECUTAR, retorno, cabecalho);

            // Assert
            mockTcpClient.Verify(x =>
                x.SendResponse(It.IsAny<tSPACabecalho>(), It.IsAny<byte[]>(), It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleSPARetorno_DeveCapturarExcecaoNoResponderTransacao()
        {
            // Arrange
            var retorno = "Mensagem de erro";
            var cabecalho = new tSPACabecalho
            {
                cracha = 123456,
                timeOut = 600,
                tamanhoCab = 10
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockOtlpService = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            mockTcpClient.Setup(x =>
                x.SendResponse(It.IsAny<tSPACabecalho>(), It.IsAny<byte[]>(), It.IsAny<int>())
            ).ThrowsAsync(new Exception("erro simulado"));

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlpService.Object);

            var service = new ProcessadorSenhaSilabica(provider.Object);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() =>
                service.HandleSPARetorno(EnumMetodoAcao.ACAO_EXECUTAR, retorno, cabecalho));

            Assert.Null(exception);

            mockTcpClient.Verify(x =>
                x.SendResponse(It.IsAny<tSPACabecalho>(), It.IsAny<byte[]>(), It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region ExecuteAcaoTests

        [Fact]
        public async Task ExecuteAcao_QuandoValidar_DeveChamarValidarTransacao()
        {
            // Arrange
            var mockOperador = new Mock<ISPAOperadorService>();
            var service = CreateService(mockOperador);

            // Act
            await service.ExecuteAcao(EnumMetodoAcao.ACAO_VALIDAR);

            // Assert
            mockOperador.Verify(x => x.ValidarTransacao(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAcao_QuandoConfirmar_DeveChamarConfirmarTransacao()
        {
            var mockOperador = new Mock<ISPAOperadorService>();
            var service = CreateService(mockOperador);

            await service.ExecuteAcao(EnumMetodoAcao.ACAO_CONFIRMAR);

            mockOperador.Verify(x => x.ConfirmarTransacao(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAcao_QuandoCancelar_DeveChamarCancelarTransacao()
        {
            var mockOperador = new Mock<ISPAOperadorService>();
            var service = CreateService(mockOperador);

            await service.ExecuteAcao(EnumMetodoAcao.ACAO_CANCELAR);

            mockOperador.Verify(x => x.CancelarTransacao(), Times.Once);
        }

        #endregion

        #region ExecutarTests

        [Fact]
        public async Task Executar_QuandoCodigo740_DeveExecutarFluxo740()
        {
            // Arrange
            var transacao = new SPATransacao
            {
                Codigo = 740,
                ListParametros = new List<SPAParametro>
                {
                    new(new SqlParameter { ParameterName = "@pvchMsgOUT", Value = "MSG_OUT" }, 1)
                }
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(transacao);
            mockOperador.Setup(x => x.ExecutarTransacao()).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockOtlp = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlp.Object);

            var service = new Mock<ProcessadorSenhaSilabica>(provider.Object) { CallBase = true };

            service.Setup(x => x.ExecutarFluxo740("MSG_OUT", It.IsAny<string>()))
                   .ReturnsAsync("RET_740");

            // Act
            var result = await service.Object.Executar();

            // Assert
            Assert.Equal("RET_740", result);
        }

        [Fact]
        public async Task Executar_QuandoCodigo741_DeveExecutarFluxo740()
        {
            // Arrange
            var transacao = new SPATransacao
            {
                Codigo = 741,
                ListParametros = new List<SPAParametro>
                {
                    new(new SqlParameter { ParameterName = "@pvchMsgOUT", Value = "MSG_OUT" }, 1)
                }
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(transacao);
            mockOperador.Setup(x => x.ExecutarTransacao()).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<ProcessadorSenhaSilabica>>();
            var mockOptions = new Mock<IOptions<GCSrvSettings>>();
            var mockSaService = new Mock<ISAServicePort>();
            var mockTcpClient = new Mock<ISPATcpClientServicePort>();
            var mockOtlp = new Mock<IOtlpServicePort>();

            mockOptions.Setup(x => x.Value).Returns(new GCSrvSettings());

            var provider = new Mock<IServiceProvider>();
            provider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            provider.Setup(x => x.GetService(typeof(ILogger<ProcessadorSenhaSilabica>))).Returns(mockLogger.Object);
            provider.Setup(x => x.GetService(typeof(IOptions<GCSrvSettings>))).Returns(mockOptions.Object);
            provider.Setup(x => x.GetService(typeof(ISAServicePort))).Returns(mockSaService.Object);
            provider.Setup(x => x.GetService(typeof(ISPATcpClientServicePort))).Returns(mockTcpClient.Object);
            provider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(mockOtlp.Object);

            var service = new Mock<ProcessadorSenhaSilabica>(provider.Object) { CallBase = true };

            service.Setup(x => x.ExecutarFluxo741("MSG_OUT", It.IsAny<string>()))
                   .ReturnsAsync("RET_740");

            // Act
            var result = await service.Object.Executar();

            // Assert
            Assert.Equal("RET_740", result);
        }

        [Fact]
        public async Task Executar_QuandoCodigoInvalido_DeveLancarExcecao()
        {
            var transacao = new SPATransacao
            {
                Codigo = 999, // código inválido
                ListParametros = new List<SPAParametro>
        {
            new(new SqlParameter { ParameterName = "@pvchMsgOUT", Value = "MSG_OUT" }, 1)
        }
            };

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(transacao);
            mockOperador.Setup(x => x.ExecutarTransacao()).Returns(Task.CompletedTask);

            var service = CreateService(mockOperador);

            var ex = await Assert.ThrowsAsync<SPAException>(() => service.Executar());

            Assert.Contains("Código de transação inválido", ex.Message);
        }

        [Fact]
        public async Task Executar_QuandoExecutarTransacaoLancarExcecao_DeveTratarEReLancar()
        {
            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740, ListParametros = new List<SPAParametro>() });
            mockOperador.Setup(x => x.ExecutarTransacao()).ThrowsAsync(new Exception("erro"));

            var service = CreateService(mockOperador);

            var ex = await Assert.ThrowsAsync<SPAException>(() => service.Executar());

            Assert.Contains("erro", ex.ToString());
        }

        #endregion
    }
}
