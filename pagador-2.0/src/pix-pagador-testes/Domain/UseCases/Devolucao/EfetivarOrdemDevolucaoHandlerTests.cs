
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace pix_pagador_testes.Domain.UseCases.Devolucao;

public class EfetivarOrdemDevolucaoHandlerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;
    private readonly EfetivarOrdemDevolucaoHandler _handler;

    public EfetivarOrdemDevolucaoHandlerTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        // Setup ServiceProvider
        _mockServiceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _mockServiceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _mockServiceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);

        _handler = new EfetivarOrdemDevolucaoHandler(_mockServiceProvider);
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        // Act
        var instance = new EfetivarOrdemDevolucaoHandler(_mockServiceProvider);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithValidTransaction_ShouldReturnValid()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Configurar mocks para retornar tuplas (não ValidationResult)
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithInvalidParameters_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "",
            endToEndIdOriginal = "invalid",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };
        var endToEndErrors = new List<ErrorDetails>
        {
            new ErrorDetails("endToEndIdOriginal", "EndToEndId inválido")
        };

        // Configurar mocks para retornar tuplas com erros
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
            .Returns((endToEndErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
        Assert.Contains(result.Errors, e => e.campo == "endToEndIdOriginal");
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithValidTransaction_ShouldReturnSuccessResponse()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var expectedResult = "{\"chvAutorizador\":\"AUTH123\"}";
        _mockSpaRepository.EfetivarOrdemDevolucao(transaction).Returns(expectedResult);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JDPIEfetivarOrdemDevolucaoResponse>(result);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithBusinessException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var businessException = new BusinessException("Erro de negócio na efetivação");
        _mockSpaRepository.EfetivarOrdemDevolucao(transaction).Throws(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Erro de negócio na efetivação", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro retornado pela Sps", businessException);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithGenericException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var genericException = new Exception("Erro de conexão");
        _mockSpaRepository.EfetivarOrdemDevolucao(transaction).Throws(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Erro de conexão", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao de ordem de devolução", genericException);
    }

    [Fact]
    public void ReturnSuccessResponse_WithValidParameters_ShouldReturnBaseReturnSuccess()
    {
        // Arrange
        var response = new JDPIEfetivarOrdemDevolucaoResponse();
        var message = "Efetivação realizada com sucesso";
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = _handler.ReturnSuccessResponse(response, message, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(correlationId, result.CorrelationId);
        Assert.Equal(response, result.Data);
    }

    [Fact]
    public void ReturnErrorResponse_WithException_ShouldReturnBaseReturnFromException()
    {
        // Arrange
        var exception = new BusinessException("Erro na efetivação");
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = _handler.ReturnErrorResponse(exception, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(correlationId, result.CorrelationId);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ShouldCallAllValidatorsOnce()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            CorrelationId = Guid.NewGuid().ToString()
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        _mockValidatorService.Received(1).ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
        _mockValidatorService.Received(1).ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal);
    }

    [Theory]
    [InlineData("REQ123", "E12345678901234567890123456789012")]
    [InlineData("REQ456", "E98765432109876543210987654321098")]
    [InlineData("REQ789", "E11111111111111111111111111111111")]
    public async Task ExecuteSpecificValidations_WithVariousValidInputs_ShouldReturnValid(
        string idReqSistemaCliente, string endToEndIdOriginal)
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = idReqSistemaCliente,
            endToEndIdOriginal = endToEndIdOriginal,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithOnlyIdReqSistemaClienteInvalid_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("idReqSistemaCliente", result.Errors.First().campo);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithOnlyEndToEndIdOriginalInvalid_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var endToEndErrors = new List<ErrorDetails>
        {
            new ErrorDetails("endToEndIdOriginal", "EndToEndId original é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
            .Returns((endToEndErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("endToEndIdOriginal", result.Errors.First().campo);
    }
}

