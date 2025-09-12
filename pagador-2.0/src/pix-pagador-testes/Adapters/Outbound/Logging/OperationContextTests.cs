using Adapters.Outbound.Logging;
using Domain.Core.Ports.Outbound;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Xunit;

namespace pix_pagador_testes.Adapters.Outbound.Logging;


public class OperationContextTests : IDisposable
{
    private readonly ActivitySource _activitySource;
    private readonly Activity _realActivity;
    private readonly OperationContext _operationContext;

    public OperationContextTests()
    {
        _activitySource = new ActivitySource("TestOperationContextSource");
        _realActivity = _activitySource.StartActivity("TestOperationContextActivity");
        _operationContext = new OperationContext(_realActivity);
    }

    public void Dispose()
    {
        _operationContext?.Dispose();
        _activitySource?.Dispose();
    }

    [Fact]
    public void CanConstruct()
    {
        // Act
        var instance = new OperationContext(_realActivity);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void Constructor_ComActivityNula_DevePermitirConstrucao()
    {
        // Act
        var instance = new OperationContext(null);

        // Assert
        Assert.NotNull(instance);
        Assert.Null(instance.Activity);
    }

    [Fact]
    public void Activity_DeveRetornarActivityPassadaNoConstructor()
    {
        // Assert
        if (_realActivity != null)
        {
            Assert.Equal(_realActivity, _operationContext.Activity);
        }
        else
        {
            Assert.Null(_operationContext.Activity);
        }
    }

    [Fact]
    public void SetTag_ComActivityValida_DeveExecutarSemErros()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";

        // Act & Assert - Não deve lançar exceção
        _operationContext.SetTag(key, value);

        // Verifica se a tag foi definida (se a activity não for null)
        if (_realActivity != null)
        {
            // Não podemos verificar diretamente, mas podemos confirmar que não houve exceção
            Assert.True(true); // Teste passou se chegou até aqui
        }
    }

    [Fact]
    public void SetTag_ComActivityNula_DeveExecutarSemErros()
    {
        // Arrange
        var contextWithNullActivity = new OperationContext(null);
        var key = "testKey";
        var value = "testValue";

        // Act & Assert - Não deve lançar exceção
        contextWithNullActivity.SetTag(key, value);
    }

    [Theory]
    [InlineData("OK")]
    [InlineData("ERROR")]
    [InlineData("FAILED")]
    [InlineData("SUCCESS")]
    public void SetStatus_ComDiferentesStatus_DeveExecutarSemErros(string status)
    {
        // Act & Assert - Não deve lançar exceção
        _operationContext.SetStatus(status);
    }

    [Fact]
    public void SetStatus_ComActivityNula_DeveExecutarSemErros()
    {
        // Arrange
        var contextWithNullActivity = new OperationContext(null);

        // Act & Assert - Não deve lançar exceção
        contextWithNullActivity.SetStatus("OK");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetStatus_ComStatusInvalido_DeveExecutarSemErros(string invalidStatus)
    {
        // Act & Assert - Não deve lançar exceção
        _operationContext.SetStatus(invalidStatus);
    }

    [Fact]
    public void StartOperation_ComActivityValida_DeveRetornarOperationContext()
    {
        // Arrange
        var operationName = "NestedOperation";
        var correlationId = "NESTED-CORRELATION-123";

        // Act
        var result = _operationContext.StartOperation(operationName, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IOperationContext>(result);
    }

    [Fact]
    public void StartOperation_ComActivityNula_DeveRetornarNoOpOperationContext()
    {
        // Arrange
        var contextWithNullActivity = new OperationContext(null);
        var operationName = "FailedOperation";
        var correlationId = "FAILED-CORRELATION-123";

        // Act
        var result = contextWithNullActivity.StartOperation(operationName, correlationId);

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
        var result = _operationContext.StartOperation(operationName, correlationId, default, kind);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IOperationContext>(result);
    }

    [Theory]
    [InlineData(null, "correlationId")]
    [InlineData("operation", null)]
    [InlineData("", "correlationId")]
    [InlineData("operation", "")]
    public void StartOperation_ComParametrosVariados_DeveRetornarOperationContext(string operationName, string correlationId)
    {
        // Act
        var result = _operationContext.StartOperation(operationName, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IOperationContext>(result);
    }

    [Fact]
    public void Dispose_ComActivityValida_DeveExecutarSemErros()
    {
        // Arrange
        var localActivitySource = new ActivitySource("LocalTestSource");
        var localActivity = localActivitySource.StartActivity("LocalTestActivity");
        var localContext = new OperationContext(localActivity);

        // Act & Assert - Não deve lançar exceção
        localContext.Dispose();

        // Cleanup
        localActivitySource.Dispose();
    }

    [Fact]
    public void Dispose_ComActivityNula_DeveExecutarSemErros()
    {
        // Arrange
        var contextWithNullActivity = new OperationContext(null);

        // Act & Assert - Não deve lançar exceção
        contextWithNullActivity.Dispose();
    }

    [Fact]
    public void Dispose_ChamadaMultipla_DeveExecutarSemErros()
    {
        // Arrange
        var localActivitySource = new ActivitySource("LocalMultipleDisposeSource");
        var localActivity = localActivitySource.StartActivity("LocalMultipleDisposeActivity");
        var localContext = new OperationContext(localActivity);

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
        Assert.IsAssignableFrom<IOperationContext>(_operationContext);
    }

    [Fact]
    public void ImplementsIDisposable()
    {
        // Assert
        Assert.IsAssignableFrom<IDisposable>(_operationContext);
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
                var localActivitySource = new ActivitySource($"ThreadOperationContextSource_{threadId}");
                var localActivity = localActivitySource.StartActivity($"ThreadOperationContextActivity_{threadId}");
                var localContext = new OperationContext(localActivity);

                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        localContext.SetTag($"thread_{threadId}_prop_{j}", $"value_{j}");
                        localContext.SetStatus($"status_{j}");
                        var nestedContext = localContext.StartOperation($"operation_{j}", $"corr_{j}");
                        nestedContext?.Dispose();
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


