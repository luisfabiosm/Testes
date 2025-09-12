using AutoFixture.Xunit2;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Request;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.Services;
using Microsoft.AspNetCore.Http;

namespace pix_pagador_testes.Domain.Core.Common.Transaction
{


    public class TransactionFactoryTest
    {
        private TransactionFactory _testClass;
        private Mock<IContextAccessorService> _mockContextAccessor;
        private Mock<HttpContext> _mockHttpContext;
        private string _correlationId;
        private short _canal;
        private string _chaveIdempotencia;
        private Mock<IServiceProvider> _mockServiceProvider;

        public TransactionFactoryTest()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockContextAccessor = new Mock<IContextAccessorService>();
            _mockHttpContext = new Mock<HttpContext>();
            _correlationId = "test-correlation-123";
            _canal = 1;
            _chaveIdempotencia = "idempotencia-key-456";

            _mockServiceProvider.Setup(x => x.GetService(typeof(IContextAccessorService))).Returns(_mockContextAccessor.Object);


            _mockContextAccessor
                .Setup(x => x.GetCanal(It.IsAny<HttpContext>()))
                .Returns(_canal);

            _mockContextAccessor
                .Setup(x => x.GetChaveIdempotencia(It.IsAny<HttpContext>()))
                .Returns(_chaveIdempotencia);

            _testClass = new TransactionFactory(_mockContextAccessor.Object);
        }

        [Fact]
         public void CanConstruct()
        {
            // Act
            var instance = new TransactionFactory(_mockContextAccessor.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullContextAccessor()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TransactionFactory(null));
        }

        [Fact]
        public void CreateRegistrarOrdemPagamentoReturnsCorrectTransaction()
        {
            // Arrange
            var request = new JDPIRegistrarOrdemPagtoRequest
            {
                idReqSistemaCliente = "client-123",
                // Add other required properties
            };

            // Act
            var result = _testClass.CreateRegistrarOrdemPagamento(_mockHttpContext.Object, request, _correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.idReqSistemaCliente, result.idReqSistemaCliente);
            Assert.Equal(_correlationId, result.CorrelationId);
            Assert.Equal(1, result.Code);
            Assert.Equal(_canal, result.canal);
            Assert.Equal(_chaveIdempotencia, result.chaveIdempotencia);
        }

        [Fact]
        public void CreateEfetivarOrdemPagamentoReturnsCorrectTransaction()
        {
            // Arrange
            var request = new JDPIEfetivarOrdemPagtoRequest
            {
                idReqSistemaCliente = "client-123",
                agendamentoID = "agenda-456",
                idReqJdPi = "jdpi-789",
                endToEndId = "e2e-123",
                dtHrReqJdPi = "2025-01-01T10:00:00"
            };

            // Act
            var result = _testClass.CreateEfetivarOrdemPagamento(_mockHttpContext.Object, request, _correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.idReqSistemaCliente, result.idReqSistemaCliente);
            Assert.Equal(_correlationId, result.CorrelationId);
            Assert.Equal(2, result.Code);
            Assert.Equal(request.agendamentoID, result.agendamentoID);
            Assert.Equal(request.idReqJdPi, result.idReqJdPi);
            Assert.Equal(request.endToEndId, result.endToEndId);
            Assert.Equal(request.dtHrReqJdPi, result.dtHrReqJdPi);
            Assert.Equal(_canal, result.canal);
            Assert.Equal(_chaveIdempotencia, result.chaveIdempotencia);
        }

        [Fact]
        public void CreateCancelarOrdemPagamentoReturnsCorrectTransaction()
        {
            // Arrange
            var request = new JDPICancelarRegistroOrdemPagtoRequest
            {
                idReqSistemaCliente = "client-123",
                agendamentoID = "agenda-456",
                motivo = "Test cancellation reason",
                tipoErro = EnumTipoErro.SISTEMA
            };

            // Act
            var result = _testClass.CreateCancelarOrdemPagamento(_mockHttpContext.Object, request, _correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.idReqSistemaCliente, result.idReqSistemaCliente);
            Assert.Equal(_correlationId, result.CorrelationId);
            Assert.Equal(3, result.Code);
            Assert.Equal(request.agendamentoID, result.agendamentoID);
            Assert.Equal(request.motivo, result.motivo);
            Assert.Equal(request.tipoErro, result.tipoErro);
            Assert.Equal(_canal, result.canal);
            Assert.Equal(_chaveIdempotencia, result.chaveIdempotencia);
        }

        [Fact]
        public void CreateRegistrarOrdemDevolucaoReturnsCorrectTransaction()
        {
            // Arrange
            var request = new JDPIRequisitarDevolucaoOrdemPagtoRequest
            {
                idReqSistemaCliente = "client-123",
                endToEndIdOriginal = "e2e-original-123",
                endToEndIdDevolucao = "e2e-devolucao-456",
                codigoDevolucao = "DEV001",
                motivoDevolucao = "Test refund reason",
                valorDevolucao = 100.00
            };

            // Act
            var result = _testClass.CreateRegistrarOrdemDevolucao(_mockHttpContext.Object, request, _correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.idReqSistemaCliente, result.idReqSistemaCliente);
            Assert.Equal(_correlationId, result.CorrelationId);
            Assert.Equal(4, result.Code);
            Assert.Equal(request.endToEndIdOriginal, result.endToEndIdOriginal);
            Assert.Equal(request.endToEndIdDevolucao, result.endToEndIdDevolucao);
            Assert.Equal(request.codigoDevolucao, result.codigoDevolucao);
            Assert.Equal(request.motivoDevolucao, result.motivoDevolucao);
            Assert.Equal(request.valorDevolucao, result.valorDevolucao);
            Assert.Equal(_canal, result.canal);
            Assert.Equal(_chaveIdempotencia, result.chaveIdempotencia);
        }

        [Fact]
        public void CreateCancelarRegistroOrdemDevolucaoReturnsCorrectTransaction()
        {
            // Arrange
            var request = new JDPICancelarRegistroOrdemdDevolucaoRequest
            {
                idReqSistemaCliente = "client-123"
            };

            // Act
            var result = _testClass.CreateCancelarRegistroOrdemDevolucao(_mockHttpContext.Object, request, _correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.idReqSistemaCliente, result.idReqSistemaCliente);
            Assert.Equal(_correlationId, result.CorrelationId);
            Assert.Equal(5, result.Code);
            Assert.Equal(_canal, result.canal);
            Assert.Equal(_chaveIdempotencia, result.chaveIdempotencia);
        }

        [Fact]
        public void CreateEfetivarOrdemDevolucaoReturnsCorrectTransaction()
        {
            // Arrange
            var request = new JDPIEfetivarOrdemDevolucaoRequest
            {
                idReqSistemaCliente = "client-123",
                idReqJdPi = "jdpi-789",
                endToEndIdOriginal = "e2e-original-123",
                endToEndIdDevolucao = "e2e-devolucao-456",
                dtHrReqJdPi = "2025-01-01T10:00:00"
            };

            // Act
            var result = _testClass.CreateEfetivarOrdemDevolucao(_mockHttpContext.Object, request, _correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.idReqSistemaCliente, result.idReqSistemaCliente);
            Assert.Equal(_correlationId, result.CorrelationId);
            Assert.Equal(6, result.Code);
            Assert.Equal(request.idReqJdPi, result.idReqJdPi);
            Assert.Equal(request.endToEndIdOriginal, result.endToEndIdOriginal);
            Assert.Equal(request.endToEndIdDevolucao, result.endToEndIdDevolucao);
            Assert.Equal(request.dtHrReqJdPi, result.dtHrReqJdPi);
            Assert.Equal(_canal, result.canal);
            Assert.Equal(_chaveIdempotencia, result.chaveIdempotencia);
        }

        [Fact]
        public void CallsContextAccessorForCanalAndIdempotencia()
        {
            // Arrange
            var request = new JDPIRegistrarOrdemPagtoRequest
            {
                idReqSistemaCliente = "client-123",
                
            };

            // Act
            _testClass.CreateRegistrarOrdemPagamento(_mockHttpContext.Object, request, _correlationId);

            // Assert
            _mockContextAccessor.Verify(x => x.GetCanal(_mockHttpContext.Object), Times.Once);
            _mockContextAccessor.Verify(x => x.GetChaveIdempotencia(_mockHttpContext.Object), Times.Once);
        }
    }

}
