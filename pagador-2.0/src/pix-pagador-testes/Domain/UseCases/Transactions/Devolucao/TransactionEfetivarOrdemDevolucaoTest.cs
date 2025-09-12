using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;

namespace pix_pagador_testes.Domain.UseCases.Transactions.Devolucao
{
    public class TransactionEfetivarOrdemDevolucaoTest
    {
        private TransactionEfetivarOrdemDevolucao _testClass;
        private string _idReqSistemaCliente;
        private string _idReqJdPi;
        private string _endToEndIdOriginal;
        private string _endToEndIdDevolucao;
        private string _dtHrReqJdPi;

        public TransactionEfetivarOrdemDevolucaoTest()
        {
            _idReqSistemaCliente = "client-123";
            _idReqJdPi = "jdpi-456";
            _endToEndIdOriginal = "e2e-original-789";
            _endToEndIdDevolucao = "e2e-devolucao-101";
            _dtHrReqJdPi = "2025-01-01T10:00:00";

            _testClass = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                dtHrReqJdPi = _dtHrReqJdPi,
                Code = 2,
                CorrelationId = "correlation-test"
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TransactionEfetivarOrdemDevolucao();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void PropertiesAreInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_idReqSistemaCliente, _testClass.idReqSistemaCliente);
            Assert.Equal(_idReqJdPi, _testClass.idReqJdPi);
            Assert.Equal(_endToEndIdOriginal, _testClass.endToEndIdOriginal);
            Assert.Equal(_endToEndIdDevolucao, _testClass.endToEndIdDevolucao);
            Assert.Equal(_dtHrReqJdPi, _testClass.dtHrReqJdPi);
            Assert.Equal(2, _testClass.Code);
        }

        [Fact]
        public void GetTransactionSerializationReturnsJsonString()
        {
            // Act
            var result = _testClass.getTransactionSerialization();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Contains(_idReqSistemaCliente, result);
            Assert.Contains(_idReqJdPi, result);
            Assert.Contains(_endToEndIdOriginal, result);
            Assert.Contains(_endToEndIdDevolucao, result);
        }

        [Fact]
        public void ImplementsCorrectInterfaces()
        {
            // Assert
            Assert.IsAssignableFrom<BaseTransaction<BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>>>(_testClass);
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<JDPIEfetivarOrdemDevolucaoResponse>>>(_testClass);
        }

        [Fact]
        public void RecordEqualityWorksForAllProperties()
        {
            // Arrange
            var instance1 = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                dtHrReqJdPi = _dtHrReqJdPi,
                Code = 2,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                dtHrReqJdPi = _dtHrReqJdPi,
                Code = 2,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void RecordInequalityWorks()
        {
            // Arrange
            var instance1 = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                dtHrReqJdPi = _dtHrReqJdPi,
                Code = 2,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = "different-client",
                idReqJdPi = _idReqJdPi,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                dtHrReqJdPi = _dtHrReqJdPi,
                Code = 2,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.NotEqual(instance1, instance2);
        }

        [Fact]
        public void CanSetAllPropertiesIndividually()
        {
            // Arrange
            var newIdReqSistemaCliente = "new-client-789";
            var newIdReqJdPi = "new-jdpi-123";
            var newEndToEndIdOriginal = "new-e2e-original-456";
            var newEndToEndIdDevolucao = "new-e2e-devolucao-789";
            var newDtHrReqJdPi = "2025-02-01T15:30:00";

            // Act
            var instance = new TransactionEfetivarOrdemDevolucao
            {
                idReqSistemaCliente = newIdReqSistemaCliente,
                idReqJdPi = newIdReqJdPi,
                endToEndIdOriginal = newEndToEndIdOriginal,
                endToEndIdDevolucao = newEndToEndIdDevolucao,
                dtHrReqJdPi = newDtHrReqJdPi,
                Code = 3,
                CorrelationId = "new-correlation"
            };

            // Assert
            Assert.Equal(newIdReqSistemaCliente, instance.idReqSistemaCliente);
            Assert.Equal(newIdReqJdPi, instance.idReqJdPi);
            Assert.Equal(newEndToEndIdOriginal, instance.endToEndIdOriginal);
            Assert.Equal(newEndToEndIdDevolucao, instance.endToEndIdDevolucao);
            Assert.Equal(newDtHrReqJdPi, instance.dtHrReqJdPi);
            Assert.Equal(3, instance.Code);
            Assert.Equal("new-correlation", instance.CorrelationId);
        }
    }
}