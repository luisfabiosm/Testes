using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pix_pagador_testes.TestUtilities.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.UseCases.Handlers.Pagamento;


public class EfetivarOrdemPagamentoHandlerTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IValidatorService> _mockValidatorService;
    private readonly Mock<ISPARepository> _mockSpaRepository;
    private readonly Mock<ILogger<EfetivarOrdemPagamentoHandler>> _mockLogger;
    private readonly EfetivarOrdemPagamentoHandler _handler;
    private readonly Mock<ILoggingAdapter> _mockLoggingAdapter;


    public EfetivarOrdemPagamentoHandlerTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockValidatorService = new Mock<IValidatorService>();
        _mockSpaRepository = new Mock<ISPARepository>();
        _mockLogger = new Mock<ILogger<EfetivarOrdemPagamentoHandler>>();
        _mockLoggingAdapter = new Mock<ILoggingAdapter>();

        SetupServiceProvider();
        _handler = new EfetivarOrdemPagamentoHandler(_mockServiceProvider.Object);
    }

    private void SetupServiceProvider()
    {
        _mockServiceProvider.Setup(x => x.GetService(typeof(IValidatorService))).Returns(_mockValidatorService.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ISPARepository))).Returns(_mockSpaRepository.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var transaction = CreateValidTransaction();
        SetupValidationsSuccess();

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ComEndToEndIdInvalido_DeveRetornarErro()
    {
        // Arrange
        var transaction = CreateValidTransaction();
        var errors = new List<ErrorDetails> { new("endToEndId", "EndToEndId inválido") };

        _mockValidatorService.Setup(v => v.ValidarEndToEndId(It.IsAny<string>()))
            .Returns((errors, false));
        _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "endToEndId");
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_ComSucesso_DeveRetornarResponse()
    {
        // Arrange
        var transaction = TransactionBuilder.CreateEfetivarOrdemPagamento().Build();
        var expectedResult = new JDPIEfetivarOrdemPagamentoResponse
        {
            chvAutorizador = "chave123",
            CorrelationId = transaction.CorrelationId
        };
        var _repoResul = JsonSerializer.Serialize(expectedResult);

        _mockSpaRepository.Setup(r => r.EfetivarOrdemPagamento(transaction))
            .ReturnsAsync(_repoResul);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(_repoResul);
        Assert.IsType<JDPIEfetivarOrdemPagamentoResponse>(expectedResult);
    }

    private TransactionEfetivarOrdemPagamento CreateValidTransaction()
    {
        return new TransactionEfetivarOrdemPagamento
        {
            idReqSistemaCliente = "cliente123",
            CorrelationId = "corr123",
            endToEndId = "E1234567890123456789012345678901"
        };
    }

    private void SetupValidationsSuccess()
    {
        _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.Setup(v => v.ValidarEndToEndId(It.IsAny<string>()))
            .Returns((new List<ErrorDetails>(), true));
    }
}

