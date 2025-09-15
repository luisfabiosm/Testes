
#region RegistrarOrdemDevolucaoHandlerTests

using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace pix_pagador_testes.Domain.UseCases.Devolucao;
public class RegistrarOrdemDevolucaoHandlerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;
    private readonly RegistrarOrdemDevolucaoHandler _handler;

    public RegistrarOrdemDevolucaoHandlerTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        // Setup ServiceProvider
        _mockServiceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _mockServiceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _mockServiceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);

        _handler = new RegistrarOrdemDevolucaoHandler(_mockServiceProvider);
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        // Act
        var instance = new RegistrarOrdemDevolucaoHandler(_mockServiceProvider);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithValidTransaction_ShouldReturnValid()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            codigoDevolucao = "CD001",
            valorDevolucao = 100.50,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Configurar mocks para retornar tuplas (não ValidationResult)
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarCodigoDevolucao(transaction.codigoDevolucao)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(transaction.valorDevolucao)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithAllInvalidParameters_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "",
            endToEndIdOriginal = "invalid",
            codigoDevolucao = "",
            valorDevolucao = -10,
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
        var codigoErrors = new List<ErrorDetails>
        {
            new ErrorDetails("codigoDevolucao", "Código de devolução é obrigatório")
        };
        var valorErrors = new List<ErrorDetails>
        {
            new ErrorDetails("valorDevolucao", "Valor deve ser maior que zero")
        };

        // Configurar mocks para retornar tuplas com erros
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
            .Returns((endToEndErrors, false));
        _mockValidatorService.ValidarCodigoDevolucao(transaction.codigoDevolucao)
            .Returns((codigoErrors, false));
        _mockValidatorService.ValidarValor(transaction.valorDevolucao)
            .Returns((valorErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
        Assert.Contains(result.Errors, e => e.campo == "endToEndIdOriginal");
        Assert.Contains(result.Errors, e => e.campo == "codigoDevolucao");
        Assert.Contains(result.Errors, e => e.campo == "valorDevolucao");
    }

    [Theory]
    [InlineData("REQ123", "E12345678901234567890123456789012", "CD001", 100.50)]
    [InlineData("REQ456", "E98765432109876543210987654321098", "CD002", 250.75)]
    [InlineData("REQ789", "E11111111111111111111111111111111", "CD003", 50.25)]
    public async Task ExecuteSpecificValidations_WithVariousValidInputs_ShouldReturnValid(
        string idReqSistemaCliente, string endToEndIdOriginal, string codigoDevolucao, double valorDevolucao)
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = idReqSistemaCliente,
            endToEndIdOriginal = endToEndIdOriginal,
            codigoDevolucao = codigoDevolucao,
            valorDevolucao = valorDevolucao,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Configurar mocks para retornar tuplas válidas
        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarCodigoDevolucao(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(Arg.Any<double>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithValidTransaction_ShouldReturnSuccessResponse()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            codigoDevolucao = "CD001",
            valorDevolucao = 100.50,
            CorrelationId = Guid.NewGuid().ToString()
        };

        var expectedResult = "{\"chvAutorizador\":\"AUTH123\",\"endToEndIdDevolucao\":\"D123456789\"}";
        _mockSpaRepository.RegistrarOrdemDevolucao(transaction).Returns(expectedResult);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JDPIRegistrarOrdemDevolucaoResponse>(result);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithBusinessException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var businessException = new BusinessException("Devolução já registrada");
        _mockSpaRepository.RegistrarOrdemDevolucao(transaction).Throws(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Devolução já registrada", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro retornado pela Sps", businessException);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithGenericException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            CorrelationId = Guid.NewGuid().ToString()
        };

        var genericException = new Exception("Falha na comunicação com o banco");
        _mockSpaRepository.RegistrarOrdemDevolucao(transaction).Throws(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Falha na comunicação com o banco", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante registro de ordem de devolução", genericException);
    }

    [Fact]
    public void ReturnSuccessResponse_WithValidParameters_ShouldReturnBaseReturnSuccess()
    {
        // Arrange
        var response = new JDPIRegistrarOrdemDevolucaoResponse();
        var message = "Registro realizado com sucesso";
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
        var exception = new Exception("Erro no registro");
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
        var transaction = new TransactionRegistrarOrdemDevolucao
        {
            idReqSistemaCliente = "REQ123456789",
            endToEndIdOriginal = "E12345678901234567890123456789012",
            codigoDevolucao = "CD001",
            valorDevolucao = 100.50,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Configurar mocks para retornar tuplas válidas
        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarCodigoDevolucao(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(Arg.Any<double>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        _mockValidatorService.Received(1).ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
        _mockValidatorService.Received(1).ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal);
        _mockValidatorService.Received(1).ValidarCodigoDevolucao(transaction.codigoDevolucao);
        _mockValidatorService.Received(1).ValidarValor(transaction.valorDevolucao);
    }
}


#endregion

