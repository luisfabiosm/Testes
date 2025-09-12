using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Transactions.Devolucao
{
    public class TransactionCancelarOrdemDevolucaoTest
    {
        private TransactionCancelarOrdemDevolucao _testClass;
        private string _idReqSistemaCliente;

        public TransactionCancelarOrdemDevolucaoTest()
        {
            _idReqSistemaCliente = "client-123";

            _testClass = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                Code = 2,
                CorrelationId = "correlation-test"
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TransactionCancelarOrdemDevolucao();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void PropertiesAreInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_idReqSistemaCliente, _testClass.idReqSistemaCliente);
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
        }

        [Fact]
        public void ImplementsCorrectInterfaces()
        {
            // Assert
            Assert.IsAssignableFrom<BaseTransaction<BaseReturn<JDPICancelarOrdemDevolucaoResponse>>>(_testClass);
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<JDPICancelarOrdemDevolucaoResponse>>>(_testClass);
        }

        [Fact]
        public void RecordEqualityWorksForAllProperties()
        {
            // Arrange
            var instance1 = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                Code = 2,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
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
            var instance1 = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                Code = 2,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = "different-client",
                Code = 2,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.NotEqual(instance1, instance2);
        }

        [Fact]
        public void IdReqSistemaClienteCanBeSetAndRetrieved()
        {
            // Arrange
            var expectedValue = "new-client-456";

            // Act
            var instance = new TransactionCancelarOrdemDevolucao
            {
                idReqSistemaCliente = expectedValue
            };

            // Assert
            Assert.Equal(expectedValue, instance.idReqSistemaCliente);
        }
    }
}
