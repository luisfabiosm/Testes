using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Transactions.Devolucao
{
    public class TransactionRegistrarOrdemDevolucaoTest
    {
        private TransactionRegistrarOrdemDevolucao _testClass;
        private string _idReqSistemaCliente;
        private string _endToEndIdOriginal;
        private string _endToEndIdDevolucao;
        private string _codigoDevolucao;
        private string _motivoDevolucao;
        private double _valorDevolucao;

        public TransactionRegistrarOrdemDevolucaoTest()
        {
            _idReqSistemaCliente = "client-123";
            _endToEndIdOriginal = "e2e-original-456";
            _endToEndIdDevolucao = "e2e-devolucao-789";
            _codigoDevolucao = "CD001";
            _motivoDevolucao = "Motivo de devolução de teste";
            _valorDevolucao = 100.50;

            _testClass = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                codigoDevolucao = _codigoDevolucao,
                motivoDevolucao = _motivoDevolucao,
                valorDevolucao = _valorDevolucao,
                Code = 1,
                CorrelationId = "correlation-test"
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TransactionRegistrarOrdemDevolucao();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void PropertiesAreInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_idReqSistemaCliente, _testClass.idReqSistemaCliente);
            Assert.Equal(_endToEndIdOriginal, _testClass.endToEndIdOriginal);
            Assert.Equal(_endToEndIdDevolucao, _testClass.endToEndIdDevolucao);
            Assert.Equal(_codigoDevolucao, _testClass.codigoDevolucao);
            Assert.Equal(_motivoDevolucao, _testClass.motivoDevolucao);
            Assert.Equal(_valorDevolucao, _testClass.valorDevolucao);
            Assert.Equal(1, _testClass.Code);
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
            Assert.Contains(_endToEndIdOriginal, result);
            Assert.Contains(_endToEndIdDevolucao, result);
            Assert.Contains(_codigoDevolucao, result);
        }

        [Fact]
        public void ImplementsCorrectInterfaces()
        {
            // Assert
            Assert.IsAssignableFrom<BaseTransaction<BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>>(_testClass);
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>>(_testClass);
        }

        [Fact]
        public void RecordEqualityWorksForAllProperties()
        {
            // Arrange
            var instance1 = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                codigoDevolucao = _codigoDevolucao,
                motivoDevolucao = _motivoDevolucao,
                valorDevolucao = _valorDevolucao,
                Code = 1,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                codigoDevolucao = _codigoDevolucao,
                motivoDevolucao = _motivoDevolucao,
                valorDevolucao = _valorDevolucao,
                Code = 1,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void RecordInequalityWorks()
        {
            // Arrange
            var instance1 = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                codigoDevolucao = _codigoDevolucao,
                motivoDevolucao = _motivoDevolucao,
                valorDevolucao = _valorDevolucao,
                Code = 1,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = "different-client",
                endToEndIdOriginal = _endToEndIdOriginal,
                endToEndIdDevolucao = _endToEndIdDevolucao,
                codigoDevolucao = _codigoDevolucao,
                motivoDevolucao = _motivoDevolucao,
                valorDevolucao = _valorDevolucao,
                Code = 1,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.NotEqual(instance1, instance2);
        }

        [Fact]
        public void CanSetAllRequiredProperties()
        {
            // Arrange
            var newIdReqSistemaCliente = "new-client-987";
            var newEndToEndIdOriginal = "new-e2e-original-654";
            var newEndToEndIdDevolucao = "new-e2e-devolucao-321";
            var newCodigoDevolucao = "CD002";
            var newMotivoDevolucao = "Novo motivo de devolução";
            var newValorDevolucao = 250.75;

            // Act
            var instance = new TransactionRegistrarOrdemDevolucao
            {
                idReqSistemaCliente = newIdReqSistemaCliente,
                endToEndIdOriginal = newEndToEndIdOriginal,
                endToEndIdDevolucao = newEndToEndIdDevolucao,
                codigoDevolucao = newCodigoDevolucao,
                motivoDevolucao = newMotivoDevolucao,
                valorDevolucao = newValorDevolucao,
                Code = 2,
                CorrelationId = "new-correlation"
            };

            // Assert
            Assert.Equal(newIdReqSistemaCliente, instance.idReqSistemaCliente);
            Assert.Equal(newEndToEndIdOriginal, instance.endToEndIdOriginal);
            Assert.Equal(newEndToEndIdDevolucao, instance.endToEndIdDevolucao);
            Assert.Equal(newCodigoDevolucao, instance.codigoDevolucao);
            Assert.Equal(newMotivoDevolucao, instance.motivoDevolucao);
            Assert.Equal(newValorDevolucao, instance.valorDevolucao);
            Assert.Equal(2, instance.Code);
            Assert.Equal("new-correlation", instance.CorrelationId);
        }

        [Fact]
        public void ValorDevolucaoCanBeZero()
        {
            // Act
            var instance = new TransactionRegistrarOrdemDevolucao
            {
                valorDevolucao = 0.0
            };

            // Assert
            Assert.Equal(0.0, instance.valorDevolucao);
        }

        [Fact]
        public void ValorDevolucaoCanBeNegative()
        {
            // Act
            var instance = new TransactionRegistrarOrdemDevolucao
            {
                valorDevolucao = -50.25
            };

            // Assert
            Assert.Equal(-50.25, instance.valorDevolucao);
        }

        [Fact]
        public void CodigoDevolucaoCanBeEmpty()
        {
            // Act
            var instance = new TransactionRegistrarOrdemDevolucao
            {
                codigoDevolucao = string.Empty
            };

            // Assert
            Assert.Equal(string.Empty, instance.codigoDevolucao);
        }

        [Fact]
        public void MotivoDevolucaoCanBeEmpty()
        {
            // Act
            var instance = new TransactionRegistrarOrdemDevolucao
            {
                motivoDevolucao = string.Empty
            };

            // Assert
            Assert.Equal(string.Empty, instance.motivoDevolucao);
        }
    }
}
