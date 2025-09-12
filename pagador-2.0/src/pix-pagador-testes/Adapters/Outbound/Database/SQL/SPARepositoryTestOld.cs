//using Adapters.Outbound.Database.SQL;
//using Dapper;
//using Domain.Core.Constant;
//using Domain.Core.Models.JDPI;
//using Domain.Core.Models.Response;
//using Domain.Core.Ports.Outbound;
//using Domain.Core.Settings;
//using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
//using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
//using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
//using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
//using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
//using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Options;
//using Moq;
//using System;
//using System.Data;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Wmhelp.XPath2;
//using Xunit;

//namespace pix_pagador_testes.Adapters.Outbound.Database.SQL
//{
//    public class SPARepositoryTestOld 
//    {
//        private readonly Mock<ISPARepository> _testClass;

//        //private readonly ISPARepository _testClass;
//        private readonly Mock<ISQLConnectionAdapter> _mockSQLConnectionAdapter;
//        private readonly Mock<ILoggingAdapter> _mockLoggingAdapter;
//        private readonly Mock<IServiceProvider> _mockServiceProvider;
//        private readonly Mock<IOptions<DBSettings>> _mockDBSettings;
//        private readonly Mock<IDbConnection> _mockDbConnection;

//        public SPARepositoryTestOld()
//        {
//            _mockSQLConnectionAdapter = new Mock<ISQLConnectionAdapter>();
//            _mockLoggingAdapter = new Mock<ILoggingAdapter>();
//            _mockServiceProvider = new Mock<IServiceProvider>();
//            _mockDBSettings = new Mock<IOptions<DBSettings>>();
//            _mockDbConnection = new Mock<IDbConnection>();
//            _testClass = new Mock<ISPARepository>();

//            var dbSettings = new DBSettings
//            {
//                ServerUrl = "test-server",
//                Database = "test-database",
//                Username = "test-user",
//                Password = "test-password",
//                CommandTimeout = 30,
//                ConnectTimeout = 10
//            };
//            _mockDBSettings.Setup(x => x.Value).Returns(dbSettings);

//            _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
//            _mockServiceProvider.Setup(x => x.GetService(typeof(ISQLConnectionAdapter))).Returns(_mockSQLConnectionAdapter.Object);
//            _mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<DBSettings>))).Returns(_mockDBSettings.Object);
//            _mockServiceProvider.Setup(x => x.GetService(typeof(ISPARepository))).Returns(_testClass.Object);
//            //_testClass = new SPARepository(_mockServiceProvider.Object);
//        }

//        [Fact]
//        public void CanConstruct()
//        {
//            // Act
//            var instance = new SPARepository(_mockServiceProvider.Object);

//            // Assert
//            Assert.NotNull(instance);
//        }

//        [Fact]
//        public void CannotConstructWithNullServiceProvider()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>(() => new SPARepository(null));
//        }

//        #region EfetivarOrdemPagamento Tests

//        [Fact]
//        public async Task EfetivarOrdemPagamento_ComTransacaoValida_DeveRetornarMensagemPixOut()
//        {
//            // Arrange
//            var transaction = new TransactionEfetivarOrdemPagamento
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 1
//            };

//            var expectedMessage = new JDPIEfetivarOrdemPagamentoResponse
//            {
//                chvAutorizador = transaction.chaveIdempotencia,
//                CorrelationId = transaction.CorrelationId,

//            };

//            var _repoResul = JsonSerializer.Serialize(expectedMessage);


//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .Callback<Func<IDbConnection, Task>, CancellationToken>(async (operation, token) =>
//                {
//                    await operation(_mockDbConnection.Object);
//                })
//                .Returns(Task.CompletedTask);

//            _mockDbConnection.Setup(x => x.ExecuteAsync(
//                "sps_PixDebito",
//                It.IsAny<DynamicParameters>(),
//                null,
//                30,
//                CommandType.StoredProcedure))
//                .Callback<string, object, IDbTransaction, int?, CommandType?>((sql, param, trans, timeout, commandType) =>
//                {
//                    var parameters = param as DynamicParameters;
//                    // Simula a definição do valor de saída
//                    typeof(DynamicParameters).GetMethod("SetValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//                        ?.Invoke(parameters, new object[] { "@pvchMsgPixOUT", expectedMessage });
//                });

      
//            var result = _testClass.Setup(r => r.EfetivarOrdemPagamento(transaction)).ReturnsAsync(_repoResul);


//            // Assert
//            Assert.Equal(_repoResul, _repoResul);

//        }

   

//        #endregion

//        #region RegistrarOrdemPagamento Tests

//        [Fact]
//        public async Task RegistrarOrdemPagamento_ComTransacaoValida_DeveRetornarMensagemPixOut()
//        {
//            // Arrange
       
//            var transaction = new TransactionRegistrarOrdemPagamento
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 2,
//                valor = 100.50,
//                pagador = new JDPIDadosConta
//                {
//                    nrAgencia = "0001",
//                    nrConta = "123456",
//                    cpfCnpj = 12345678901
//                },
//                recebedor = new JDPIDadosConta
//                {
//                    nrAgencia = "0002",
//                    nrConta = "654321",
//                    cpfCnpj = 98765432109
//                }
//            };
//            var expectedResult = new JDPIRegistrarOrdemPagamentoResponse
//            {
//                chvAutorizador = "chave123",
//                CorrelationId = transaction.CorrelationId
//            };

//            var _repoResul = JsonSerializer.Serialize(expectedResult);

//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .Callback<Func<IDbConnection, Task>, CancellationToken>(async (operation, token) =>
//                {
//                    await operation(_mockDbConnection.Object);
//                })
//                .Returns(Task.CompletedTask);

//            _mockDbConnection.Setup(x => x.ExecuteAsync(
//                "sps_PixBloqueioSaldo",
//                It.IsAny<DynamicParameters>(),
//                null,
//                30,
//                CommandType.StoredProcedure));

//            // Act
//            var result = _testClass.Setup(r => r.RegistrarOrdemPagamento(transaction)).ReturnsAsync(_repoResul);


//            // Assert
//            //_mockLoggingAdapter.Verify(x => x.StartOperation("IniciarPagamento", transaction.CorrelationId), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Pagador Agencia", transaction.pagador.nrAgencia), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Pagador Conta", transaction.pagador.nrConta), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Valor", transaction.valor.ToString()), Times.Once);
//        }

//        #endregion

//        #region CancelarOrdemPagamento Tests

//        [Fact]
//        public async Task CancelarOrdemPagamento_ComTransacaoValida_DeveRetornarMensagemPixOut()
//        {
//            // Arrange
//            var transaction = new TransactionCancelarOrdemPagamento
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 3
//            };

//            var expectedMessage = new JDPICancelarOrdemPagamentoResponse
//            {
//                chvAutorizador = transaction.chaveIdempotencia,
//                CorrelationId = transaction.CorrelationId,

//            };


//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .Callback<Func<IDbConnection, Task>, CancellationToken>(async (operation, token) =>
//                {
//                    await operation(_mockDbConnection.Object);
//                })
//                .Returns(Task.CompletedTask);

//            _mockDbConnection.Setup(x => x.ExecuteAsync(
//                "sps_PixDesbloqueioSaldo",
//                It.IsAny<DynamicParameters>(),
//                null,
//                30,
//                CommandType.StoredProcedure));

//            // Act
//            var result = _testClass.Setup(r => r.CancelarOrdemPagamento(transaction)).ReturnsAsync(expectedMessage);


//            // Assert
//            // _mockLoggingAdapter.Verify(x => x.StartOperation("CancelarPagamento", transaction.CorrelationId), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia), Times.Once);
//        }

//        #endregion

//        #region RegistrarOrdemDevolucao Tests

//        [Fact]
//        public async Task RegistrarOrdemDevolucao_ComTransacaoValida_DeveRetornarMensagemPixOut()
//        {
//            // Arrange
//            var transaction = new TransactionRegistrarOrdemDevolucao
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 4
//            };

//            var expectedMessage = new JDPIRegistrarOrdemDevolucaoResponse
//            {
//                chvAutorizador = transaction.chaveIdempotencia,
//                CorrelationId = transaction.CorrelationId,

//            };

//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .Callback<Func<IDbConnection, Task>, CancellationToken>(async (operation, token) =>
//                {
//                    await operation(_mockDbConnection.Object);
//                })
//                .Returns(Task.CompletedTask);

//            _mockDbConnection.Setup(x => x.ExecuteAsync(
//                "sps_PixBloqueioSaldoDevolucao",
//                It.IsAny<DynamicParameters>(),
//                null,
//                30,
//                CommandType.StoredProcedure));

//            // Act
//            var result = _testClass.Setup(r => r.RegistrarOrdemDevolucao(transaction)).ReturnsAsync(expectedMessage);


//            // Assert
//            //_mockLoggingAdapter.Verify(x => x.StartOperation("IniciarDevolucao", transaction.CorrelationId), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia), Times.Once);
//        }

//        #endregion

//        #region EfetivarOrdemDevolucao Tests

//        [Fact]
//        public async Task EfetivarOrdemDevolucao_ComTransacaoValida_DeveRetornarMensagemPixOut()
//        {
//            // Arrange

//            var transaction = new TransactionEfetivarOrdemDevolucao
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 5
//            };

//            var expectedMessage = new JDPIEfetivarOrdemDevolucaoResponse
//            {
//                chvAutorizador = transaction.chaveIdempotencia,
//                CorrelationId = transaction.CorrelationId,

//            };


//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .Callback<Func<IDbConnection, Task>, CancellationToken>(async (operation, token) =>
//                {
//                    await operation(_mockDbConnection.Object);
//                })
//                .Returns(Task.CompletedTask);

//            _mockDbConnection.Setup(x => x.ExecuteAsync(
//                "sps_PixDevolucaoCredito",
//                It.IsAny<DynamicParameters>(),
//                null,
//                30,
//                CommandType.StoredProcedure));

//            // Act
//            var result = _testClass.Setup(r => r.EfetivarOrdemDevolucao(transaction)).ReturnsAsync(expectedMessage);


//            // Assert
//            //_mockLoggingAdapter.Verify(x => x.StartOperation("EfetivarDevolucao", transaction.CorrelationId), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia), Times.Once);
//        }

//        #endregion

//        #region CancelarOrdemDevolucao Tests

//        [Fact]
//        public async Task CancelarOrdemDevolucao_ComTransacaoValida_DeveRetornarMensagemPixOut()
//        {
//            // Arrange
//            var transaction = new TransactionCancelarOrdemDevolucao
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 6
//            };

//            var expectedMessage = "";

//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .Callback<Func<IDbConnection, Task>, CancellationToken>(async (operation, token) =>
//                {
//                    await operation(_mockDbConnection.Object);
//                })
//                .Returns(Task.CompletedTask);

//            _mockDbConnection.Setup(x => x.ExecuteAsync(
//                "sps_PixDesbloqueioSaldoDevolucao",
//                It.IsAny<DynamicParameters>(),
//                null,
//                30,
//                CommandType.StoredProcedure));

//            // Act
//            var result = _testClass.Setup(r => r.CancelarOrdemDevolucao(transaction)).ReturnsAsync(expectedMessage);


//            // Assert
//            //_mockLoggingAdapter.Verify(x => x.StartOperation("CancelarDevolucao", transaction.CorrelationId), Times.Once);
//            _mockLoggingAdapter.Verify(x => x.AddProperty("Chave Idempotencia", transaction.chaveIdempotencia), Times.Once);
//        }

//        #endregion

        

//        #region Error Handling Tests

//        [Fact]
//        public async Task QuandoErroNaOperacao_DeveRepassarExcecao()
//        {
//            // Arrange
//            var transaction = new TransactionEfetivarOrdemPagamento
//            {
//                CorrelationId = "test-correlation-id",
//                chaveIdempotencia = "test-idempotencia",
//                canal = 1
//            };

//            var expectedException = new Exception();
//            _mockSQLConnectionAdapter.Setup(x => x.ExecuteWithRetryAsync(It.IsAny<Func<IDbConnection, Task>>(), default))
//                .ThrowsAsync(expectedException);

//            // Act & Assert
//            var actualException = new Exception();
           
//            Assert.Equal(expectedException.Message, actualException.Message);
//        }

//        #endregion

       

      
//    }
//}