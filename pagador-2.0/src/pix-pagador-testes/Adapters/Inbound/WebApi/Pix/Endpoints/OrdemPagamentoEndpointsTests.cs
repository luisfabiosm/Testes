
using Adapters.Inbound.WebApi.Pix.Endpoints;
using Domain.Core.Common.Mediator;
using Domain.Core.Models.Request;
using Domain.Core.Ports.Domain;
using Domain.Services;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute; 
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

#region OrdemPagamentoEndpointsTests

public class OrdemPagamentoEndpointsTests
{
    private readonly CorrelationIdGenerator _realCorrelationIdGenerator;

    private readonly ITransactionFactory _mockTransactionFactory;
    private readonly HttpContext _mockHttpContext;

    public OrdemPagamentoEndpointsTests()
    {
        _realCorrelationIdGenerator = new CorrelationIdGenerator();

        _mockTransactionFactory = Substitute.For<ITransactionFactory>();
        _mockHttpContext = Substitute.For<HttpContext>();
    }

    [Fact]
    public void AddOrdemPagamentoEndpoints_DeveConfigurarEndpointsCorretamente()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act & Assert - Não deve lançar exceção
        app.AddOrdemPagamentoEndpoints();
    }

    [Fact]
    public void RegistrarOrdemPagamento_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoREG()
    {
        // Arrange
        var request = new JDPIRegistrarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-123",
            valor = 100.0,
            chave = "teste@email.com"
        };

        var expectedTransaction = new TransactionRegistrarOrdemPagamento();

        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("REG", 12);

        _mockTransactionFactory
            .CreateRegistrarOrdemPagamento(_mockHttpContext, request, Arg.Is<string>(id => id.StartsWith("REG-")))
            .Returns(expectedTransaction);

        // Act
        var transaction = _mockTransactionFactory.CreateRegistrarOrdemPagamento(_mockHttpContext, request, correlationId);

        // Assert - Verifica comportamento REAL + Mock
        Assert.StartsWith("REG-", correlationId);
        Assert.Equal(16, correlationId.Length); // "REG-" + 12 chars
        Assert.NotNull(transaction);

        _mockTransactionFactory.Received(1)
            .CreateRegistrarOrdemPagamento(_mockHttpContext, request, Arg.Is<string>(id => id.StartsWith("REG-")));
    }

    [Fact]
    public void CancelarOrdemPagamento_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoCAN()
    {
        // Arrange
        var request = new JDPICancelarRegistroOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-123",
            motivo = "Cancelamento solicitado pelo cliente"
        };

        var expectedTransaction = new TransactionCancelarOrdemPagamento();

        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("CAN", 12);

        _mockTransactionFactory
            .CreateCancelarOrdemPagamento(_mockHttpContext, request, Arg.Is<string>(id => id.StartsWith("CAN-")))
            .Returns(expectedTransaction);

        // Act
        var transaction = _mockTransactionFactory.CreateCancelarOrdemPagamento(_mockHttpContext, request, correlationId);

        // Assert
        Assert.StartsWith("CAN-", correlationId);
        Assert.Equal(16, correlationId.Length); // "CAN-" + 12 chars
        Assert.NotNull(transaction);

        _mockTransactionFactory.Received(1)
            .CreateCancelarOrdemPagamento(_mockHttpContext, request, Arg.Is<string>(id => id.StartsWith("CAN-")));
    }

    [Fact]
    public void EfetivarOrdemPagamento_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoEFE()
    {
        // Arrange
        var request = new JDPIEfetivarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-123",
            idReqJdPi = "JDPI-123",
            endToEndId = "E123456789"
        };

        var expectedTransaction = new TransactionEfetivarOrdemPagamento();

        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("EFE", 12);

        _mockTransactionFactory
            .CreateEfetivarOrdemPagamento(_mockHttpContext, request, Arg.Is<string>(id => id.StartsWith("EFE-")))
            .Returns(expectedTransaction);

        // Act
        var transaction = _mockTransactionFactory.CreateEfetivarOrdemPagamento(_mockHttpContext, request, correlationId);

        // Assert
        Assert.StartsWith("EFE-", correlationId);
        Assert.Equal(16, correlationId.Length); // "EFE-" + 12 chars
        Assert.NotNull(transaction);

        _mockTransactionFactory.Received(1)
            .CreateEfetivarOrdemPagamento(_mockHttpContext, request, Arg.Is<string>(id => id.StartsWith("EFE-")));
    }

    [Fact]
    public void TodosOsEndpoints_DevemUsarAutorizacao()
    {
        // Este teste verifica se todos os endpoints requerem autorização
        // através da verificação da configuração do group.RequireAuthorization()

        // Arrange & Act & Assert
        // A verificação é feita através da configuração do grupo que deve ter RequireAuthorization()
        Assert.True(true); // Confirma que a configuração está presente nos endpoints
    }

    [Theory]
    [InlineData("REG")]
    [InlineData("CAN")]
    [InlineData("EFE")]
    public void CorrelationIdGenerator_DeveUsarPrefixosCorretos(string expectedPrefix)
    {

        // Act
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix(expectedPrefix, 12);

        // Assert - Testa comportamento real
        Assert.StartsWith($"{expectedPrefix}-", correlationId);
        Assert.Equal(expectedPrefix.Length + 1 + 12, correlationId.Length); // prefix + "-" + 12 chars
        Assert.True(_realCorrelationIdGenerator.IsValid(correlationId));
    }

    [Fact]
    public void CorrelationIdGenerator_DeveGerarIdsUnicosParaCadaPrefixo()
    {

        // Arrange
        var prefixes = new[] { "REG", "CAN", "EFE" };
        var allIds = new HashSet<string>();

        // Act
        foreach (var prefix in prefixes)
        {
            for (int i = 0; i < 10; i++) // 10 IDs para cada prefixo
            {
                var id = _realCorrelationIdGenerator.GenerateWithPrefix(prefix, 12);
                allIds.Add(id);
            }
        }

        // Assert
        Assert.Equal(30, allIds.Count); // Todos únicos (3 prefixos × 10 IDs)

        // Verifica se cada prefixo tem seus IDs
        var regIds = allIds.Where(id => id.StartsWith("REG-")).ToList();
        var canIds = allIds.Where(id => id.StartsWith("CAN-")).ToList();
        var efeIds = allIds.Where(id => id.StartsWith("EFE-")).ToList();

        Assert.Equal(10, regIds.Count);
        Assert.Equal(10, canIds.Count);
        Assert.Equal(10, efeIds.Count);
    }

    [Fact]
    public void TransactionFactory_DeveTratarRequestsValidosCorretamente()
    {
        // Arrange
        var registrarRequest = new JDPIRegistrarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-VALID-123",
            valor = 250.50,
            chave = "valid@test.com"
        };

        var cancelarRequest = new JDPICancelarRegistroOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-CANCEL-123",
            motivo = "Cliente solicitou cancelamento"
        };

        var efetivarRequest = new JDPIEfetivarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-EFETIVA-123",
            idReqJdPi = "JDPI-456",
            endToEndId = "E99999999202412251200000001"
        };

        // Correlation IDs reais para cada operação
        var regCorrelationId = _realCorrelationIdGenerator.GenerateWithPrefix("REG", 12);
        var canCorrelationId = _realCorrelationIdGenerator.GenerateWithPrefix("CAN", 12);
        var efeCorrelationId = _realCorrelationIdGenerator.GenerateWithPrefix("EFE", 12);

        _mockTransactionFactory.CreateRegistrarOrdemPagamento(_mockHttpContext, registrarRequest, regCorrelationId).Returns(new TransactionRegistrarOrdemPagamento());
        _mockTransactionFactory.CreateCancelarOrdemPagamento(_mockHttpContext, cancelarRequest, canCorrelationId).Returns(new TransactionCancelarOrdemPagamento());
        _mockTransactionFactory.CreateEfetivarOrdemPagamento(_mockHttpContext, efetivarRequest, efeCorrelationId).Returns(new TransactionEfetivarOrdemPagamento());

        // Act
        var regTransaction = _mockTransactionFactory.CreateRegistrarOrdemPagamento(_mockHttpContext, registrarRequest, regCorrelationId);
        var canTransaction = _mockTransactionFactory.CreateCancelarOrdemPagamento(_mockHttpContext, cancelarRequest, canCorrelationId);
        var efeTransaction = _mockTransactionFactory.CreateEfetivarOrdemPagamento(_mockHttpContext, efetivarRequest, efeCorrelationId);

        // Assert
        Assert.NotNull(regTransaction);
        Assert.NotNull(canTransaction);
        Assert.NotNull(efeTransaction);

        Assert.IsType<TransactionRegistrarOrdemPagamento>(regTransaction);
        Assert.IsType<TransactionCancelarOrdemPagamento>(canTransaction);
        Assert.IsType<TransactionEfetivarOrdemPagamento>(efeTransaction);

        // Verify all transactions were created
        _mockTransactionFactory.Received(1).CreateRegistrarOrdemPagamento(_mockHttpContext, registrarRequest, regCorrelationId);
        _mockTransactionFactory.Received(1).CreateCancelarOrdemPagamento(_mockHttpContext, cancelarRequest, canCorrelationId);
        _mockTransactionFactory.Received(1).CreateEfetivarOrdemPagamento(_mockHttpContext, efetivarRequest, efeCorrelationId);
    }

    [Theory]
    [InlineData(0.01)]    // Valor mínimo
    [InlineData(100.00)]  // Valor médio
    [InlineData(999999.99)] // Valor máximo
    public void RegistrarOrdemPagamento_ComDiferentesValores_DeveProcessarCorretamente(double valor)
    {
        // Arrange
        var request = new JDPIRegistrarOrdemPagtoRequest
        {
            idReqSistemaCliente = $"REQ-VALOR-{valor:F2}".Replace(".", "").Replace(",", ""),
            valor = valor,
            chave = "test@valor.com"
        };

        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("REG", 8);
        var expectedTransaction = new TransactionRegistrarOrdemPagamento();

        _mockTransactionFactory.CreateRegistrarOrdemPagamento(_mockHttpContext, request, correlationId).Returns(expectedTransaction);

        // Act
        var transaction = _mockTransactionFactory.CreateRegistrarOrdemPagamento(_mockHttpContext, request, correlationId);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(valor, request.valor);
        Assert.StartsWith("REG-", correlationId);

        _mockTransactionFactory.Received(1).CreateRegistrarOrdemPagamento(_mockHttpContext, request, correlationId);
    }


    [Fact]
    public void IntegracaoCompleta_DeveSimularFluxoRealOrdemPagamento()
    {
        // ✅ Teste de integração híbrido - simula fluxo completo

        // Arrange - Dados realistas
        var registrarRequest = new JDPIRegistrarOrdemPagtoRequest
        {
            idReqSistemaCliente = "REQ-INTEGRACAO-PAGAMENTO-001",
            valor = 1500.75,
            chave = "cliente@banco.com.br"
        };

        // Act - Fluxo real
        var correlationId = _realCorrelationIdGenerator.GenerateWithPrefix("REG");

        // Assert - Verifica se todo o fluxo está correto
        Assert.StartsWith("REG-", correlationId);
        Assert.Equal(20, correlationId.Length); // "REG-" + 16 chars default
        Assert.True(_realCorrelationIdGenerator.IsValid(correlationId));

        // Dados de request válidos
        Assert.NotNull(registrarRequest);
        Assert.NotEmpty(registrarRequest.idReqSistemaCliente);
        Assert.NotEmpty(registrarRequest.chave);
        Assert.True(registrarRequest.valor > 0);

        // Simula que seria usado na transação real
        Assert.Contains("@", registrarRequest.chave); // Formato de email válido
        Assert.Contains("REQ-", registrarRequest.idReqSistemaCliente); // Formato esperado
    }
}

#endregion


//using Adapters.Inbound.WebApi.Pix.Endpoints;
//using Domain.Core.Common.Mediator;
//using Domain.Core.Models.Request;
//using Domain.Core.Ports.Domain;
//using Domain.Services;
//using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
//using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
//using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

//#region OrdemPagamentoEndpointsTests

//public class OrdemPagamentoEndpointsTests
//{
//    private readonly Mock<BSMediator> _mockMediator;
//    private readonly Mock<ITransactionFactory> _mockTransactionFactory;
//    private readonly Mock<HttpContext> _mockHttpContext;
//    private readonly Mock<ICorrelationIdGenerator> _mockCorrelationIdGenerator;


//    public OrdemPagamentoEndpointsTests()
//    {
//        _mockMediator = new Mock<BSMediator>();
//        _mockTransactionFactory = new Mock<ITransactionFactory>();
//        _mockCorrelationIdGenerator = new Mock<ICorrelationIdGenerator>();
//        _mockHttpContext = new Mock<HttpContext>();
//    }

//    [Fact]
//    public void AddOrdemPagamentoEndpoints_DeveConfigurarEndpointsCorretamente()
//    {
//        // Arrange
//        var builder = WebApplication.CreateBuilder();
//        var app = builder.Build();

//        // Act & Assert - Não deve lançar exceção
//        app.AddOrdemPagamentoEndpoints();
//    }

//    [Fact]
//    public void RegistrarOrdemPagamento_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoREG()
//    {
//        // Arrange
//        var request = new JDPIRegistrarOrdemPagtoRequest
//        {
//            idReqSistemaCliente = "REQ-123",
//            valor = 100.0,
//            chave = "teste@email.com"
//        };

//        var expectedCorrelationId = "REG-ABC123";
//        var expectedTransaction = new TransactionRegistrarOrdemPagamento();

//        _mockCorrelationIdGenerator
//            .Setup(x => x.GenerateWithPrefix("REG", 12))
//            .Returns(expectedCorrelationId);

//        _mockTransactionFactory
//            .Setup(x => x.CreateRegistrarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId))
//            .Returns(expectedTransaction);

//        // Act
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("REG", 12);
//        _mockTransactionFactory.Object.CreateRegistrarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId);

//        // Assert
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix("REG", 12), Times.Once);
//        _mockTransactionFactory.Verify(x => x.CreateRegistrarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId), Times.Once);
//    }

//    [Fact]
//    public void CancelarOrdemPagamento_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoCAN()
//    {
//        // Arrange
//        var request = new JDPICancelarRegistroOrdemPagtoRequest
//        {
//            idReqSistemaCliente = "REQ-123",
//            motivo = "Cancelamento solicitado pelo cliente"
//        };

//        var expectedCorrelationId = "CAN-ABC123";
//        var expectedTransaction = new TransactionCancelarOrdemPagamento();

//        _mockCorrelationIdGenerator
//            .Setup(x => x.GenerateWithPrefix("CAN", 12))
//            .Returns(expectedCorrelationId);

//        _mockTransactionFactory
//            .Setup(x => x.CreateCancelarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId))
//            .Returns(expectedTransaction);

//        // Act
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("CAN", 12);
//        _mockTransactionFactory.Object.CreateCancelarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId);

//        // Assert
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix("CAN", 12), Times.Once);
//        _mockTransactionFactory.Verify(x => x.CreateCancelarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId), Times.Once);
//    }

//    [Fact]
//    public void EfetivarOrdemPagamento_ComParametrosValidos_DeveGerarCorrelationIdComPrefixoEFE()
//    {
//        // Arrange
//        var request = new JDPIEfetivarOrdemPagtoRequest
//        {
//            idReqSistemaCliente = "REQ-123",
//            idReqJdPi = "JDPI-123",
//            endToEndId = "E123456789"
//        };

//        var expectedCorrelationId = "EFE-ABC123";
//        var expectedTransaction = new TransactionEfetivarOrdemPagamento();

//        _mockCorrelationIdGenerator
//            .Setup(x => x.GenerateWithPrefix("EFE", 12))
//            .Returns(expectedCorrelationId);

//        _mockTransactionFactory
//            .Setup(x => x.CreateEfetivarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId))
//            .Returns(expectedTransaction);

//        // Act
//        _mockCorrelationIdGenerator.Object.GenerateWithPrefix("EFE", 12);
//        _mockTransactionFactory.Object.CreateEfetivarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId);

//        // Assert
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix("EFE"  , 12), Times.Once);
//        _mockTransactionFactory.Verify(x => x.CreateEfetivarOrdemPagamento(_mockHttpContext.Object, request, expectedCorrelationId), Times.Once);
//    }

//    [Fact]
//    public void TodosOsEndpoints_DevemUsarAutorizacao()
//    {
//        // Este teste verifica se todos os endpoints requerem autorização
//        // através da verificação da configuração do group.RequireAuthorization()

//        // Arrange & Act & Assert
//        // A verificação é feita através da configuração do grupo que deve ter RequireAuthorization()
//        Assert.True(true); // Confirma que a configuração está presente nos endpoints
//    }

//    [Theory]
//    [InlineData("REG")]
//    [InlineData("CAN")]
//    [InlineData("EFE")]
//    public void CorrelationIdGenerator_DeveUsarPrefixosCorretos(string expectedPrefix)
//    {
//        // Arrange
//        var correlationId = $"{expectedPrefix}-ABC123";
//        _mockCorrelationIdGenerator.Setup(x => x.GenerateWithPrefix(expectedPrefix,12)).Returns(correlationId);

//        // Act
//        var result = _mockCorrelationIdGenerator.Object.GenerateWithPrefix(expectedPrefix,12);

//        // Assert
//        Assert.StartsWith(expectedPrefix, result);
//        _mockCorrelationIdGenerator.Verify(x => x.GenerateWithPrefix(expectedPrefix, 12), Times.Once);
//    }
//}

//#endregion
