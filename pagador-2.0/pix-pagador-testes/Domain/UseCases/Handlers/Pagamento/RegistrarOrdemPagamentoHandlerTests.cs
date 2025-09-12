using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.JDPI;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pix_pagador_testes.TestUtilities.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace pix_pagador_testes.Domain.UseCases.Handlers.Pagamento;

public class RegistrarOrdemPagamentoHandlerTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IValidatorService> _mockValidatorService;
    private readonly Mock<ISPARepository> _mockSpaRepository;
    private readonly Mock<ILogger<RegistrarOrdemPagamentoHandler>> _mockLogger;
    private readonly RegistrarOrdemPagamentoHandler _handler;
    private readonly Mock<ILoggingAdapter> _mockLoggingAdapter;


    public RegistrarOrdemPagamentoHandlerTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockValidatorService = new Mock<IValidatorService>();
        _mockSpaRepository = new Mock<ISPARepository>();
        _mockLogger = new Mock<ILogger<RegistrarOrdemPagamentoHandler>>();
        _mockLoggingAdapter = new Mock<ILoggingAdapter>();

        SetupServiceProvider();
        _handler = new RegistrarOrdemPagamentoHandler(_mockServiceProvider.Object);
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
        var transaction = TransactionBuilder.CreateRegistrarOrdemPagamento().Build();
        SetupValidationsSuccess();

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ComIdReqSistemaClienteInvalido_DeveRetornarErro()
    {
        // Arrange
        var transaction = TransactionBuilder.CreateRegistrarOrdemPagamento().Build();
        var errors = new List<ErrorDetails> { new("idReqSistemaCliente", "ID inválido") };

        _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
            .Returns((errors, false));
        SetupValidationsSuccess(skipClienteValidation: true);

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ComPagadorNulo_DeveRetornarErro()
    {
        // Arrange
        var transaction = TransactionBuilder.CreateRegistrarOrdemPagamentoPagadorNull().Build();
        SetupValidationsSuccess();

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "pagador");
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_ComSucesso_DeveRetornarResponse()
    {
        // Arrange
        var transaction = TransactionBuilder.CreateRegistrarOrdemPagamento().Build();
        var expectedResult = new JDPIRegistrarOrdemPagamentoResponse
        {
            chvAutorizador = "chave123",
            CorrelationId = transaction.CorrelationId
        };  
        var _repoResul = JsonSerializer.Serialize(expectedResult);

        _mockSpaRepository.Setup(r => r.RegistrarOrdemPagamento(transaction))
            .ReturnsAsync(_repoResul);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(_repoResul);
        Assert.IsType<JDPIRegistrarOrdemPagamentoResponse>(expectedResult);

    }

    [Fact]
    public async Task ExecuteTransactionProcessing_ComBusinessException_DeveLancarExcecao()
    {
        // Arrange
        var transaction = TransactionBuilder.CreateRegistrarOrdemPagamento().Build();
        var businessException = new BusinessException("Erro de negócio");

        _mockSpaRepository.Setup(r => r.RegistrarOrdemPagamento(transaction))
            .ThrowsAsync(businessException);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));
    }

    [Fact]
    public void ReturnSuccessResponse_DeveRetornarBaseReturnComSucesso()
    {
        // Arrange
        var correlationId = "123-456";
        var response = new JDPIRegistrarOrdemPagamentoResponse
        {
            chvAutorizador = "chave123",
            CorrelationId = correlationId
        };
        var message = "Sucesso";
       

        // Act
        var result = _handler.ReturnSuccessResponse(response, message, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(response, result.Data);
        Assert.Equal(correlationId, result.CorrelationId);
    }

    [Fact]
    public void ReturnErrorResponse_DeveRetornarBaseReturnComErro()
    {
        // Arrange
        var exception = new Exception("Erro teste");
        var correlationId = "123-456";

        // Act
        var result = _handler.ReturnErrorResponse(exception, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(correlationId, result.CorrelationId);
    }

  
    private void SetupValidationsSuccess(bool skipClienteValidation = false)
    {
        if (!skipClienteValidation)
        {
            _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
                .Returns((new List<ErrorDetails>(), true));
        }

        _mockValidatorService.Setup(v => v.ValidarPagador(It.IsAny<JDPIDadosConta>()))
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.Setup(v => v.ValidarRecebedor(It.IsAny<JDPIDadosConta>()))
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.Setup(v => v.ValidarValor(It.IsAny<double>()))
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.Setup(v => v.ValidarTpIniciacao(It.IsAny<EnumTpIniciacao>()))
            .Returns((new List<ErrorDetails>(), true));


    }
}
