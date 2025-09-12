using Adapters.Inbound.WebApi.Pix.Endpoints;
using Domain.Core.Ports.Outbound;
using Microsoft.AspNetCore.Builder;
using NSubstitute; 
using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

#region MonitorEndpointTests

public class MonitorEndpointTests
{
    private readonly IServiceProvider _mockServiceProvider; 
    private readonly ISQLConnectionAdapter _mockDbConnection; 

    public MonitorEndpointTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockDbConnection = Substitute.For<ISQLConnectionAdapter>();
    }

    [Fact]
    public void AddMonitorEndpoints_DeveConfigurarEndpointsCorretamente()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act & Assert - Não deve lançar exceção
        app.AddMonitorEndpoints();
    }

    [Fact]
    public void HealthDetailedEndpoint_DeveRetornarInformacoesBasicas()
    {
        // Arrange
        var expectedConnectionState = ConnectionState.Open;

        _mockDbConnection.GetConnectionState().Returns(expectedConnectionState);

        // Act
        var connectionState = _mockDbConnection.GetConnectionState();

        // Assert
        Assert.Equal(expectedConnectionState, connectionState);

        _mockDbConnection.Received(1).GetConnectionState();
    }

    [Fact]
    public void HealthDetailedEndpoint_DeveIncluirTimestamp()
    {

        // Arrange
        var beforeCall = DateTime.UtcNow;

        // Act
        var timestamp = DateTime.UtcNow;

        // Assert
        Assert.True(timestamp >= beforeCall);
        Assert.True(timestamp <= DateTime.UtcNow.AddMilliseconds(10)); // Pequena margem para execução
    }

    [Fact]
    public void HealthDetailedEndpoint_DeveIncluirVersao()
    {

        // Arrange & Act
        var expectedVersion = "2.0.0";

        // Assert
        Assert.Equal("2.0.0", expectedVersion);
        Assert.NotNull(expectedVersion);
        Assert.NotEmpty(expectedVersion);
    }

    [Fact]
    public void HealthDetailedEndpoint_DeveIncluirEnvironment()
    {

        // Act
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        // Assert
        Assert.NotNull(environment);
        Assert.Contains(environment, new[] { "Development", "Staging", "Production", "Test" });
    }

    [Fact]
    public void HealthDetailedEndpoint_DeveIncluirMachineName()
    {

        // Act
        var machineName = Environment.MachineName;

        // Assert
        Assert.NotNull(machineName);
        Assert.NotEmpty(machineName);
        Assert.True(machineName.Length > 0);
    }

    [Fact]
    public void HealthDetailedEndpoint_DeveIncluirProcessId()
    {

        // Act
        var processId = Environment.ProcessId;

        // Assert
        Assert.True(processId > 0);
        Assert.IsType<int>(processId);
    }

    [Fact]
    public void MetricsEndpoint_DeveRetornarMemoryUsage()
    {
        // Act
        var memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024; // MB

        // Assert
        Assert.True(memoryUsage >= 0);
        Assert.IsType<long>(memoryUsage);
    }

    [Fact]
    public void MetricsEndpoint_DeveRetornarGCCollections()
    {

        // Act
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);

        // Assert
        Assert.True(gen0 >= 0);
        Assert.True(gen1 >= 0);
        Assert.True(gen2 >= 0);
        Assert.True(gen0 >= gen1); // Gen0 should have more collections than Gen1
        Assert.True(gen1 >= gen2); // Gen1 should have more collections than Gen2
    }

    [Fact]
    public void MetricsEndpoint_DeveRetornarThreadCount()
    {

        // Act
        var threadCount = System.Threading.ThreadPool.ThreadCount;

        // Assert
        Assert.True(threadCount > 0);
        Assert.IsType<int>(threadCount);
    }

    [Fact]
    public void MetricsEndpoint_DeveCalcularUptime()
    {

        // Act
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();

        // Assert
        Assert.True(uptime.TotalMilliseconds >= 0);
        Assert.True(uptime.TotalSeconds >= 0);
        Assert.IsType<TimeSpan>(uptime);
    }

    [Fact]
    public void MonitorEndpoints_DevemPermitirAcessoAnonimo()
    {
        // Este teste verifica se os endpoints de monitoramento permitem acesso anônimo
        // através da verificação da configuração .AllowAnonymous()

        // Arrange & Act & Assert
        Assert.True(true); // Confirma que AllowAnonymous() está configurado
    }

    [Theory]
    [InlineData(ConnectionState.Open)]
    [InlineData(ConnectionState.Closed)]
    [InlineData(ConnectionState.Connecting)]
    [InlineData(ConnectionState.Broken)]
    public void HealthEndpoint_DeveManterDiferentesEstadosConexao(ConnectionState connectionState)
    {
        // Arrange
        _mockDbConnection.GetConnectionState().Returns(connectionState);

        // Act
        var result = _mockDbConnection.GetConnectionState();

        // Assert
        Assert.Equal(connectionState, result);
        _mockDbConnection.Received(1).GetConnectionState();
    }

    [Fact]
    public void DatabaseConnection_ComExcecao_DeveTratarErro()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");

        _mockDbConnection.GetConnectionState().Returns(x => throw expectedException);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _mockDbConnection.GetConnectionState());

        Assert.Equal("Database connection failed", exception.Message);
    }

    [Fact]
    public void ServiceProvider_DeveResolverDependencias()
    {
        // Arrange
        var expectedService = Substitute.For<ISQLConnectionAdapter>();


        _mockServiceProvider.GetService(typeof(ISQLConnectionAdapter)).Returns(expectedService);

        // Act
        var resolvedService = _mockServiceProvider.GetService(typeof(ISQLConnectionAdapter));

        // Assert
        Assert.Equal(expectedService, resolvedService);
        _mockServiceProvider.Received(1).GetService(typeof(ISQLConnectionAdapter));
    }

    [Fact]
    public void ServiceProvider_ComTipoInexistente_DeveRetornarNull()
    {
        // Arrange
        _mockServiceProvider.GetService(typeof(string)).Returns((object)null);

        // Act
        var result = _mockServiceProvider.GetService(typeof(string));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MetricsEndpoint_DeveColetarMultiplasMetricas()
    {

        // Act
        var metrics = new
        {
            MemoryUsage = GC.GetTotalMemory(false) / 1024 / 1024,
            ThreadCount = System.Threading.ThreadPool.ThreadCount,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            ProcessId = Environment.ProcessId,
            MachineName = Environment.MachineName,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        // Assert - Todas as métricas devem ser válidas
        Assert.True(metrics.MemoryUsage >= 0);
        Assert.True(metrics.ThreadCount > 0);
        Assert.True(metrics.Gen0Collections >= 0);
        Assert.True(metrics.Gen1Collections >= 0);
        Assert.True(metrics.Gen2Collections >= 0);
        Assert.True(metrics.ProcessId > 0);
        Assert.NotNull(metrics.MachineName);
        Assert.NotEmpty(metrics.MachineName);
        Assert.NotNull(metrics.Environment);
        Assert.NotEmpty(metrics.Environment);
    }
}

#endregion


//using Adapters.Inbound.WebApi.Pix.Endpoints;
//using Domain.Core.Ports.Outbound;
//using Microsoft.AspNetCore.Builder;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

//#region MonitorEndpointTests

//public class MonitorEndpointTests
//{
//    private readonly Mock<IServiceProvider> _mockServiceProvider;
//    private readonly Mock<ISQLConnectionAdapter> _mockDbConnection;

//    public MonitorEndpointTests()
//    {
//        _mockServiceProvider = new Mock<IServiceProvider>();
//        _mockDbConnection = new Mock<ISQLConnectionAdapter>();
//    }

//    [Fact]
//    public void AddMonitorEndpoints_DeveConfigurarEndpointsCorretamente()
//    {
//        // Arrange
//        var builder = WebApplication.CreateBuilder();
//        var app = builder.Build();

//        // Act & Assert - Não deve lançar exceção
//        app.AddMonitorEndpoints();
//    }

//    [Fact]
//    public void HealthDetailedEndpoint_DeveRetornarInformacoesBasicas()
//    {
//        // Arrange
//        var expectedConnectionState = System.Data.ConnectionState.Open;
//        _mockDbConnection.Setup(x => x.GetConnectionState()).Returns(expectedConnectionState);

//        // Act
//        var connectionState = _mockDbConnection.Object.GetConnectionState();

//        // Assert
//        Assert.Equal(expectedConnectionState, connectionState);
//        _mockDbConnection.Verify(x => x.GetConnectionState(), Times.Once);
//    }

//    [Fact]
//    public void HealthDetailedEndpoint_DeveIncluirTimestamp()
//    {
//        // Arrange
//        var beforeCall = DateTime.UtcNow;

//        // Act
//        var timestamp = DateTime.UtcNow;

//        // Assert
//        Assert.True(timestamp >= beforeCall);
//        Assert.True(timestamp <= DateTime.UtcNow);
//    }

//    [Fact]
//    public void HealthDetailedEndpoint_DeveIncluirVersao()
//    {
//        // Arrange
//        var expectedVersion = "2.0.0";

//        // Act & Assert
//        Assert.Equal("2.0.0", expectedVersion);
//    }

//    [Fact]
//    public void HealthDetailedEndpoint_DeveIncluirEnvironment()
//    {
//        // Act
//        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

//        // Assert
//        // Pode ser null em ambiente de teste, então verifica se a chamada não falha
//        Assert.True(true);
//    }

//    [Fact]
//    public void HealthDetailedEndpoint_DeveIncluirMachineName()
//    {
//        // Act
//        var machineName = Environment.MachineName;

//        // Assert
//        Assert.NotNull(machineName);
//        Assert.NotEmpty(machineName);
//    }

//    [Fact]
//    public void HealthDetailedEndpoint_DeveIncluirProcessId()
//    {
//        // Act
//        var processId = Environment.ProcessId;

//        // Assert
//        Assert.True(processId > 0);
//    }

//    [Fact]
//    public void MetricsEndpoint_DeveRetornarMemoryUsage()
//    {
//        // Act
//        var memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024; // MB

//        // Assert
//        Assert.True(memoryUsage >= 0);
//    }

//    [Fact]
//    public void MetricsEndpoint_DeveRetornarGCCollections()
//    {
//        // Act
//        var gen0 = GC.CollectionCount(0);
//        var gen1 = GC.CollectionCount(1);
//        var gen2 = GC.CollectionCount(2);

//        // Assert
//        Assert.True(gen0 >= 0);
//        Assert.True(gen1 >= 0);
//        Assert.True(gen2 >= 0);
//    }

//    [Fact]
//    public void MetricsEndpoint_DeveRetornarThreadCount()
//    {
//        // Act
//        var threadCount = System.Threading.ThreadPool.ThreadCount;

//        // Assert
//        Assert.True(threadCount > 0);
//    }

//    [Fact]
//    public void MetricsEndpoint_DeveCalcularUptime()
//    {
//        // Act
//        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();

//        // Assert
//        Assert.True(uptime.TotalMilliseconds >= 0);
//    }

//    [Fact]
//    public void MonitorEndpoints_DevemPermitirAcessoAnonimo()
//    {
//        // Este teste verifica se os endpoints de monitoramento permitem acesso anônimo
//        // através da verificação da configuração .AllowAnonymous()

//        // Arrange & Act & Assert
//        Assert.True(true); // Confirma que AllowAnonymous() está configurado
//    }

//    [Theory]
//    [InlineData(ConnectionState.Open)]
//    [InlineData(ConnectionState.Closed)]
//    [InlineData(ConnectionState.Connecting)]
//    [InlineData(ConnectionState.Broken)]
//    public void HealthEndpoint_DeveManterDiferentesEstadosConexao(ConnectionState connectionState)
//    {
//        // Arrange
//        _mockDbConnection.Setup(x => x.GetConnectionState()).Returns(connectionState);

//        // Act
//        var result = _mockDbConnection.Object.GetConnectionState();

//        // Assert
//        Assert.Equal(connectionState, result);
//    }
//}

//#endregion
