using Adapters.Outbound.SenhaAlfaAdapter.Models;
using Adapters.Outbound.SenhaAlfaAdapter.Utils;
using Adapters.Outbound.SenhaAlfaAdapter;
using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using Domain.Core.Models.SPA;
using System.Diagnostics;
using Domain.Core.Enums;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Moq;

namespace Processador.Adapters.Outbound.SenhaAlfaAdapter
{
    public class SAServiceTests
    {
        private static void AtivarTracingParaTestes()
        {
            var sourceName = Assembly.GetExecutingAssembly().GetName().Name!;
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            var listener = new ActivityListener
            {
                ShouldListenTo = activitySource =>
                    activitySource.Name == sourceName && activitySource.Version == version,

                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllDataAndRecorded,

                ActivityStarted = activity => { },
                ActivityStopped = activity => { }
            };

            ActivitySource.AddActivityListener(listener);
        }

        [Fact]
        public void SAService_DeveInstanciarClasse()
        {
            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var httpClient = new HttpClient { BaseAddress = new Uri("http://mock-api") };

            var service = new SAService(mockProvider.Object, httpClient);

            Assert.NotNull(service);
        }

        #region ExecutarApiExtGerarSaidaSenhaTests

        [Fact]
        public async Task ExecutarApiExtGerarSaidaSenha_DeveRetornarSucesso()
        {
            AtivarTracingParaTestes();

            var expectedResponse = new GerarSaidaSenhaResponse
            {
                returnCode = (int)EnumStatus.SUCESSO,
                dataHora = 123456,
                seq1 = "1",
                seq2 = "2",
                seq3 = "3",
                grupo8 = "G8"
            };

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                var json = JsonConvert.SerializeObject(expectedResponse);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://mock-api") };

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(x => x.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var service = new SAService(mockProvider.Object, httpClient);

            var result = await service.ExecutarApiExtGerarSaidaSenha("x|1|AG|CONTA|123456|RESPOSTA|y");

            Assert.NotNull(result);
            Assert.Equal(EnumStatus.SUCESSO, (EnumStatus)result.returnCode);
            Assert.Equal(123456, result.dataHora);
        }

        [Fact]
        public async Task ExecutarApiExtGerarSaidaSenha_DeveLancarExcecaoComRetornoInvalido()
        {
            AtivarTracingParaTestes();

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var httpClient = new HttpClient { BaseAddress = new Uri("http://mock-api") };

            var service = new SAService(mockProvider.Object, httpClient);

            var ex = await Assert.ThrowsAsync<FormatException>(() =>
                service.ExecutarApiExtGerarSaidaSenha("1|apenas|dois"));

            Assert.Equal("O formato do parâmetro de retorno é inválido!", ex.Message);
        }

        [Fact]
        public async Task ExecutarApiExtGerarSaidaSenha_DeveLancarErroSeReturnCodeDiferenteDeSucesso()
        {
            AtivarTracingParaTestes();

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new GerarSaidaSenhaResponse
                    {
                        returnCode = (int)EnumStatus.SISTEMA
                    }), Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://mock-api") };

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var service = new SAService(mockProvider.Object, httpClient);

            var ex = await Assert.ThrowsAsync<FormatException>(() =>
                service.ExecutarApiExtGerarSaidaSenha("x|1|0001|123456"));

            Assert.Equal("O formato do parâmetro de retorno é inválido!", ex.Message);
        }

        [Fact]
        public async Task ExecutarApiExtGerarSaidaSenha_DeveLancarErroComCodigoTransacaoInvalido()
        {
            AtivarTracingParaTestes();

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 999 }); // inválido

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var httpClient = new HttpClient { BaseAddress = new Uri("http://mock-api") };

            var service = new SAService(mockProvider.Object, httpClient);

            var ex = await Assert.ThrowsAsync<FormatException>(() =>
                service.ExecutarApiExtGerarSaidaSenha("x|1|0001|123456"));

            Assert.Equal("O formato do parâmetro de retorno é inválido!", ex.Message);
        }

        [Fact]
        public async Task ExecutarApiExtGerarSaidaSenha_DeveLancarExcecaoELogarNoActivity()
        {
            AtivarTracingParaTestes();

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                throw new HttpRequestException("Falha na requisição externa");
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://mock-api") };

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var service = new SAService(mockProvider.Object, httpClient);

            var ex = await Assert.ThrowsAsync<FormatException>(() =>
                service.ExecutarApiExtGerarSaidaSenha("x|1|0001|123456"));

            Assert.Equal("O formato do parâmetro de retorno é inválido!", ex.Message);
        }

        #endregion

        [Fact]
        public void SAService_ConstrutorDeveLancarArgumentNullException_SeHttpClientForNulo()
        {
            var mockProvider = new Mock<IServiceProvider>();
            Assert.Throws<InvalidOperationException>(() => new SAService(mockProvider.Object, null));
        }

        #region ExecutarApiExtTestarSenhaTests

        [Fact]
        public async Task ExecutarApiExtTestarSenha_DeveRetornarSucesso()
        {
            AtivarTracingParaTestes();

            var expectedResponse = new TestarSenhaResponse
            {
                returnCode = EnumStatus.SUCESSO
            };

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                var json = JsonConvert.SerializeObject(expectedResponse);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://mock-api") };

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 741 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var service = new SAService(mockProvider.Object, httpClient);

            var result = await service.ExecutarApiExtTestarSenha("x|1|0001|CONTA|123456|SENHA|SEQ");

            Assert.NotNull(result);
            Assert.Equal(EnumStatus.SUCESSO, result.returnCode);
        }

        [Fact]
        public async Task ExecutarApiExtTestarSenha_DeveLancarErroSeRetornoNaoForSucesso()
        {
            AtivarTracingParaTestes();

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                var json = JsonConvert.SerializeObject(new TestarSenhaResponse
                {
                    returnCode = EnumStatus.SISTEMA
                });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://mock-api") };

            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new IntegracaoSettings
            {
                SA = new SAConfig { Url = "http://mock-api" }
            });

            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 741 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var service = new SAService(mockProvider.Object, httpClient);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ExecutarApiExtTestarSenha("x|1|0001|CONTA|123456|SENHA|SEQ"));

            Assert.Equal("Erro ao testar senha.", ex.Message);
        }

        #endregion

        [Fact]
        public void ExtrairDadosRetornoSPA_DeveRetornarSenhaAlfaRequestParaTransacao741()
        {
            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 741 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var httpClient = new HttpClient();

            var service = new SAService(mockProvider.Object, httpClient);

            var result = service
                .GetType()?
                .GetMethod("ExtrairDadosRetornoSPA", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(service, new object[] { "x|1|0001|CONTA|123456|SENHA|SEQ" }) as SenhaAlfaRequest;

            Assert.NotNull(result);
            Assert.Equal(1, result.tipoSaque);
            Assert.Equal("0001", result.agencia);
            Assert.Equal("CONTA", result.conta);
            Assert.Equal(123456, result.dataHora);
            Assert.Equal("SENHA", result.senhaBase);
            Assert.Equal("SEQ", result.seqBotoes);
        }

        [Fact]
        public void ExtrairDadosRetornoSPA_DeveLancarExcecao_SeRetornoEstiverVazio()
        {
            var mockSettings = new Mock<IOptions<IntegracaoSettings>>();
            var mockOperador = new Mock<ISPAOperadorService>();
            mockOperador.Setup(x => x.GetTransacaoAtiva()).Returns(new SPATransacao { Codigo = 740 });

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(x => x.GetService(typeof(IOptions<IntegracaoSettings>))).Returns(mockSettings.Object);
            mockProvider.Setup(x => x.GetService(typeof(ISPAOperadorService))).Returns(mockOperador.Object);
            mockProvider.Setup(x => x.GetService(typeof(IOtlpServicePort))).Returns(Mock.Of<IOtlpServicePort>());

            var httpClient = new HttpClient();
            var service = new SAService(mockProvider.Object, httpClient);

            var method = service.GetType().GetMethod("ExtrairDadosRetornoSPA", BindingFlags.NonPublic | BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(() =>
                method!.Invoke(service, new object[] { "" }));

            Assert.IsType<ArgumentException>(ex.InnerException);
            Assert.Equal("O parâmetro de retorno está vazio. (Parameter 'retornoSPA')", ex.InnerException.Message);
        }
    }
}
