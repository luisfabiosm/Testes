using Adapters.Outbound.DBAdapter.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Adapters.Outbound.DBAdapter;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using Domain.Core.Exceptions;
using Domain.Core.Models.SPA;
using System.Data.SqlClient;
using Domain.Core.Enums;
using System.Reflection;
using Domain.Core.Base;
using System.Data;
using Moq.Dapper;
using Dapper;
using Moq;

namespace Processador.Adapters.Outbound.DBAdapter
{
    public class SPARepositoryTests
    {
        private readonly SPARepository _repository;
        private readonly Mock<IDBAdapterConnection> _mockDbAdapter = new();
        private readonly Mock<IOptions<DBSettings>> _mockSettings = new();
        private readonly Mock<ILogger<SPARepository>> _mockLogger = new();
        private readonly Mock<IServiceProvider> _mockServiceProvider = new();
        private readonly Mock<IOtlpServicePort> _mockOtlpService = new();

        public SPARepositoryTests()
        {
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<SPARepository>)))
                                .Returns(_mockLogger.Object);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IDBAdapterConnection)))
                                .Returns(_mockDbAdapter.Object);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<DBSettings>)))
                                .Returns(_mockSettings.Object);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IOtlpServicePort)))
                                .Returns(_mockOtlpService.Object);

            _repository = new SPARepository(_mockServiceProvider.Object);
        }

        #region IniciarSPATransacaoTests

        [Fact]
        public async Task IniciarSPATransacao_DevePreencherParametrosCorretamente()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                Codigo = 1234,
                ListParametros = new List<SPAParametro>()
            };

            var connMock = new Mock<IDbConnection>();

            _mockDbAdapter.Setup(d => d.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task<bool>>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<IDbConnection, Task<bool>>, CancellationToken>((func, _) =>
                {
                    return func(connMock.Object).ContinueWith(task =>
                    {
                        spaTransacao.Descricao = "Teste";
                        spaTransacao.DataContabil = DateTime.Now;
                        spaTransacao.ProcedureSQL = "sp_fake";
                        spaTransacao.NumeroParametros = 1;
                        spaTransacao.FormatoValores = "1.00";
                        spaTransacao.Timeout = 30;
                        spaTransacao.TransacaoGravaLog = true;

                        spaTransacao.ListParametros.Add(new SPAParametro(new SqlParameter("@p1", SqlDbType.VarChar), 0));

                        return true;
                    });
                });

            var fakeSqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True");
            typeof(SPARepository)
                .GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_repository, fakeSqlConnection);

            // Act
            await _repository.IniciarSPATransacao(1, 2, spaTransacao);

            // Assert
            Assert.Equal("Teste", spaTransacao.Descricao);
            Assert.Equal("sp_fake", spaTransacao.ProcedureSQL);
            Assert.NotNull(spaTransacao.ListParametros);
            Assert.NotEmpty(spaTransacao.ListParametros);
        }

        [Fact]
        public async Task IniciarSPATransacao_DeveExecutarComSucesso()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                Codigo = 1234,
                ListParametros = new List<SPAParametro>(),
                Descricao = string.Empty
            };

            int agencia = 1;
            int posto = 2;

            _mockDbAdapter.Setup(d => d.GetConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<IDbConnection>().Object);

            _mockDbAdapter.Setup(db =>
                db.ExecuteWithRetryAsync(
                    It.IsAny<Func<IDbConnection, Task<bool>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns<Func<IDbConnection, Task<bool>>, CancellationToken>((operation, token) =>
                {
                    return operation(new Mock<IDbConnection>().Object).ContinueWith(task =>
                    {
                        spaTransacao.Descricao = "Teste";
                        spaTransacao.DataContabil = DateTime.Now;
                        spaTransacao.ProcedureSQL = "sp_fake";
                        spaTransacao.NumeroParametros = 2;
                        spaTransacao.FormatoValores = "1.00";
                        spaTransacao.Timeout = 30;
                        spaTransacao.TransacaoGravaLog = true;
                        spaTransacao.ListParametros = new List<SPAParametro>();

                        return true;
                    });
                });

            // Act
            await _repository.IniciarSPATransacao(agencia, posto, spaTransacao);

            // Assert
            Assert.NotNull(spaTransacao);
            Assert.Equal("Teste", spaTransacao.Descricao);
            Assert.Equal("sp_fake", spaTransacao.ProcedureSQL);
            Assert.True(spaTransacao.TransacaoGravaLog);
        }

        #endregion

        #region ExecutaDBTests

        [Fact]
        public async Task ExecutaDB_DeveExecutarComSucesso()
        {
            // Arrange
            var spaParametro = new SPAParametro(
                new SqlParameter("@param1", SqlDbType.VarChar)
                {
                    Direction = ParameterDirection.Input,
                    Value = "valor"
                }, 1
            );

            var spaTransacao = new SPATransacao
            {
                Codigo = 1234,
                Timeout = 5,
                ProcedureSQL = "SELECT 1",
                ParametrosFixos = new SPAParametrosFixos
                {
                    Acao = (int)EnumAcao.ACAO_EXECUTAR,
                    DataContabil = "01/01/2024 00:00:00",
                    TipoTransacao = "T",
                    NSU1 = 123,
                    Agencia1 = 1,
                    Posto1 = 2
                },

                ListParametros = [spaParametro]
            };

            _mockSettings.Setup(s => s.Value).Returns(new DBSettings
            {
                IsTraceExecActive = false
            });

            var fakeConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True");

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<BaseReturn>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IDbConnection, Task<BaseReturn>>, CancellationToken>((func, token) =>
            {
                return Task.FromResult(new BaseReturn());
            });

            // Act
            var result = await _repository.ExecutaDB(spaTransacao);

            // Assert
            Assert.IsType<BaseReturn>(result);
            Assert.Equal(1234, spaTransacao.Codigo);
            Assert.Equal("SELECT 1", spaTransacao.ProcedureSQL);
        }

        [Fact]
        public async Task ExecutaDB_DeveLancarExcecao_SeFalharNaExecucao()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                Codigo = 1234,
                Timeout = 5,
                ProcedureSQL = "sp_teste_falha",
                ParametrosFixos = new SPAParametrosFixos
                {
                    Acao = (int)EnumAcao.ACAO_EXECUTAR,
                    DataContabil = "01/01/2024 00:00:00",
                    TipoTransacao = "T",
                    NSU1 = 123,
                    Agencia1 = 1,
                    Posto1 = 2
                },
                ListParametros = new List<SPAParametro>()
            };

            _mockSettings.Setup(s => s.Value).Returns(new DBSettings
            {
                IsTraceExecActive = false
            });

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<BaseReturn>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha simulada"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<SPAException>(() =>
                _repository.ExecutaDB(spaTransacao).AsTask());

            Assert.Contains("Falha simulada", ex.Message);
        }

        [Fact]
        public async Task ExecutaDB_DeveExecutarComTraceExecActive()
        {
            // Arrange
            var spaParametro = new SPAParametro(
                new SqlParameter("@param1", SqlDbType.VarChar)
                {
                    Direction = ParameterDirection.Input,
                    Value = "valor"
                }, 1
            );

            var spaTransacao = new SPATransacao
            {
                Codigo = 1234,
                Timeout = 5,
                ProcedureSQL = "SELECT 1",
                ParametrosFixos = new SPAParametrosFixos
                {
                    Acao = (int)EnumAcao.ACAO_EXECUTAR,
                    DataContabil = "01/01/2024 00:00:00",
                    TipoTransacao = "T",
                    NSU1 = 123,
                    Agencia1 = 1,
                    Posto1 = 2
                },
                ListParametros = [spaParametro]
            };

            _mockSettings.Setup(s => s.Value).Returns(new DBSettings
            {
                IsTraceExecActive = true
            });

            var fakeConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True");

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<BaseReturn>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IDbConnection, Task<BaseReturn>>, CancellationToken>((func, token) =>
            {
                return Task.FromResult(new BaseReturn());
            });

            // Act
            var result = await _repository.ExecutaDB(spaTransacao);

            // Assert
            Assert.IsType<BaseReturn>(result);
        }

        #endregion

        #region RecuperarSituacaoTests

        [Fact]
        public async Task RecuperarSituacao_DeveRetornarSituacaoCorreta()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                Descricao = "Descrição",
                ParametrosFixos = new SPAParametrosFixos
                {
                    Agencia1 = 123,
                    Posto1 = 456,
                    NSU1 = 789,
                    DataContabil = "01/01/2024 00:00:00"
                }
            };

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<EnumSPASituacaoTransacao>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IDbConnection, Task<EnumSPASituacaoTransacao>>, CancellationToken>((func, token) =>
            {
                return func(new Mock<IDbConnection>().Object);
            });

            var situacaoEsperada = EnumSPASituacaoTransacao.Executada; 

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<EnumSPASituacaoTransacao>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(situacaoEsperada);

            // Act
            var result = await _repository.RecuperarSituacao(spaTransacao);

            // Assert
            Assert.Equal(situacaoEsperada, result);
        }

        [Fact]
        public async Task RecuperarSituacao_DeveLancarSPAException_SeFalhar()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                ParametrosFixos = new SPAParametrosFixos
                {
                    Agencia1 = 1,
                    Posto1 = 1,
                    NSU1 = 999,
                    DataContabil = "01/01/2024 00:00:00"
                }
            };

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<EnumSPASituacaoTransacao>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulação de erro"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<SPAException>(() =>
                _repository.RecuperarSituacao(spaTransacao).AsTask());

            Assert.Contains("Simulação de erro", ex.Message);
        }

        #endregion

        #region GravarParametroLogTests

        [Fact]
        public async Task GravarParametroLog_DeveExecutarComSucesso()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                ParametrosFixos = new SPAParametrosFixos
                {
                    Agencia1 = 1,
                    Posto1 = 2,
                    NSU1 = 999,
                    TipoTransacao = "T",
                    DataContabil = "01/01/2024 00:00:00"
                }
            };

            var logParams = new [] { "valor1", "valor2" };

            var mockSPA = new Mock<SPATransacao>();
            mockSPA.Object.ParametrosFixos = spaTransacao.ParametrosFixos;
            mockSPA.Setup(x => x.ParamsToLog()).Returns(logParams);

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<IDbConnection, Task<bool>>, CancellationToken>((func, _) =>
            {
                return func(new Mock<IDbConnection>().Object);
            });

            // Act
            await _repository.GravarParametroLog(mockSPA.Object);

            // Assert
            _mockDbAdapter.Verify(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<bool>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GravarParametroLog_NaoDeveLancarExcecao_SeOcorrerErro()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                ParametrosFixos = new SPAParametrosFixos
                {
                    Agencia1 = 1,
                    Posto1 = 2,
                    NSU1 = 999,
                    TipoTransacao = "T",
                    DataContabil = "01/01/2024 00:00:00"
                }
            };

            _mockDbAdapter.Setup(db => db.ExecuteWithRetryAsync(
                It.IsAny<Func<IDbConnection, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro simulado"));

            // Act & Assert
            var exception = await Record.ExceptionAsync(() =>
                _repository.GravarParametroLog(spaTransacao).AsTask());

            Assert.Null(exception); // Não deve propagar exceção
        }

        #endregion

        #region ExecutarSPXTests

        [Fact]
        public async Task ExecutarSPXIdentificaCartao_DeveRetornarResultadosComSucesso()
        {
            // Arrange
            var parametros = new ParametrosSPX("dados", "resposta", "seq1", "seq2", "seq3", "grupo", 123);

            var resultadoEsperado = new List<RetornoSPX>
            {
                new()
            };

            var mockDbConnection = new Mock<IDbConnection>();

            mockDbConnection.SetupDapperAsync(c =>
                c.QueryAsync<RetornoSPX>(
                    "dbo.spx_IdentificaCartao2",
                    It.IsAny<object>(),
                    null,
                    null,
                    CommandType.StoredProcedure))
                .ReturnsAsync(resultadoEsperado);

            var sessionField = typeof(SPARepository).GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance);
            sessionField!.SetValue(_repository, mockDbConnection.Object);

            // Act
            var result = await _repository.ExecutarSPXIdentificaCartao(parametros);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Collection(result, item => { Assert.Null(item.vchParam); });
        }

        [Fact]
        public async Task ExecutarSPXSenhaSilabica_DeveRetornarResultadosComSucesso()
        {
            // Arrange
            var parametros = new ParametrosSPX("dados", "msgIN", "msgOUT", 999);

            var resultadoEsperado = new List<RetornoSPX>
            {
                new RetornoSPX()
            };

            var mockDbConnection = new Mock<IDbConnection>();

            // Simula o QueryAsync do Dapper
            mockDbConnection.SetupDapperAsync(c =>
                c.QueryAsync<RetornoSPX>(
                    "dbo.spx_SenhaSilabica2",
                    It.IsAny<object>(),
                    null,
                    null,
                    CommandType.StoredProcedure))
                .ReturnsAsync(resultadoEsperado);

            // Injeta a conexão mockada no campo _session
            var sessionField = typeof(SPARepository).GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance);
            sessionField!.SetValue(_repository, mockDbConnection.Object);

            // Act
            var result = await _repository.ExecutarSPXSenhaSilabica(parametros);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(resultadoEsperado.First().vchParam, result.First().vchParam);
        }

        #endregion

        [Fact]
        public void SPExec_DeveGerarComandoExecuteCorretamente()
        {
            // Arrange
            var spaTransacao = new SPATransacao
            {
                ProcedureSQL = "sp_MinhaProcedure"
            };

            var command = new SqlCommand();
            command.Parameters.Add(new SqlParameter("@param1", SqlDbType.VarChar)
            {
                Value = "valor1"
            });
            command.Parameters.Add(new SqlParameter("@param2", SqlDbType.Int)
            {
                Value = 123
            });

            var method = typeof(SPARepository).GetMethod("SPExec", BindingFlags.NonPublic | BindingFlags.Instance);
            var parameters = new object[] { spaTransacao, command };

            // Act
            var resultado = method!.Invoke(_repository, parameters) as string;

            // Assert
            Assert.NotNull(resultado);
            Assert.Contains("EXECUTE sp_MinhaProcedure", resultado);
            Assert.Contains("@param1= 'valor1'", resultado);
            Assert.Contains("@param2= 123", resultado);
        }
    }
}
