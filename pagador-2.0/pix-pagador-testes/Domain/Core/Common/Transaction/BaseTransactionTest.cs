using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Exceptions;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using T = Domain.Core.Models.Response.JDPIRegistrarOrdemPagamentoResponse;

namespace pix_pagador_testes.Domain.Core.Common.Transaction
{
    public class BaseTransactionTest
    {
        private TestableBaseTransaction _testClass;
        private int _code;
        private string _correlationId;
        private int _canal;
        private string _chaveIdempotencia;

        public BaseTransactionTest()
        {
            _code = 1;
            _correlationId = "test-correlation-123";
            _canal = 100;
            _chaveIdempotencia = "idempotencia-key-456";

            _testClass = new TestableBaseTransaction
            {
                Code = _code,
                CorrelationId = _correlationId,
                canal = _canal,
                chaveIdempotencia = _chaveIdempotencia
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TestableBaseTransaction();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CodeIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_code, _testClass.Code);
        }

        [Fact]
        public void CorrelationIdIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_correlationId, _testClass.CorrelationId);
        }

        [Fact]
        public void CanalIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_canal, _testClass.canal);
        }

        [Fact]
        public void ChaveIdempotenciaIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_chaveIdempotencia, _testClass.chaveIdempotencia);
        }

        [Fact]
        public void DefaultConstructorInitializesEmptyCorrelationId()
        {
            // Act
            var instance = new TestableBaseTransaction();

            // Assert
            Assert.Equal(string.Empty, instance.CorrelationId);
        }

        [Fact]
        public void CanSetCorrelationId()
        {
            // Arrange
            var newCorrelationId = "new-correlation-789";

            // Act
            _testClass.CorrelationId = newCorrelationId;

            // Assert
            Assert.Equal(newCorrelationId, _testClass.CorrelationId);
        }

        [Fact]
        public void GetTransactionSerializationIsAbstract()
        {
            // Act
            var result = _testClass.getTransactionSerialization();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestSerialization", result);
        }

        [Fact]
        public void ImplementsIBSRequest()
        {
            // Assert
            Assert.IsAssignableFrom<IBSRequest<BaseReturn<T>>>(_testClass);
        }

        // Testable implementation to test abstract class
        private record TestableBaseTransaction : BaseTransaction<BaseReturn<T>>
        {
            public override string getTransactionSerialization()
            {
                return "TestSerialization";
            }
        }
    }


}
