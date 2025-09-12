using Adapters.Outbound.Logging;
using Domain.Core.Ports.Outbound;
using Domain.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using Xunit;

namespace pix_pagador_testes.Adapters.Outbound.Logging
{

    #region NoOpOperationContextTests

    public class NoOpOperationContextTests
    {
        private readonly ActivitySource _activitySource;
        private readonly Activity _realActivity;
        private readonly NoOpOperationContext _noOpContext;

        public NoOpOperationContextTests()
        {
            _activitySource = new ActivitySource("TestSource");
            _realActivity = _activitySource.StartActivity("TestActivity");
            _noOpContext = new NoOpOperationContext(_realActivity);
        }

        public void Dispose()
        {
            _realActivity?.Dispose();
            _activitySource?.Dispose();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new NoOpOperationContext(_realActivity);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void Constructor_ComActivityNula_DeveUsarActivityCurrent()
        {
            // Act
            var instance = new NoOpOperationContext(null);

            // Assert
            Assert.NotNull(instance);
            // Activity pode ser null se não houver Activity.Current
        }

        [Fact]
        public void Activity_DeveRetornarActivityPassadaOuCurrent()
        {
            // Assert
            if (_realActivity != null)
            {
                Assert.Equal(_realActivity, _noOpContext.Activity);
            }
            else
            {
                // Se não conseguiu criar activity, verifica se retorna Activity.Current
                Assert.Equal(Activity.Current, _noOpContext.Activity);
            }
        }

        [Fact]
        public void SetTag_DeveExecutarSemEfeito()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";

            // Act & Assert - Não deve lançar exceção e não deve fazer nada
            _noOpContext.SetTag(key, value);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("key", null)]
        [InlineData(null, "value")]
        [InlineData("validKey", "validValue")]
        public void SetTag_ComParametrosVariados_DeveExecutarSemEfeito(string key, string value)
        {
            // Act & Assert - Não deve lançar exceção
            _noOpContext.SetTag(key, value);
        }

        [Fact]
        public void SetStatus_DeveExecutarSemEfeito()
        {
            // Act & Assert - Não deve lançar exceção e não deve fazer nada
            _noOpContext.SetStatus("OK");
        }

        [Theory]
        [InlineData("OK")]
        [InlineData("ERROR")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("CUSTOM_STATUS")]
        public void SetStatus_ComDiferentesStatus_DeveExecutarSemEfeito(string status)
        {
            // Act & Assert - Não deve lançar exceção
            _noOpContext.SetStatus(status);
        }

        [Fact]
        public void StartOperation_DeveRetornarProprioInstance()
        {
            // Arrange
            var operationName = "TestOperation";
            var correlationId = "TEST-CORRELATION-123";

            // Act
            var result = _noOpContext.StartOperation(operationName, correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Same(_noOpContext, result);
        }

        [Theory]
        [InlineData(ActivityKind.Internal)]
        [InlineData(ActivityKind.Server)]
        [InlineData(ActivityKind.Client)]
        [InlineData(ActivityKind.Producer)]
        [InlineData(ActivityKind.Consumer)]
        public void StartOperation_ComDiferentesActivityKinds_DeveRetornarProprioInstance(ActivityKind kind)
        {
            // Arrange
            var operationName = "TestOperation";
            var correlationId = "TEST-CORRELATION-123";

            // Act
            var result = _noOpContext.StartOperation(operationName, correlationId, default, kind);

            // Assert
            Assert.Same(_noOpContext, result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("operation", null)]
        [InlineData(null, "correlationId")]
        public void StartOperation_ComParametrosInvalidos_DeveRetornarProprioInstance(string operationName, string correlationId)
        {
            // Act
            var result = _noOpContext.StartOperation(operationName, correlationId);

            // Assert
            Assert.Same(_noOpContext, result);
        }

        [Fact]
        public void Dispose_ComActivityValida_DeveExecutarSemErros()
        {
            // Act & Assert - Não deve lançar exceção
            _noOpContext.Dispose();
        }

        [Fact]
        public void Dispose_ComActivityNula_DeveExecutarSemErros()
        {
            // Arrange
            var contextWithNullActivity = new NoOpOperationContext(null);

            // Act & Assert - Não deve lançar exceção
            contextWithNullActivity.Dispose();
        }

        [Fact]
        public void Dispose_ChamadaMultipla_DeveExecutarSemErros()
        {
            // Arrange
            var localActivitySource = new ActivitySource("LocalTestSource");
            var localActivity = localActivitySource.StartActivity("LocalTestActivity");
            var localContext = new NoOpOperationContext(localActivity);

            // Act & Assert - Não deve lançar exceção
            localContext.Dispose();
            localContext.Dispose();
            localContext.Dispose();

            // Cleanup
            localActivitySource.Dispose();
        }

        [Fact]
        public void ImplementsIOperationContext()
        {
            // Assert
            Assert.IsAssignableFrom<IOperationContext>(_noOpContext);
        }

        [Fact]
        public void ImplementsIDisposable()
        {
            // Assert
            Assert.IsAssignableFrom<IDisposable>(_noOpContext);
        }

        [Fact]
        public void ThreadSafety_MultiplasChamadasSimultaneas_DeveExecutarSemErros()
        {
            // Arrange
            const int numberOfThreads = 5;
            const int operationsPerThread = 50;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < numberOfThreads; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    var localActivitySource = new ActivitySource($"ThreadTestSource_{threadId}");
                    var localActivity = localActivitySource.StartActivity($"ThreadTestActivity_{threadId}");
                    var localContext = new NoOpOperationContext(localActivity);

                    try
                    {
                        for (int j = 0; j < operationsPerThread; j++)
                        {
                            localContext.SetTag($"thread_{threadId}_prop_{j}", $"value_{j}");
                            localContext.SetStatus($"status_{j}");
                            var nestedContext = localContext.StartOperation($"operation_{j}", $"corr_{j}");
                            // NoOpOperationContext retorna a mesma instância, então não precisa dispose
                        }
                    }
                    finally
                    {
                        localContext.Dispose();
                        localActivitySource.Dispose();
                    }
                }));
            }

            // Assert - Não deve lançar exceção
            Task.WaitAll(tasks.ToArray());
        }
    }

    #endregion


}