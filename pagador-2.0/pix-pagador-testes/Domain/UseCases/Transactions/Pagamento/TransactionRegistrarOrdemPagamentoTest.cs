using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.JDPI;
using Domain.Core.Models.Response;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Transactions.Pagamento
{
    public class TransactionRegistrarOrdemPagamentoTest
    {
        private TransactionRegistrarOrdemPagamento _testClass;
        private string _idReqSistemaCliente;
        private string _correlationId;
        private int _code;

        public TransactionRegistrarOrdemPagamentoTest()
        {
            _idReqSistemaCliente = "client-123";
            _correlationId = "correlation-456";
            _code = 1;

            _testClass = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                CorrelationId = _correlationId,
                Code = _code
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TransactionRegistrarOrdemPagamento();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void IdReqSistemaClienteIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_idReqSistemaCliente, _testClass.idReqSistemaCliente);
        }

        [Fact]
        public void CorrelationIdIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_correlationId, _testClass.CorrelationId);
        }

        [Fact]
        public void CodeIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_code, _testClass.Code);
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
            Assert.Contains(_correlationId, result);
        }

        [Fact]
        public void ImplementsBaseTransaction()
        {
            // Assert
            Assert.IsAssignableFrom<BaseTransaction<BaseReturn<JDPIRegistrarOrdemPagamentoResponse>>>(_testClass);
        }

        [Fact]
        public void ImplementsIBSRequest()
        {
            // Assert
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<JDPIRegistrarOrdemPagamentoResponse>>>(_testClass);
        }

        [Fact]
        public void RecordEqualityWorks()
        {
            // Arrange
            var instance1 = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                CorrelationId = _correlationId,
                Code = _code
            };
            var instance2 = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                CorrelationId = _correlationId,
                Code = _code
            };

            // Assert
            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void RecordInequalityWorks()
        {
            // Arrange
            var instance1 = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                CorrelationId = _correlationId,
                Code = _code
            };
            var instance2 = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = "different-client",
                CorrelationId = _correlationId,
                Code = _code
            };

            // Assert
            Assert.NotEqual(instance1, instance2);
        }

        [Fact]
        public void CanSetAllRequiredProperties()
        {
            // Arrange
            var pagador = new JDPIDadosConta();
            var recebedor = new JDPIDadosConta();
            var valor = 100.00;
            var endToEndId = "E2E123456";

            // Act
            var instance = new TransactionRegistrarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                CorrelationId = _correlationId,
                Code = _code,
                pagador = pagador,
                recebedor = recebedor,
                valor = valor,
                endToEndId = endToEndId
            };

            // Assert
            Assert.Equal(_idReqSistemaCliente, instance.idReqSistemaCliente);
            Assert.Equal(_correlationId, instance.CorrelationId);
            Assert.Equal(_code, instance.Code);
            Assert.Equal(pagador, instance.pagador);
            Assert.Equal(recebedor, instance.recebedor);
            Assert.Equal(valor, instance.valor);
            Assert.Equal(endToEndId, instance.endToEndId);
        }
    }

}
