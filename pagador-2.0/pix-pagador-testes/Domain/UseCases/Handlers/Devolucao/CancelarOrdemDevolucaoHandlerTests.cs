//using Domain.Core.Common.Transaction;
//using Domain.Core.Exceptions;
//using Domain.Core.Models.Response;
//using Domain.Core.Ports.Domain;
//using Domain.Core.Ports.Outbound;
//using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using pix_pagador_testes.TestUtilities.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Transactions;

//namespace pix_pagador_testes.Domain.UseCases.Handlers.Devolucao;

//public class CancelarOrdemDevolucaoHandlerTests
//{
//    private readonly Mock<IServiceProvider> _mockServiceProvider;
//    private readonly Mock<IValidatorService> _mockValidatorService;
//    private readonly Mock<ISPARepository> _mockSpaRepository;
//    private readonly Mock<ILoggingAdapter> _mockLoggingAdapter;
//    private readonly Mock<ILogger<CancelarOrdemDevolucaoHandler>> _mockLogger;
//    private readonly CancelarOrdemDevolucaoHandler _handler;

//    public CancelarOrdemDevolucaoHandlerTests()
//    {
//        _mockServiceProvider = new Mock<IServiceProvider>();
//        _mockValidatorService = new Mock<IValidatorService>();
//        _mockSpaRepository = new Mock<ISPARepository>();
//        _mockLogger = new Mock<ILogger<CancelarOrdemDevolucaoHandler>>();
//        _mockLoggingAdapter = new Mock<ILoggingAdapter>();

//        SetupServiceProvider();
//        _handler = new CancelarOrdemDevolucaoHandler(_mockServiceProvider.Object);
//    }

//    private void SetupServiceProvider()
//    {
//        _mockServiceProvider.Setup(x => x.GetService(typeof(IValidatorService))).Returns(_mockValidatorService.Object);
//        _mockServiceProvider.Setup(x => x.GetService(typeof(ISPARepository))).Returns(_mockSpaRepository.Object);
//        _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);

//    }

//    [Fact]
//    public async Task ExecuteSpecificValidations_ComDadosValidos_DeveRetornarSucesso()
//    {
//        // Arrange
//        var transaction = CreateValidTransaction();
//        SetupValidationsSuccess();

//        // Act
//        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsValid);
//        Assert.Empty(result.Errors);
//    }

//    [Fact]
//    public async Task ExecuteSpecificValidations_ComIdReqSistemaClienteInvalido_DeveRetornarErro()
//    {
//        // Arrange
//        var transaction = CreateValidTransaction();
//        var errors = new List<ErrorDetails> { new("idReqSistemaCliente", "ID sistema cliente inválido") };

//        _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
//            .Returns((errors, false));

//        // Act
//        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsValid);
//        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
//    }

//    [Fact]
//    public async Task ExecuteTransactionProcessing_ComSucesso_DeveRetornarResponse()
//    {
//        // Arrange
//        var transaction = TransactionBuilder.CreateCancelarOrdemDevolucao().Build();
//        var expectedResult = new JDPICancelarOrdemDevolucaoResponse
//        {
//            chvAutorizador = "chave123",
//            CorrelationId = transaction.CorrelationId
//        };

//        var _repoResul = JsonSerializer.Serialize(expectedResult);

//        _mockSpaRepository.Setup(r => r.CancelarOrdemDevolucao(transaction))
//            .ReturnsAsync(_repoResul);

//        // Act
//        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

//        // Assert
//        Assert.NotNull(_repoResul);
//        Assert.IsType<JDPICancelarOrdemDevolucaoResponse>(expectedResult);
//    }

//    [Fact]
//    public async Task ExecuteTransactionProcessing_ComBusinessException_DeveLancarExcecao()
//    {
//        // Arrange
//        var transaction = TransactionBuilder.CreateCancelarOrdemDevolucao().Build(); 
//        var businessException = new BusinessException("Erro de negócio na devolução");

//        _mockSpaRepository.Setup(r => r.CancelarOrdemDevolucao(transaction))
//            .ThrowsAsync(businessException);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
//            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

//        Assert.Equal("Erro de negócio na devolução", exception.Message);
//    }

//    [Fact]
//    public void ReturnSuccessResponse_DeveRetornarBaseReturnComSucesso()
//    {
//        // Arrange
//        var correlationId = "123-456";

//        var response = new JDPICancelarOrdemDevolucaoResponse
//        {
//            chvAutorizador = "chave123",
//            CorrelationId = correlationId
//        };
//        var message = "Cancelamento realizado com sucesso";

//        // Act
//        var result = _handler.ReturnSuccessResponse(response, message, correlationId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(response, result.Data);
//        Assert.Equal(correlationId, result.CorrelationId);

//    }

//    [Fact]
//    public void ReturnErrorResponse_DeveRetornarBaseReturnComErro()
//    {
//        // Arrange
//        var exception = new Exception("Erro teste");
//        var correlationId = "123-456";

//        // Act
//        var result = _handler.ReturnErrorResponse(exception, correlationId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(correlationId, result.CorrelationId);
//    }

//    private TransactionCancelarOrdemDevolucao CreateValidTransaction()
//    {
//        return new TransactionCancelarOrdemDevolucao
//        {
//            idReqSistemaCliente = "cliente123",
//            CorrelationId = "corr123"
//        };
//    }

//    private void SetupValidationsSuccess()
//    {
//        _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
//            .Returns((new List<ErrorDetails>(), true));
//    }
//}
