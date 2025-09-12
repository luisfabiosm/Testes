using Domain.Core.Common.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Transaction
{
    public class BaseTransactionResponseTest
    {
        private BaseTransactionResponse _testClass;
        private string _correlationId;
        private string _chvAutorizador;

        public BaseTransactionResponseTest()
        {
            _correlationId = "test-correlation-123";
            _chvAutorizador = "auth-key-456";

            _testClass = new BaseTransactionResponse
            {
                CorrelationId = _correlationId,
                chvAutorizador = _chvAutorizador
            };
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new BaseTransactionResponse();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CorrelationIdIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_correlationId, _testClass.CorrelationId);
        }

        [Fact]
        public void ChvAutorizadorIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_chvAutorizador, _testClass.chvAutorizador);
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
        public void CanSetChvAutorizador()
        {
            // Arrange
            var newChvAutorizador = "new-auth-key-789";

            // Act
            _testClass.chvAutorizador = newChvAutorizador;

            // Assert
            Assert.Equal(newChvAutorizador, _testClass.chvAutorizador);
        }

        [Fact]
        public void RecordEqualityWorks()
        {
            // Arrange
            var instance1 = new BaseTransactionResponse
            {
                CorrelationId = _correlationId,
                chvAutorizador = _chvAutorizador
            };
            var instance2 = new BaseTransactionResponse
            {
                CorrelationId = _correlationId,
                chvAutorizador = _chvAutorizador
            };

            // Assert
            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void RecordInequalityWorks()
        {
            // Arrange
            var instance1 = new BaseTransactionResponse
            {
                CorrelationId = _correlationId,
                chvAutorizador = _chvAutorizador
            };
            var instance2 = new BaseTransactionResponse
            {
                CorrelationId = "different-correlation",
                chvAutorizador = _chvAutorizador
            };

            // Assert
            Assert.NotEqual(instance1, instance2);
        }
    }
}
