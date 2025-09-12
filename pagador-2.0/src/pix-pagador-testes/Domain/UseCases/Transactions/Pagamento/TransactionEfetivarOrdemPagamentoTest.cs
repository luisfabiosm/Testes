using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Transactions.Pagamento
{
    public class TransactionEfetivarOrdemPagamentoTest
    {
        private TransactionEfetivarOrdemPagamento _testClass;
        private string _idReqSistemaCliente;
        private string _idReqJdPi;
        private string _endToEndId;
        private string _dtHrReqJdPi;
        private string _agendamentoID;

        public TransactionEfetivarOrdemPagamentoTest()
        {
            _idReqSistemaCliente = "client-123";
            _idReqJdPi = "jdpi-456";
            _endToEndId = "e2e-789";
            _dtHrReqJdPi = "2025-01-01T10:00:00";
            _agendamentoID = "agenda-012";

            _testClass = new TransactionEfetivarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndId = _endToEndId,
                dtHrReqJdPi = _dtHrReqJdPi,
                agendamentoID = _agendamentoID,
                Code = 2,
                CorrelationId = "correlation-test"
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TransactionEfetivarOrdemPagamento();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void PropertiesAreInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_idReqSistemaCliente, _testClass.idReqSistemaCliente);
            Assert.Equal(_idReqJdPi, _testClass.idReqJdPi);
            Assert.Equal(_endToEndId, _testClass.endToEndId);
            Assert.Equal(_dtHrReqJdPi, _testClass.dtHrReqJdPi);
            Assert.Equal(_agendamentoID, _testClass.agendamentoID);
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
            Assert.Contains(_endToEndId, result);
        }

        [Fact]
        public void ImplementsCorrectInterfaces()
        {
            // Assert
            Assert.IsAssignableFrom<BaseTransaction<BaseReturn<JDPIEfetivarOrdemPagamentoResponse>>>(_testClass);
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<JDPIEfetivarOrdemPagamentoResponse>>>(_testClass);
        }

        [Fact]
        public void RecordEqualityWorksForAllProperties()
        {
            // Arrange
            var instance1 = new TransactionEfetivarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndId = _endToEndId,
                dtHrReqJdPi = _dtHrReqJdPi,
                agendamentoID = _agendamentoID,
                Code = 2,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionEfetivarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                idReqJdPi = _idReqJdPi,
                endToEndId = _endToEndId,
                dtHrReqJdPi = _dtHrReqJdPi,
                agendamentoID = _agendamentoID,
                Code = 2,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.Equal(instance1, instance2);
        }
    }


}
