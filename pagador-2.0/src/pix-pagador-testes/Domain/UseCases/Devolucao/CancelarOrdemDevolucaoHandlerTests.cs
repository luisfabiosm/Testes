#region CancelarOrdemDevolucaoHandlerTests

using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace pix_pagador_testes.Domain.UseCases.Devolucao;

public class CancelarOrdemDevolucaoHandlerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;
    private readonly CancelarOrdemDevolucaoHandler _handler;

    public CancelarOrdemDevolucaoHandlerTests()
    {
        // Criar os mocks primeiro
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        // Configurar o ServiceProvider ANTES de criar o handler
        // O handler precisa resolver estes serviços no construtor
        _mockServiceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _mockServiceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _mockServiceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);

        // Agora criar o handler com o service provider já configurado
        _handler = new CancelarOrdemDevolucaoHandler(_mockServiceProvider);
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        // Arrange - Configurar um novo service provider para este teste
        var serviceProvider = Substitute.For<IServiceProvider>();
        var validatorService = Substitute.For<IValidatorService>();
        var spaRepository = Substitute.For<ISPARepository>();
        var loggingAdapter = Substitute.For<ILoggingAdapter>();

        serviceProvider.GetService<IValidatorService>().Returns(validatorService);
        serviceProvider.GetService<ISPARepository>().Returns(spaRepository);
        serviceProvider.GetService(typeof(ILoggingAdapter)).Returns(loggingAdapter);

        // Act
        var instance = new CancelarOrdemDevolucaoHandler(serviceProvider);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CancelarOrdemDevolucaoHandler(null));
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithValidTransaction_ShouldReturnValid()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Configurar mock para retornar tupla (não ValidationResult)
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithInvalidIdReqSistemaCliente_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var errors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };

        // Configurar mock para retornar tupla com erros
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((errors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("idReqSistemaCliente", result.Errors.First().campo);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithValidTransaction_ShouldReturnSuccessResponse()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var expectedResult = "{\"chvAutorizador\":\"AUTH123\"}";
        _mockSpaRepository.CancelarOrdemDevolucao(transaction).Returns(expectedResult);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JDPICancelarOrdemDevolucaoResponse>(result);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithBusinessException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var businessException = new BusinessException("Erro de negócio");
        _mockSpaRepository.CancelarOrdemDevolucao(transaction).Throws(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Erro de negócio", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro retornado pela Sps", businessException);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithGenericException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var genericException = new Exception("Erro genérico");
        _mockSpaRepository.CancelarOrdemDevolucao(transaction).Throws(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Erro genérico", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante cancelamento de ordem de devolução", genericException);
    }

    [Fact]
    public void ReturnSuccessResponse_WithValidParameters_ShouldReturnBaseReturnSuccess()
    {
        // Arrange
        var response = new JDPICancelarOrdemDevolucaoResponse();
        var message = "Operação realizada com sucesso";
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = _handler.ReturnSuccessResponse(response, message, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success); // Corrigido: usar Sucesso em vez de Success
        Assert.Equal(message, result.Message); // Corrigido: usar Mensagem em vez de Message
        Assert.Equal(correlationId, result.CorrelationId);
        Assert.Equal(response, result.Data);
    }

    [Fact]
    public void ReturnErrorResponse_WithException_ShouldReturnBaseReturnFromException()
    {
        // Arrange
        var exception = new Exception("Erro de teste");
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = _handler.ReturnErrorResponse(exception, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success); // Corrigido: usar Sucesso em vez de Success
        Assert.Equal(correlationId, result.CorrelationId);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ShouldCallValidatorOnce()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        _mockValidatorService.Received(1).ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
    }

    [Theory]
    [InlineData("REQ123")]
    [InlineData("REQ456")]
    [InlineData("REQ789")]
    public async Task ExecuteSpecificValidations_WithVariousValidInputs_ShouldReturnValid(string idReqSistemaCliente)
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemDevolucao
        {
            idReqSistemaCliente = idReqSistemaCliente,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

#endregion