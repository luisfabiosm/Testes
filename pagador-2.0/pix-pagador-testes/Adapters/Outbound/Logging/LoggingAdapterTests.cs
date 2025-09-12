using Adapters.Outbound.Logging;
using Domain.Core.Ports.Outbound;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Outbound.Logging
{
    public class LoggingAdapterTests
    {
        private readonly LoggingAdapter _loggingAdapter;
        private const string TestSourceName = "TestSource";

        public LoggingAdapterTests()
        {
            _loggingAdapter = new LoggingAdapter(TestSourceName);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new LoggingAdapter(TestSourceName);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void Constructor_ComSourceNameNulo_DevePermitirConstrucao()
        {
            // Act
            var instance = new LoggingAdapter(null);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void Constructor_ComSourceNameVazio_DevePermitirConstrucao()
        {
            // Act
            var instance = new LoggingAdapter(string.Empty);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void LogInformation_ComMensagemSimples_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Test information message";

            // Act & Assert - Não deve lançar exceção
            _loggingAdapter.LogInformation(message);
        }

        [Fact]
        public void LogInformation_ComMensagemEArgumentos_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Test message with {Arg1} and {Arg2}";
            var arg1 = "value1";
            var arg2 = 123;

            // Act & Assert - Não deve lançar exceção
            _loggingAdapter.LogInformation(message, arg1, arg2);
        }

        [Fact]
        public void LogWarning_ComMensagemSimples_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Test warning message";

            // Act & Assert
            _loggingAdapter.LogWarning(message);
        }

        [Fact]
        public void LogWarning_ComMensagemEArgumentos_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Warning: {Operation} failed with {ErrorCode}";
            var operation = "TestOperation";
            var errorCode = 500;

            // Act & Assert
            _loggingAdapter.LogWarning(message, operation, errorCode);
        }

        [Fact]
        public void LogError_ComMensagemSimples_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Test error message";

            // Act & Assert
            _loggingAdapter.LogError(message);
        }

        [Fact]
        public void LogError_ComExcecao_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Error occurred in {Operation}";
            var operation = "TestOperation";
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert
            _loggingAdapter.LogError(message, exception, operation);
        }

        [Fact]
        public void LogError_ComExcecaoNula_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Error message without exception";

            // Act & Assert
            _loggingAdapter.LogError(message, null);
        }

        [Fact]
        public void LogDebug_ComMensagemSimples_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Debug message";

            // Act & Assert
            _loggingAdapter.LogDebug(message);
        }

        [Fact]
        public void LogDebug_ComMensagemEArgumentos_DeveExecutarSemErros()
        {
            // Arrange
            var message = "Debug: Processing {ItemCount} items";
            var itemCount = 42;

            // Act & Assert
            _loggingAdapter.LogDebug(message, itemCount);
        }

        [Fact]
        public void AddProperty_ComAtividadeAtual_DeveExecutarSemErros()
        {
            // Arrange
            var key = "TestKey";
            var value = "TestValue";

            // Act & Assert - Não deve lançar exceção
            _loggingAdapter.AddProperty(key, value);
        }

        [Theory]
        [InlineData(null, "value")]
        [InlineData("key", null)]
        [InlineData("", "value")]
        [InlineData("key", "")]
        public void AddProperty_ComParametrosVariados_DeveExecutarSemErros(string key, string value)
        {
            // Act & Assert - Não deve lançar exceção
            _loggingAdapter.AddProperty(key, value);
        }

        [Fact]
        public void StartOperation_ComParametrosValidos_DeveRetornarOperationContext()
        {
            // Arrange
            var operationName = "TestOperation";
            var correlationId = "TEST-CORRELATION-123";

            // Act
            var result = _loggingAdapter.StartOperation(operationName, correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IOperationContext>(result);
        }

        [Theory]
        [InlineData(ActivityKind.Internal)]
        [InlineData(ActivityKind.Server)]
        [InlineData(ActivityKind.Client)]
        [InlineData(ActivityKind.Producer)]
        [InlineData(ActivityKind.Consumer)]
        public void StartOperation_ComDiferentesActivityKinds_DeveRetornarOperationContext(ActivityKind kind)
        {
            // Arrange
            var operationName = "TestOperation";
            var correlationId = "TEST-CORRELATION-123";

            // Act
            var result = _loggingAdapter.StartOperation(operationName, correlationId, default, kind);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IOperationContext>(result);
        }

        [Theory]
        [InlineData(null, "correlationId")]
        [InlineData("operation", null)]
        [InlineData("", "correlationId")]
        [InlineData("operation", "")]
        public void StartOperation_ComParametrosInvalidos_DeveRetornarOperationContext(string operationName, string correlationId)
        {
            // Act
            var result = _loggingAdapter.StartOperation(operationName, correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IOperationContext>(result);
        }

        [Fact]
        public void StartOperation_ComParentContext_DeveRetornarOperationContext()
        {
            // Arrange
            var operationName = "ChildOperation";
            var correlationId = "CHILD-CORRELATION-123";
            var parentContext = new ActivityContext();

            // Act
            var result = _loggingAdapter.StartOperation(operationName, correlationId, parentContext);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IOperationContext>(result);
        }

        [Fact]
        public void Dispose_DeveExecutarSemErros()
        {
            // Act & Assert - Não deve lançar exceção
            _loggingAdapter.Dispose();
        }

        [Fact]
        public void Dispose_ChamadaMultipla_DeveExecutarSemErros()
        {
            // Act & Assert - Não deve lançar exceção
            _loggingAdapter.Dispose();
            _loggingAdapter.Dispose();
            _loggingAdapter.Dispose();
        }

        [Fact]
        public void ImplementsIDisposable()
        {
            // Assert
            Assert.IsAssignableFrom<IDisposable>(_loggingAdapter);
        }

        [Fact]
        public void ImplementsILoggingAdapter()
        {
            // Assert
            Assert.IsAssignableFrom<ILoggingAdapter>(_loggingAdapter);
        }

        [Fact]
        public void ThreadSafety_MultiplasChamadasSimultaneas_DeveExecutarSemErros()
        {
            // Arrange
            const int numberOfThreads = 10;
            const int operationsPerThread = 100;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < numberOfThreads; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        _loggingAdapter.LogInformation($"Thread {threadId}, Operation {j}");
                        _loggingAdapter.LogWarning($"Thread {threadId}, Warning {j}");
                        _loggingAdapter.LogDebug($"Thread {threadId}, Debug {j}");
                        _loggingAdapter.AddProperty($"thread_{threadId}_prop_{j}", $"value_{j}");
                    }
                }));
            }

            // Assert - Não deve lançar exceção
            Task.WaitAll(tasks.ToArray());
        }
    }

}
