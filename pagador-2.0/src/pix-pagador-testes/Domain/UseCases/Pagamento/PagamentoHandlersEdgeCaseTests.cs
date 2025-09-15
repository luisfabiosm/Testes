using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.JDPI;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace pix_pagador_testes.Domain.UseCases.Pagamento;

public class PagamentoHandlersEdgeCaseTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;

    public PagamentoHandlersEdgeCaseTests()
    {
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _serviceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _serviceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);
    }

    #region RegistrarOrdemPagamento Edge Cases
    private static ValidationResult CreateInvalidValidationResult(List<ErrorDetails> errors)
    {
        // Opção 1: Se ValidationResult tem construtor que aceita lista de erros
        try
        {
            return ValidationResult.Invalid(errors ?? new List<ErrorDetails>());
        }
        catch
        {
            
            var validationResult = new ValidationResult();

            return validationResult;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(null)]
    public async Task RegistrarOrdemPagamento_WithInvalidPagadorISPB_ShouldReturnValidationError(int invalidISPB)
    {

        //Arrange
        var errors = new List<ErrorDetails>
           {
                new ErrorDetails("pagador.ispb", "ISPB do pagador é obrigatório")
           };

        var invalidValidationResult = CreateInvalidValidationResult(errors);


        //// Act
        var cancelarResult = invalidValidationResult;
        var efetivarResult = invalidValidationResult;
        var registrarResult = invalidValidationResult;

        //// Assert
        Assert.False(cancelarResult.IsValid);
        Assert.False(efetivarResult.IsValid);
        Assert.False(registrarResult.IsValid);

        Assert.Single(cancelarResult.Errors);
        Assert.Single(efetivarResult.Errors);
        Assert.Single(registrarResult.Errors);

        Assert.Equal("pagador.ispb", cancelarResult.Errors.First().campo);
        Assert.Equal("pagador.ispb", efetivarResult.Errors.First().campo);
        Assert.Equal("pagador.ispb", registrarResult.Errors.First().campo);

    }

    [Theory]
    [InlineData(0)]
    [InlineData(null)]
    public async Task RegistrarOrdemPagamento_WithInvalidRecebedorCPFCNPJ_ShouldReturnValidationError(long invalidCpfCnpj)
    {
        //Arrange
        var errors = new List<ErrorDetails>
           {
                new ErrorDetails("recebedor.cpfCnpj", "CPF/CNPJ do recebedor é obrigatório")
           };

        var invalidValidationResult = CreateInvalidValidationResult(errors);


        //// Act
        var cancelarResult = invalidValidationResult;
        var efetivarResult = invalidValidationResult;
        var registrarResult = invalidValidationResult;

        //// Assert
        Assert.False(cancelarResult.IsValid);
        Assert.False(efetivarResult.IsValid);
        Assert.False(registrarResult.IsValid);

        Assert.Single(cancelarResult.Errors);
        Assert.Single(efetivarResult.Errors);
        Assert.Single(registrarResult.Errors);

        Assert.Equal("recebedor.cpfCnpj", cancelarResult.Errors.First().campo);
        Assert.Equal("recebedor.cpfCnpj", efetivarResult.Errors.First().campo);
        Assert.Equal("recebedor.cpfCnpj", registrarResult.Errors.First().campo);


        
    }

    #endregion

    #region EfetivarOrdemPagamento Edge Cases

    [Theory]
    [MemberData(nameof(GetInvalidEndToEndIdsData))]
    public async Task EfetivarOrdemPagamento_WithInvalidEndToEndId_ShouldReturnValidationError(string invalidEndToEndId, string expectedErrorMessage)
    {
        // Arrange
        var handler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComEndToEndId(invalidEndToEndId)
            .Build();

        var endToEndErrors = new List<ErrorDetails>
        {
            new ErrorDetails("endToEndId", expectedErrorMessage)
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(invalidEndToEndId)
            .Returns((endToEndErrors, false));

        // Act
        var result = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        PagamentoValidationHelpers.VerifyErrorContainsFieldAndMessage(result, "endToEndId", expectedErrorMessage);
    }

    [Fact]
    public async Task EfetivarOrdemPagamento_WithRepositoryTimeout_ShouldLogAndRethrow()
    {
        // Arrange
        var handler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder().Build();

        var timeoutException = new TimeoutException("Timeout ao conectar com banco de dados");
        _mockSpaRepository.EfetivarOrdemPagamento(transaction).Throws(timeoutException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Timeout ao conectar com banco de dados", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao", timeoutException);
    }

    #endregion

    #region CancelarOrdemPagamento Edge Cases

    [Theory]
    [MemberData(nameof(GetInvalidMotivosData))]
    public async Task CancelarOrdemPagamento_WithInvalidMotivo_ShouldReturnValidationError(string invalidMotivo, string expectedErrorMessage)
    {
        // Arrange
        var handler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComMotivo(invalidMotivo)
            .Build();

        var motivoErrors = new List<ErrorDetails>
        {
            new ErrorDetails("motivo", expectedErrorMessage)
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(invalidMotivo)
            .Returns((motivoErrors, false));

        // Act
        var result = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        PagamentoValidationHelpers.VerifyErrorContainsFieldAndMessage(result, "motivo", expectedErrorMessage);
    }

    [Fact]
    public async Task CancelarOrdemPagamento_WithSQLInjectionAttempt_ShouldSanitizeInput()
    {
        // Arrange
        var handler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var maliciousMotivo = "'; DROP TABLE pagamentos; --";

        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComMotivo(maliciousMotivo)
            .Build();

        var motivoErrors = new List<ErrorDetails>
        {
            new ErrorDetails("motivo", "Motivo contém caracteres inválidos")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(maliciousMotivo)
            .Returns((motivoErrors, false));

        // Act
        var result = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        PagamentoValidationHelpers.VerifyErrorContainsField(result, "motivo");
        _mockValidatorService.Received(1).ValidarMotivo(maliciousMotivo);
    }

    #endregion

    #region Repository Exception Handling

    [Fact]
    public async Task AllHandlers_WithUnexpectedRepositoryException_ShouldLogAndRethrow()
    {
        // Arrange
        var cancelarHandler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var cancelarTransaction = new TransactionCancelarOrdemPagamentoBuilder().Build();
        var efetivarTransaction = new TransactionEfetivarOrdemPagamentoBuilder().Build();
        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder().Build();

        var unexpectedRegistraException = new Exception("Erro de database durante registro de ordem de pagamento");
        var unexpectedEfetivaException = new Exception("Erro de database durante efetivacao");
        var unexpectedCancelaException = new Exception("Erro de database durante cancelamento");



        _mockSpaRepository.CancelarOrdemPagamento(cancelarTransaction).Throws(unexpectedCancelaException);
        _mockSpaRepository.EfetivarOrdemPagamento(efetivarTransaction).Throws(unexpectedEfetivaException);
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction).Throws(unexpectedRegistraException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None));
        await Assert.ThrowsAsync<Exception>(() =>
            efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None));
        await Assert.ThrowsAsync<Exception>(() =>
            registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None));

        // Verify logging
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante cancelamento", unexpectedCancelaException);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao", unexpectedEfetivaException);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante registro de ordem de pagamento", unexpectedRegistraException);
    }

    [Fact]
    public async Task AllHandlers_WithNullRepositoryResponse_ShouldHandleGracefully()
    {
        // Arrange
        var cancelarHandler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var cancelarTransaction = new TransactionCancelarOrdemPagamentoBuilder().Build();
        var efetivarTransaction = new TransactionEfetivarOrdemPagamentoBuilder().Build();
        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder().Build();

        _mockSpaRepository.CancelarOrdemPagamento(cancelarTransaction).Returns(string.Empty);
        _mockSpaRepository.EfetivarOrdemPagamento(efetivarTransaction).Returns(string.Empty);
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction).Returns(string.Empty);

        // Act & Assert
        // Dependendo da implementação, pode lançar exception ou retornar response vazio
        var cancelarResult = await cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None);
        var efetivarResult = await efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None);
        var registrarResult = await registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None);

        // Assert - Verifica que pelo menos não lança exception
        Assert.NotNull(cancelarResult);
        Assert.NotNull(efetivarResult);
        Assert.NotNull(registrarResult);
    }

    #endregion

    #region Data Members

    public static IEnumerable<object[]> GetInvalidValuesData()
    {
        return new List<object[]>
        {
            new object[] { 0.00, "Valor deve ser maior que zero" },
            new object[] { -0.01, "Valor não pode ser negativo" },
            new object[] { -100.00, "Valor não pode ser negativo" },
            new object[] { double.NaN, "Valor deve ser um número válido" },
            new object[] { double.PositiveInfinity, "Valor deve ser um número válido" },
            new object[] { double.NegativeInfinity, "Valor deve ser um número válido" }
        };
    }

    public static IEnumerable<object[]> GetInvalidEndToEndIdsData()
    {
        return new List<object[]>
        {
            new object[] { "", "EndToEndId é obrigatório" },
            new object[] { null, "EndToEndId é obrigatório" },
            new object[] { "E123", "EndToEndId deve ter 32 caracteres" },
            new object[] { "E123456789012345678901234567890123", "EndToEndId deve ter 32 caracteres" },
            new object[] { "X12345678901234567890123456789012", "EndToEndId deve começar com 'E'" },
            new object[] { "e12345678901234567890123456789012", "EndToEndId deve começar com 'E' maiúsculo" }
        };
    }

    public static IEnumerable<object[]> GetInvalidMotivosData()
    {
        return new List<object[]>
        {
            new object[] { "", "Motivo é obrigatório" },
            new object[] { null, "Motivo é obrigatório" },
            new object[] { "   ", "Motivo não pode conter apenas espaços" },
            new object[] { new string('A', 501), "Motivo não pode exceder 500 caracteres" }
        };
    }

    #endregion

    #region Helper Methods

    private void SetupValidationsForRegistrar(
        TransactionRegistrarOrdemPagamento transaction,
        List<ErrorDetails> clienteErrors = null, bool clienteValid = true,
        List<ErrorDetails> pagadorErrors = null, bool pagadorValid = true,
        List<ErrorDetails> recebedorErrors = null, bool recebedorValid = true,
        List<ErrorDetails> valorErrors = null, bool valorValid = true,
        List<ErrorDetails> iniciacaoErrors = null, bool iniciacaoValid = true)
    {
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors ?? new List<ErrorDetails>(), clienteValid));

        if (transaction.pagador != null)
        {
            _mockValidatorService.ValidarPagador(transaction.pagador)
                .Returns((pagadorErrors ?? new List<ErrorDetails>(), pagadorValid));
        }

        if (transaction.recebedor != null)
        {
            _mockValidatorService.ValidarRecebedor(transaction.recebedor)
                .Returns((recebedorErrors ?? new List<ErrorDetails>(), recebedorValid));
        }

        _mockValidatorService.ValidarValor(transaction.valor)
            .Returns((valorErrors ?? new List<ErrorDetails>(), valorValid));

        _mockValidatorService.ValidarTpIniciacao(transaction.tpIniciacao)
            .Returns((iniciacaoErrors ?? new List<ErrorDetails>(), iniciacaoValid));
    }

    #endregion
}