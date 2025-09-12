using Adapters.Outbound.Database.SQL;
using Domain.Core.Ports.Outbound;
using Domain.Core.Settings;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Domain.Core.Models.JDPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace pix_pagador_testes.Adapters.Outbound.Database.SQL
{
    public class SPARepositoryTest : IDisposable
    {
        private readonly SPARepository _testClass;
        private readonly Mock<ISQLConnectionAdapter> _mockSQLConnectionAdapter;
        private readonly Mock<ILoggingAdapter> _mockLoggingAdapter;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IOptions<DBSettings>> _mockDBSettings;

        public SPARepositoryTest()
        {
            _mockSQLConnectionAdapter = new Mock<ISQLConnectionAdapter>();
            _mockLoggingAdapter = new Mock<ILoggingAdapter>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockDBSettings = new Mock<IOptions<DBSettings>>();

            // Create valid DB settings
            var dbSettings = new DBSettings
            {
                ServerUrl = "test-server",
                Database = "test-database",
                Username = "test-user",
                Password = "test-password",
                CommandTimeout = 30,
                ConnectTimeout = 10
            };
            _mockDBSettings.Setup(x => x.Value).Returns(dbSettings);

            // Setup service provider
            _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(ISQLConnectionAdapter))).Returns(_mockSQLConnectionAdapter.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<DBSettings>))).Returns(_mockDBSettings.Object);

            // Create the repository instance
            _testClass = new SPARepository(_mockServiceProvider.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new SPARepository(_mockServiceProvider.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullServiceProvider()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SPARepository(null));
        }

        #region EfetivarOrdemPagamento Tests

        [Fact]
        public async Task EfetivarOrdemPagamento_ComTransacaoValida_DeveExecutarCorretamente()
        {
            // Arrange
            var transaction = new TransactionEfetivarOrdemPagamento
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 1
            };

            // Setup the mock - ONLY mock the ISQLConnectionAdapter, NOT Dapper methods
            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.EfetivarOrdemPagamento(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();
            VerifyLoggingCalls("EfetivarPagamento", transaction.CorrelationId, transaction.chaveIdempotencia);
        }

        [Fact]
        public async Task EfetivarOrdemPagamento_ComTransacaoNula_DeveLancarExcecao()
        {
            // Act & Assert
         
            await Assert.ThrowsAsync<NullReferenceException>(() => _testClass.EfetivarOrdemPagamento(null).AsTask());
        }

        [Fact]
        public async Task EfetivarOrdemPagamento_ComErroNaOperacao_DeveRepassarExcecao()
        {
            // Arrange
            var transaction = new TransactionEfetivarOrdemPagamento
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 1
            };

            var expectedException = new InvalidOperationException("Erro de teste");
            SetupOperationWithError(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _testClass.EfetivarOrdemPagamento(transaction).AsTask());

            Assert.Equal(expectedException.Message, actualException.Message);
        }

        #endregion

        #region RegistrarOrdemPagamento Tests

        [Fact]
        public async Task RegistrarOrdemPagamento_ComTransacaoValida_DeveExecutarCorretamente()
        {
            // Arrange
            var transaction = new TransactionRegistrarOrdemPagamento
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 2,
                valor = 100.50,
                pagador = new JDPIDadosConta
                {
                    nrAgencia = "0001",
                    nrConta = "123456",
                    cpfCnpj = 12345678901
                },
                recebedor = new JDPIDadosConta
                {
                    nrAgencia = "0002",
                    nrConta = "654321",
                    cpfCnpj = 98765432109
                }
            };

            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.RegistrarOrdemPagamento(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();

            // Verify specific logging for this operation
            _mockLoggingAdapter.Verify(x => x.AddProperty("Pagador Agencia", transaction.pagador.nrAgencia), Times.Once);
            _mockLoggingAdapter.Verify(x => x.AddProperty("Pagador Conta", transaction.pagador.nrConta), Times.Once);
            _mockLoggingAdapter.Verify(x => x.AddProperty("Valor", transaction.valor.ToString()), Times.Once);
        }

        [Fact]
        public async Task RegistrarOrdemPagamento_ComTransacaoNula_DeveLancarExcecao()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _testClass.RegistrarOrdemPagamento(null).AsTask());

      
        }

        #endregion

        #region CancelarOrdemPagamento Tests

        [Fact]
        public async Task CancelarOrdemPagamento_ComTransacaoValida_DeveExecutarCorretamente()
        {
            // Arrange
            var transaction = new TransactionCancelarOrdemPagamento
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 3
            };

            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.CancelarOrdemPagamento(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();
            VerifyLoggingCalls("CancelarPagamento", transaction.CorrelationId, transaction.chaveIdempotencia);
        }

        #endregion

        #region RegistrarOrdemDevolucao Tests

        [Fact]
        public async Task RegistrarOrdemDevolucao_ComTransacaoValida_DeveExecutarCorretamente()
        {
            // Arrange
            var transaction = new TransactionRegistrarOrdemDevolucao
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 4
            };

            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.RegistrarOrdemDevolucao(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();
            VerifyLoggingCalls("IniciarDevolucao", transaction.CorrelationId, transaction.chaveIdempotencia);
        }

        #endregion

        #region EfetivarOrdemDevolucao Tests

        [Fact]
        public async Task EfetivarOrdemDevolucao_ComTransacaoValida_DeveExecutarCorretamente()
        {
            // Arrange
            var transaction = new TransactionEfetivarOrdemDevolucao
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 5
            };

            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.EfetivarOrdemDevolucao(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();
            VerifyLoggingCalls("EfetivarDevolucao", transaction.CorrelationId, transaction.chaveIdempotencia);
        }

        #endregion

        #region CancelarOrdemDevolucao Tests

        [Fact]
        public async Task CancelarOrdemDevolucao_ComTransacaoValida_DeveExecutarCorretamente()
        {
            // Arrange
            var transaction = new TransactionCancelarOrdemDevolucao
            {
                CorrelationId = "test-correlation-id",
                chaveIdempotencia = "test-idempotencia",
                canal = 6
            };

            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.CancelarOrdemDevolucao(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();
            VerifyLoggingCalls("CancelarDevolucao", transaction.CorrelationId, transaction.chaveIdempotencia);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CenarioCompleto_FluxoPagamento_DeveExecutarCorretamente()
        {
            // Arrange
            var transactionRegistrar = new TransactionRegistrarOrdemPagamento
            {
                CorrelationId = "integration-test-id",
                chaveIdempotencia = "integration-key",
                canal = 1,
                valor = 50.00,
                pagador = new JDPIDadosConta { nrAgencia = "0001", nrConta = "111111", cpfCnpj = 11111111111 },
                recebedor = new JDPIDadosConta { nrAgencia = "0002", nrConta = "222222", cpfCnpj = 22222222222 }
            };

            var transactionEfetivar = new TransactionEfetivarOrdemPagamento
            {
                CorrelationId = "integration-test-id",
                chaveIdempotencia = "integration-key",
                canal = 1
            };

            SetupSuccessfulOperation();

            // Act
            var registroResult = await _testClass.RegistrarOrdemPagamento(transactionRegistrar);
            var efetivacaoResult = await _testClass.EfetivarOrdemPagamento(transactionEfetivar);

            // Assert
            Assert.NotNull(registroResult);
            Assert.NotNull(efetivacaoResult);

            // Verify both operations were called
            _mockSQLConnectionAdapter.Verify(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default), Times.Exactly(2));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task TestarDiferentesCanais_DeveExecutarCorretamente(int canal)
        {
            // Arrange
            var transaction = new TransactionEfetivarOrdemPagamento
            {
                CorrelationId = "canal-test-id",
                chaveIdempotencia = "canal-key",
                canal = canal
            };

            SetupSuccessfulOperation();

            // Act
            var result = await _testClass.EfetivarOrdemPagamento(transaction);

            // Assert
            Assert.NotNull(result);
            VerifyOperationWasCalled();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Setup mock for successful operation - ONLY mocks ISQLConnectionAdapter
        /// </summary>
        private void SetupSuccessfulOperation()
        {
            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
                .Returns(Task.CompletedTask);
        }

        /// <summary>
        /// Setup mock for operation that throws an exception
        /// </summary>
        private void SetupOperationWithError(Exception exception)
        {
            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
                .ThrowsAsync(exception);
        }

        /// <summary>
        /// Verify that the database operation was called exactly once
        /// </summary>
        private void VerifyOperationWasCalled()
        {
            _mockSQLConnectionAdapter.Verify(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default), Times.Once);
        }

        /// <summary>
        /// Verify standard logging calls
        /// </summary>
        private void VerifyLoggingCalls(string operationName, string correlationId, string chaveIdempotencia)
        {
            _mockLoggingAdapter.Verify(x => x.AddProperty("Chave Idempotencia", chaveIdempotencia), Times.Once);
        }

        #endregion

        #region Dispose

        [Fact]
        public void Dispose_DeveExecutarCorretamente()
        {
            // Act
            _testClass.Dispose();

            // Assert - Should not throw
            Assert.True(true);
        }

        public void Dispose()
        {
            _testClass?.Dispose();
        }

        #endregion
    }
}