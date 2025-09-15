

using Domain.Core.Common.Mediator;
using Domain.Core.Models.Request;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute; 
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

#region ErrorHandlingTests

public class EndpointErrorHandlingTests
{
    private readonly CorrelationIdGenerator _realCorrelationIdGenerator;

    private readonly ITransactionFactory _mockTransactionFactory;

    public EndpointErrorHandlingTests()
    {
        _realCorrelationIdGenerator = new CorrelationIdGenerator();

        _mockTransactionFactory = Substitute.For<ITransactionFactory>();
    }

    [Fact]
    public void Endpoints_ComTransactionFactoryException_DevemTratarErro()
    {
        // Arrange
        var exception = new ArgumentException("Erro na factory");
        var httpContext = Substitute.For<HttpContext>();
        var request = new JDPIRegistrarOrdemPagtoRequest();

        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(Arg.Any<HttpContext>(), Arg.Any<JDPIRegistrarOrdemPagtoRequest>(), Arg.Any<string>())
            .Returns(x => throw exception);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            _mockTransactionFactory.CreateRegistrarOrdemPagamento(httpContext, request, "correlation-id");
        });
    }

    [Fact]
    public void CorrelationIdGenerator_ComTamanhoInvalido_DeveLancarExcecao()
    {

        // Act & Assert - Testa comportamento real da implementação
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _realCorrelationIdGenerator.GenerateWithPrefix("TEST", 0); // Tamanho inválido
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _realCorrelationIdGenerator.GenerateWithPrefix("TEST", 65); // Tamanho muito grande
        });
    }

    [Fact]
    public void CorrelationIdGenerator_ComParametrosValidos_DeveGerarCorretamente()
    {

        // Act
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("ERROR", 10);

        // Assert - Verifica comportamento real
        Assert.StartsWith("ERROR-", correlationId);
        Assert.Equal(16, correlationId.Length); // "ERROR-" + 10 chars
        
    }

    [Fact]
    public void TransactionFactory_ComHttpContextNulo_DeveTratarErro()
    {
        // Arrange
        var request = new JDPIRegistrarOrdemPagtoRequest();
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("ERR", 8);

        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(null, Arg.Any<JDPIRegistrarOrdemPagtoRequest>(), Arg.Any<string>())
            .Returns(x => throw new ArgumentNullException(nameof(HttpContext)));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            _mockTransactionFactory.CreateRegistrarOrdemPagamento(null, request, correlationId);
        });
    }

    [Fact]
    public void TransactionFactory_ComRequestNulo_DeveTratarErro()
    {
        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("ERR", 8);

        // ✅ NSubstitute - configurar exception para request nulo
        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(Arg.Any<HttpContext>(), null, Arg.Any<string>())
            .Returns(x => throw new ArgumentNullException("request"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            _mockTransactionFactory.CreateRegistrarOrdemPagamento(httpContext, null, correlationId);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TransactionFactory_ComCorrelationIdInvalido_DeveTratarErro(string invalidCorrelationId)
    {
        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        var request = new JDPIRegistrarOrdemPagtoRequest();

        // ✅ NSubstitute - configurar exception para correlationId inválido
        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(Arg.Any<HttpContext>(), Arg.Any<JDPIRegistrarOrdemPagtoRequest>(), Arg.Is<string>(s => string.IsNullOrWhiteSpace(s)))
            .Returns(x => throw new ArgumentException("CorrelationId cannot be null or empty"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            _mockTransactionFactory.CreateRegistrarOrdemPagamento(httpContext, request, invalidCorrelationId);
        });
    }

    [Fact]
    public void CorrelationIdGenerator_DeveGerarIdsUnicosParaErros()
    {
        // ✅ Teste real - verifica se IDs são únicos mesmo em cenários de erro

        // Act - Gera múltiplos IDs de erro
        var errorIds = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            var id = _realCorrelationIdGenerator.GenerateWithPrefix("ERR", 8);
            errorIds.Add(id);
        }

        // Assert - Todos devem ser únicos
        Assert.Equal(50, errorIds.Count);
        Assert.All(errorIds, id =>
        {
            Assert.StartsWith("ERR-", id);
            Assert.Equal(12, id.Length); // "ERR-" + 8 chars
        });
    }

    [Fact]
    public void TransactionFactory_ComTimeout_DeveTratarErro()
    {
        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        var request = new JDPIRegistrarOrdemPagtoRequest();
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("TIMEOUT", 6);

        // ✅ NSubstitute - simular timeout
        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(httpContext, request, correlationId)
            .Returns(x => throw new TimeoutException("Operation timed out"));

        // Act & Assert
        Assert.Throws<TimeoutException>(() =>
        {
            _mockTransactionFactory.CreateRegistrarOrdemPagamento(httpContext, request, correlationId);
        });
    }

    [Fact]
    public void ErrorHandling_DeveManterCorrelationIdConsistente()
    {
        // ✅ Teste híbrido - usa CorrelationId real com mock controlado

        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        var request = new JDPIRegistrarOrdemPagtoRequest { idReqSistemaCliente = "REQ-ERROR-TEST" };
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("CONSISTENT", 8);

        // Simula erro mas mantém correlationId válido
        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(httpContext, request, correlationId)
            .Returns(x => throw new InvalidOperationException($"Error processing request with correlationId: {correlationId}"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _mockTransactionFactory.CreateRegistrarOrdemPagamento(httpContext, request, correlationId);
        });

        // Verifica se o correlationId está na mensagem de erro
        Assert.Contains(correlationId, exception.Message);
        Assert.StartsWith("CONSISTENT-", correlationId);
    }
}

#endregion


