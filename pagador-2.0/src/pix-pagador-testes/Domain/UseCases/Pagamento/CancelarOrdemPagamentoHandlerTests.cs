#region CancelarOrdemPagamentoHandlerTests

using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using pix_pagador_testes.TestUtilities.Builders;

namespace pix_pagador_testes.Domain.UseCases.Handlers.Pagamento;

public class CancelarOrdemPagamentoHandlerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;
    private readonly CancelarOrdemPagamentoHandler _handler;

    public CancelarOrdemPagamentoHandlerTests()
    {
        // Criar os mocks primeiro
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        // Configurar o ServiceProvider ANTES de criar o handler
        _mockServiceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _mockServiceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _mockServiceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);

        // Agora criar o handler com o service provider já configurado
        _handler = new CancelarOrdemPagamentoHandler(_mockServiceProvider);
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
        var instance = new CancelarOrdemPagamentoHandler(serviceProvider);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CancelarOrdemPagamentoHandler(null));
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithValidTransaction_ShouldReturnValid()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .ComMotivo("Cancelamento solicitado pelo cliente")
            .Build();

        // Configurar mocks para retornar tuplas (não ValidationResult)
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(transaction.motivo)
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
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("")
            .Build();

        var errors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };

        // Configurar mock para retornar tupla com erros
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((errors, false));
        _mockValidatorService.ValidarMotivo(transaction.motivo)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("idReqSistemaCliente", result.Errors.First().campo);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithInvalidMotivo_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComMotivo("")
            .Build();

        var motivoErrors = new List<ErrorDetails>
        {
            new ErrorDetails("motivo", "Motivo é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(transaction.motivo)
            .Returns((motivoErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("motivo", result.Errors.First().campo);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithValidTransaction_ShouldReturnSuccessResponse()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .Build();

        var expectedResult = "{\"chvAutorizador\":\"AUTH123\"}";
        _mockSpaRepository.CancelarOrdemPagamento(transaction).Returns(expectedResult);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JDPICancelarOrdemPagamentoResponse>(result);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithBusinessException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .Build();

        var businessException = new BusinessException("Erro de negócio");
        _mockSpaRepository.CancelarOrdemPagamento(transaction).Throws(businessException);

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
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .Build();

        var genericException = new Exception("Erro genérico");
        _mockSpaRepository.CancelarOrdemPagamento(transaction).Throws(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Erro genérico", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante cancelamento", genericException);
    }

    [Fact]
    public void ReturnSuccessResponse_WithValidParameters_ShouldReturnBaseReturnSuccess()
    {
        // Arrange
        var response = new JDPICancelarOrdemPagamentoResponse();
        var message = "Operação realizada com sucesso";
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
        var exception = new Exception("Erro de teste");
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = _handler.ReturnErrorResponse(exception, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(correlationId, result.CorrelationId);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_ShouldCallValidatorsOnce()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ123456789")
            .ComMotivo("Cancelamento teste")
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        _mockValidatorService.Received(1).ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
        _mockValidatorService.Received(1).ValidarMotivo(transaction.motivo);
    }

    [Theory]
    [InlineData("REQ123", "Cancelamento por solicitação do cliente")]
    [InlineData("REQ456", "Erro no processamento")]
    [InlineData("REQ789", "Solicitação de estorno")]
    public async Task ExecuteSpecificValidations_WithVariousValidInputs_ShouldReturnValid(
        string idReqSistemaCliente, string motivo)
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente(idReqSistemaCliente)
            .ComMotivo(motivo)
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithBothInvalidParameters_ShouldReturnAllErrors()
    {
        // Arrange
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("")
            .ComMotivo("")
            .Build();

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };
        var motivoErrors = new List<ErrorDetails>
        {
            new ErrorDetails("motivo", "Motivo é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarMotivo(transaction.motivo)
            .Returns((motivoErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
        Assert.Contains(result.Errors, e => e.campo == "motivo");
    }
}

#endregion