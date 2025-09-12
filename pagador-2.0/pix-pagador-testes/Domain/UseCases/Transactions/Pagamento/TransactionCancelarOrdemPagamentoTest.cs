using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.Response;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;


namespace pix_pagador_testes.Domain.UseCases.Transactions.Pagamento
{
    public class TransactionCancelarOrdemPagamentoTest
    {

        private TransactionCancelarOrdemPagamento _testClass;
        private string _idReqSistemaCliente;
        private string _agendamentoID;
        private string _motivo; 
        private EnumTipoErro _tipoErro;

        public TransactionCancelarOrdemPagamentoTest()
        {
            _idReqSistemaCliente = "client-123";
            _agendamentoID = "agenda-012";
            _motivo = "Motivo de teste";
            _tipoErro = EnumTipoErro.SISTEMA;

            _testClass = new TransactionCancelarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                agendamentoID = _agendamentoID,
                Code = 2,
                motivo = _motivo,
                tipoErro = _tipoErro,
                CorrelationId = "correlation-test"
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TransactionCancelarOrdemPagamento();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void PropertiesAreInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_idReqSistemaCliente, _testClass.idReqSistemaCliente);
            Assert.Equal(_agendamentoID, _testClass.agendamentoID);
            Assert.Equal(_motivo, _testClass.motivo);
            Assert.Equal(_tipoErro, _testClass.tipoErro);
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
            Assert.Contains(_agendamentoID, result);
            Assert.Contains(_motivo, result);
        }

        [Fact]
        public void ImplementsCorrectInterfaces()
        {
            // Assert
            Assert.IsAssignableFrom<BaseTransaction<BaseReturn<JDPICancelarOrdemPagamentoResponse>>>(_testClass);
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<JDPICancelarOrdemPagamentoResponse>>>(_testClass);
        }

        [Fact]
        public void RecordEqualityWorksForAllProperties()
        {
            // Arrange
            var instance1 = new TransactionCancelarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                agendamentoID = _agendamentoID,
                motivo = _motivo,
                tipoErro = _tipoErro,
                Code = 2,
                CorrelationId = "correlation-test"
            };
            var instance2 = new TransactionCancelarOrdemPagamento
            {
                idReqSistemaCliente = _idReqSistemaCliente,
                agendamentoID = _agendamentoID,
                Code = 2,
                motivo = _motivo,
                tipoErro = _tipoErro,
                CorrelationId = "correlation-test"
            };

            // Assert
            Assert.Equal(instance1, instance2);
        }



    }


}
