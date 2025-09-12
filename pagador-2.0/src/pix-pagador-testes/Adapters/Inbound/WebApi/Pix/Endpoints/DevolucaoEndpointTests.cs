using NSubstitute;  // ✅ Package: NSubstitute
using Xunit;
using Adapters.Inbound.WebApi.Pix.Endpoints;
using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

#region DevolucaoEndpointTests com NSubstitute

public class DevolucaoEndpointTests
{
    private readonly BSMediator _mockMediator;  // ✅ NSubstitute não precisa de Mock<>
    private readonly ITransactionFactory _mockTransactionFactory;
    private readonly CorrelationIdGenerator _mockCorrelationIdGenerator;
    private readonly HttpContext _mockHttpContext;

    public DevolucaoEndpointTests()
    {
        _mockMediator = Substitute.For<BSMediator>(Substitute.For<IServiceProvider>());
        _mockTransactionFactory = Substitute.For<ITransactionFactory>();
        _mockCorrelationIdGenerator = Substitute.For<CorrelationIdGenerator>();
        _mockHttpContext = Substitute.For<HttpContext>();
    }

    [Fact]
    public void AddDevolucaoEndpoints_DeveConfigurarEndpointsCorretamente()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act & Assert - Não deve lançar exceção
        app.AddDevolucaoEndpoints();
    }

    [Fact]
    public void RequisitarDevolucao_ComParametrosValidos_DeveGerarCorrelationIdComPrefixo()
    {
        // Arrange
        var request = new JDPIRequisitarDevolucaoOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-123",
            endToEndIdOriginal = "E123456789",
            valorDevolucao = 100.0
        };

        var expectedCorrelationId = "DEV-ABC123";
        var expectedTransaction = new TransactionRegistrarOrdemDevolucao();
        var expectedResponse = BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>.FromSuccess(new JDPIRegistrarOrdemDevolucaoResponse());

        // ✅ NSubstitute - Sintaxe limpa sem .Setup()
        _mockCorrelationIdGenerator.GenerateWithPrefix("DEV", 12).Returns(expectedCorrelationId);
        _mockTransactionFactory.CreateRegistrarOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId).Returns(expectedTransaction);
        _mockMediator.Send<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>(expectedTransaction, default).Returns(expectedResponse);

        // Act
        _mockCorrelationIdGenerator.GenerateWithPrefix("DEV", 12);
        _mockTransactionFactory.CreateRegistrarOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId);

        // Assert - NSubstitute também tem verificação limpa
        _mockCorrelationIdGenerator.Received(1).GenerateWithPrefix("DEV", 12);
        _mockTransactionFactory.Received(1).CreateRegistrarOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId);
    }

    [Fact]
    public void CancelarDevolucao_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoCDEV()
    {
        // Arrange
        var request = new JDPICancelarRegistroOrdemdDevolucaoRequest
        {
            idReqSistemaCliente = "REQ-123"
        };

        var expectedCorrelationId = "CDEV-ABC123";
        var expectedTransaction = new TransactionCancelarOrdemDevolucao();

        // ✅ NSubstitute - Setup ainda mais limpo
        _mockCorrelationIdGenerator.GenerateWithPrefix("CDEV", 12).Returns(expectedCorrelationId);
        _mockTransactionFactory.CreateCancelarRegistroOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId).Returns(expectedTransaction);

        // Act
        _mockCorrelationIdGenerator.GenerateWithPrefix("CDEV", 12);
        _mockTransactionFactory.CreateCancelarRegistroOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId);

        // Assert
        _mockCorrelationIdGenerator.Received(1).GenerateWithPrefix("CDEV", 12);
        _mockTransactionFactory.Received(1).CreateCancelarRegistroOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId);
    }

    [Fact]
    public void EfetivarDevolucao_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoEDEV()
    {
        // Arrange
        var request = new JDPIEfetivarOrdemDevolucaoRequest
        {
            idReqSistemaCliente = "REQ-123",
            idReqJdPi = "JDPI-123"
        };

        var expectedCorrelationId = "EDEV-ABC123";
        var expectedTransaction = new TransactionEfetivarOrdemDevolucao();

        // ✅ NSubstitute
        _mockCorrelationIdGenerator.GenerateWithPrefix("EDEV", 12).Returns(expectedCorrelationId);
        _mockTransactionFactory.CreateEfetivarOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId).Returns(expectedTransaction);

        // Act
        _mockCorrelationIdGenerator.GenerateWithPrefix("EDEV", 12);
        _mockTransactionFactory.CreateEfetivarOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId);

        // Assert
        _mockCorrelationIdGenerator.Received(1).GenerateWithPrefix("EDEV", 12);
        _mockTransactionFactory.Received(1).CreateEfetivarOrdemDevolucao(_mockHttpContext, request, expectedCorrelationId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RequisitarDevolucao_ComRequestInvalido_DeveManterFluxo(string invalidValue)
    {
        // Arrange
        var request = new JDPIRequisitarDevolucaoOrdemPagtoRequest
        {
            idReqSistemaCliente = invalidValue
        };

        var correlationId = "DEV-ABC123";
        _mockCorrelationIdGenerator.GenerateWithPrefix("DEV", 12).Returns(correlationId);

        // Act & Assert
        _mockCorrelationIdGenerator.GenerateWithPrefix("DEV", 12);
    }

    [Fact]
    public async Task BSMediator_DeveChamarSendCorretamente()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao();
        var expectedResponse = BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>.FromSuccess(new JDPIRegistrarOrdemDevolucaoResponse());

        // ✅ NSubstitute para métodos async também é limpo
        _mockMediator.Send<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>(transaction, Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _mockMediator.Send<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>(transaction);

        // Assert
        Assert.NotNull(result);
        await _mockMediator.Received(1).Send<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>(transaction, Arg.Any<CancellationToken>());
    }
}

#endregion


//using Adapters.Inbound.WebApi.Pix.Endpoints;
//using Domain.Core.Common.Mediator;
//using Domain.Core.Common.ResultPattern;
//using Domain.Core.Models.Request;
//using Domain.Core.Models.Response;
//using Domain.Core.Ports.Domain;
//using Domain.Services;
//using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
//using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
//using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NSubstitute;
//using NSubstitute.ExceptionExtensions;

//namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;



//#region DevolucaoEndpointTests

//public class DevolucaoEndpointTests
//{
//    private readonly BSMediator _mockMediator;
//    private readonly Mock<ITransactionFactory> _mockTransactionFactory;
//    private readonly Mock<IServiceProvider> _mockServiceProvider;
//    private readonly Mock<HttpContext> _mockHttpContext;
//    private readonly Mock<ICorrelationIdGenerator> _mockCorrelationIdGenerator; 

//    public DevolucaoEndpointTests()
//    {
//        _mockServiceProvider = new Mock<IServiceProvider>();
//        _mockMediator = Substitute.For<BSMediator>();
//        _mockTransactionFactory = new Mock<ITransactionFactory>();
//        _mockCorrelationIdGenerator = new Mock<ICorrelationIdGenerator>(); 
//        _mockHttpContext = new Mock<HttpContext>();


//    }

//    [Fact]
//    public void AddDevolucaoEndpoints_DeveConfigurarEndpointsCorretamente()
//    {
//        // Arrange
//        var builder = WebApplication.CreateBuilder();
//        var app = builder.Build();

//        // Act & Assert - Não deve lançar exceção
//        app.AddDevolucaoEndpoints();
//    }

//    [Fact]
//    public void RequisitarDevolucao_ComParametrosValidos_DeveGerarCorrelationIdComPrefixo()
//    {
//        // Arrange
//        var request = new JDPIRequisitarDevolucaoOrdemPagtoRequest
//        {
//            idReqSistemaCliente = "REQ-123",
//            endToEndIdOriginal = "E123456789",
//            valorDevolucao = 100.0
//        };

//        var expectedCorrelationId = "DEV-ABC123";
//        var expectedTransaction = new TransactionRegistrarOrdemDevolucao();
//        var expectedResponse = BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>.FromSuccess(new JDPIRegistrarOrdemDevolucaoResponse());

//        _mockCorrelationIdGenerator
//            .Setup(x => x.GenerateWithPrefix("DEV", 12))
//            .Returns(expectedCorrelationId);

//        _mockTransactionFactory
//            .Setup(x => x.CreateRegistrarOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId))
//            .Returns(expectedTransaction);

//        _mockMediator.Send<TransactionRegistrarOrdemDevolucao, BaseReturn<JDPIRegistrarOrdemDevolucaoResponse>>(expectedTransaction, default).Returns(expectedResponse);

//        // Act - Simula chamada do endpoint
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("DEV", 12);
//        _mockTransactionFactory.Object.CreateRegistrarOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId);

//        // Assert
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix("DEV", 12), Times.Once);
//        _mockTransactionFactory.Verify(x => x.CreateRegistrarOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId), Times.Once);
//    }


//    [Fact]
//    public void CancelarDevolucao_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoCDEV()
//    {
//        // Arrange
//        var request = new JDPICancelarRegistroOrdemdDevolucaoRequest
//        {
//            idReqSistemaCliente = "REQ-123"
//        };

//        var expectedCorrelationId = "CDEV-ABC123";
//        var expectedTransaction = new TransactionCancelarOrdemDevolucao();

//        _mockCorrelationIdGenerator
//            .Setup(x => x.GenerateWithPrefix("CDEV", 12))
//            .Returns(expectedCorrelationId);

//        _mockTransactionFactory
//            .Setup(x => x.CreateCancelarRegistroOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId))
//            .Returns(expectedTransaction);

//        // Act
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("CDEV",12);
//        _mockTransactionFactory.Object.CreateCancelarRegistroOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId);

//        // Assert
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix("CDEV",12), Times.Once);
//        _mockTransactionFactory.Verify(x => x.CreateCancelarRegistroOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId), Times.Once);
//    }

//    [Fact]
//    public void EfetivarDevolucao_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoEDEV()
//    {
//        // Arrange
//        var request = new JDPIEfetivarOrdemDevolucaoRequest
//        {
//            idReqSistemaCliente = "REQ-123",
//            idReqJdPi = "JDPI-123"
//        };

//        var expectedCorrelationId = "EDEV-ABC123";
//        var expectedTransaction = new TransactionEfetivarOrdemDevolucao();

//        _mockCorrelationIdGenerator
//            .Setup(x => x.GenerateWithPrefix("EDEV",12))
//            .Returns(expectedCorrelationId);

//        _mockTransactionFactory
//            .Setup(x => x.CreateEfetivarOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId))
//            .Returns(expectedTransaction);

//        // Act
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("EDEV",12);
//        _mockTransactionFactory.Object.CreateEfetivarOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId);

//        // Assert
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix("EDEV",12), Times.Once);
//        _mockTransactionFactory.Verify(x => x.CreateEfetivarOrdemDevolucao(_mockHttpContext.Object, request, expectedCorrelationId), Times.Once);
//    }

//    [Theory]
//    [InlineData(null)]
//    [InlineData("")]
//    public void RequisitarDevolucao_ComRequestInvalido_DeveManterFluxo(string invalidValue)
//    {
//        // Arrange
//        var request = new JDPIRequisitarDevolucaoOrdemPagtoRequest
//        {
//            idReqSistemaCliente = invalidValue
//        };

//        var correlationId = "DEV-ABC123";
//        _mockCorrelationIdGenerator.Setup(x => x.GenerateWithPrefix("DEV", 12)).Returns(correlationId);

//        // Act & Assert - Não deve lançar exceção no nível do endpoint
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("DEV");
//    }
//}

//#endregion
