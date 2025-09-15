#region EfetivarOrdemPagamentoHandlerTests

using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using pix_pagador_testes.TestUtilities.Builders;

namespace pix_pagador_testes.Domain.UseCases.Handlers.Pagamento;

public class EfetivarOrdemPagamentoHandlerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;
    private readonly EfetivarOrdemPagamentoHandler _handler;

    public EfetivarOrdemPagamentoHandlerTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        // Setup ServiceProvider
        _mockServiceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _mockServiceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _mockServiceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);

        _handler = new EfetivarOrdemPagamentoHandler(_mockServiceProvider);
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        // Act
        var instance = new EfetivarOrdemPagamentoHandler(_mockServiceProvider);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithValidTransaction_ShouldReturnValid()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .ComEndToEndId("E12345678901234567890123456789012")
            .Build();

        // Configurar mocks para retornar tuplas (não ValidationResult)
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(transaction.endToEndId)
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
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("")
            .ComEndToEndId("invalid")
            .Build();

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };
        var endToEndErrors = new List<ErrorDetails>
        {
            new ErrorDetails("endToEndId", "EndToEndId inválido")
        };

        // Configurar mocks para retornar tuplas com erros
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarEndToEndId(transaction.endToEndId)
            .Returns((endToEndErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
        Assert.Contains(result.Errors, e => e.campo == "endToEndId");
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithValidTransaction_ShouldReturnSuccessResponse()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .ComEndToEndId("E12345678901234567890123456789012")
            .Build();

        var expectedResult = "{\"chvAutorizador\":\"AUTH123\"}";
        _mockSpaRepository.EfetivarOrdemPagamento(transaction).Returns(expectedResult);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JDPIEfetivarOrdemPagamentoResponse>(result);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithBusinessException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .Build();

        var businessException = new BusinessException("Erro de negócio na efetivação");
        _mockSpaRepository.EfetivarOrdemPagamento(transaction).Throws(businessException);

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
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .Build();

        var genericException = new Exception("Erro de conexão");
        _mockSpaRepository.EfetivarOrdemPagamento(transaction).Throws(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Erro de conexão", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao", genericException);
    }

    [Fact]
    public void ReturnSuccessResponse_WithValidParameters_ShouldReturnBaseReturnSuccess()
    {
        // Arrange
        var response = new JDPIEfetivarOrdemPagamentoResponse();
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
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .ComEndToEndId("E12345678901234567890123456789012")
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        _mockValidatorService.Received(1).ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
        _mockValidatorService.Received(1).ValidarEndToEndId(transaction.endToEndId);
    }

    [Theory]
    [InlineData("REQ123", "E12345678901234567890123456789012")]
    [InlineData("REQ456", "E98765432109876543210987654321098")]
    [InlineData("REQ789", "E11111111111111111111111111111111")]
    public async Task ExecuteSpecificValidations_WithVariousValidInputs_ShouldReturnValid(
        string idReqSistemaCliente, string endToEndId)
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente(idReqSistemaCliente)
            .ComEndToEndId(endToEndId)
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(Arg.Any<string>())
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
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("")
            .ComEndToEndId("E12345678901234567890123456789012")
            .Build();

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarEndToEndId(transaction.endToEndId)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("idReqSistemaCliente", result.Errors.First().campo);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithOnlyEndToEndIdInvalid_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .ComEndToEndId("")
            .Build();

        var endToEndErrors = new List<ErrorDetails>
        {
            new ErrorDetails("endToEndId", "EndToEndId é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(transaction.endToEndId)
            .Returns((endToEndErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("endToEndId", result.Errors.First().campo);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithTimeoutException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .Build();

        var timeoutException = new TimeoutException("Database timeout");
        _mockSpaRepository.EfetivarOrdemPagamento(transaction).Throws(timeoutException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Database timeout", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao", timeoutException);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithNullTransaction_ShouldHandleGracefully()
    {
        // Este teste verifica se o handler lida graciosamente com dados nulos
        // Arrange
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente(null)
            .ComEndToEndId(null)
            .Build();

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };
        var endToEndErrors = new List<ErrorDetails>
        {
            new ErrorDetails("endToEndId", "EndToEndId é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarEndToEndId(transaction.endToEndId)
            .Returns((endToEndErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}

#endregion