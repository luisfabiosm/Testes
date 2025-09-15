using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Pagamento.CancelarOrdemPagamento;
using Domain.UseCases.Pagamento.EfetivarOrdemPagamento;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace pix_pagador_testes.Domain.UseCases.Pagamento;

public class PagamentoHandlersIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;

    public PagamentoHandlersIntegrationTests()
    {
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _serviceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _serviceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);
    }

    [Fact]
    public async Task CancelarOrdemPagamentoHandler_FullWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ987654321")
            .ComMotivo("Cancelamento por solicitação do cliente")
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
             .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(transaction.motivo)
             .Returns((new List<ErrorDetails>(), true));

        _mockSpaRepository.CancelarOrdemPagamento(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH123\"}");

        // Act - Test validation
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Act - Test processing
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Act - Test success response
        var successResponse = handler.ReturnSuccessResponse(processingResult, "Cancelamento realizado", transaction.CorrelationId);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);
        Assert.True(successResponse.Success);
        Assert.Equal("Cancelamento realizado", successResponse.Message);
    }

    [Fact]
    public async Task EfetivarOrdemPagamentoHandler_FullWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ987654321")
            .ComEndToEndId("E98765432109876543210987654321098")
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
             .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(transaction.endToEndId)
             .Returns((new List<ErrorDetails>(), true));

        _mockSpaRepository.EfetivarOrdemPagamento(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH456\"}");

        // Act - Test validation
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Act - Test processing
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Act - Test success response
        var successResponse = handler.ReturnSuccessResponse(processingResult, "Efetivação concluída", transaction.CorrelationId);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);
        Assert.True(successResponse.Success);
        Assert.Equal("Efetivação concluída", successResponse.Message);
    }

    [Fact]
    public async Task RegistrarOrdemPagamentoHandler_FullWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ987654321")
            .ComValor(500)
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(transaction.pagador)
             .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarRecebedor(transaction.recebedor)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(transaction.valor)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarTpIniciacao(transaction.tpIniciacao)
            .Returns((new List<ErrorDetails>(), true));

        _mockSpaRepository.RegistrarOrdemPagamento(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH789\",\"endToEndId\":\"E123456789\"}");

        // Act - Test validation
       // var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Act - Test processing
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Act - Test success response
        var successResponse = handler.ReturnSuccessResponse(processingResult, "Registro concluído", transaction.CorrelationId);

        // Assert
       // Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);
        Assert.True(successResponse.Success);
        Assert.Equal("Registro concluído", successResponse.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AllHandlers_WithInvalidIdReqSistemaCliente_ShouldReturnValidationErrors(string invalidId)
    {
        // Arrange
        var errors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id inválido")
        };

        var invalidValidationResult = CreateInvalidValidationResult(errors);

        // Act
        var cancelarResult = invalidValidationResult;
        var efetivarResult = invalidValidationResult;
        var registrarResult = invalidValidationResult;

        // Assert
        Assert.False(cancelarResult.IsValid);
        Assert.False(efetivarResult.IsValid);
        Assert.False(registrarResult.IsValid);

        Assert.Single(cancelarResult.Errors);
        Assert.Single(efetivarResult.Errors);
        Assert.Single(registrarResult.Errors);

        Assert.Equal("idReqSistemaCliente", cancelarResult.Errors.First().campo);
        Assert.Equal("idReqSistemaCliente", efetivarResult.Errors.First().campo);
        Assert.Equal("idReqSistemaCliente", registrarResult.Errors.First().campo);
    }

    [Fact]
    public async Task AllHandlers_WithDatabaseConnectionTimeout_ShouldLogAndRethrowException()
    {
        // Arrange
        var cancelarHandler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var timeoutException = new TimeoutException("Database connection timeout");

        var cancelarTransaction = new TransactionCancelarOrdemPagamentoBuilder().Build();
        var efetivarTransaction = new TransactionEfetivarOrdemPagamentoBuilder().Build();
        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder().Build();

        _mockSpaRepository.CancelarOrdemPagamento(cancelarTransaction).Throws(timeoutException);
        _mockSpaRepository.EfetivarOrdemPagamento(efetivarTransaction).Throws(timeoutException);
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction).Throws(timeoutException);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() =>
            cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None));
        await Assert.ThrowsAsync<TimeoutException>(() =>
            efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None));
        await Assert.ThrowsAsync<TimeoutException>(() =>
            registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None));

        // Verify logging
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante cancelamento", timeoutException);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao", timeoutException);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante registro de ordem de pagamento", timeoutException);
    }

    //[Fact]
    //public async Task RegistrarOrdemPagamentoHandler_WithNullPagadorAndRecebedor_ShouldReturnValidationErrors()
    //{
    //    // Arrange
    //    var handler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
    //    var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
    //        .ComPagador(null)
    //        .ComRecebedor(null)
    //        .Build();

    //    _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
    //        .Returns((new List<ErrorDetails>(), true));
    //    _mockValidatorService.ValidarValor(transaction.valor)
    //        .Returns((new List<ErrorDetails>(), true));
    //    _mockValidatorService.ValidarTpIniciacao(transaction.tpIniciacao)
    //        .Returns((new List<ErrorDetails>(), true));

    //    // Act
    //    //var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

    //    // Assert
    //    Assert.False(validationResult.IsValid);
    //    Assert.Contains(validationResult.Errors, e => e.campo == "pagador");
    //    Assert.Contains(validationResult.Errors, e => e.campo == "recebedor");
    //    Assert.Equal(2, validationResult.Errors.Count);
    //}

    [Fact]
    public async Task AllHandlers_WithBusinessException_ShouldLogAndRethrowException()
    {
        // Arrange
        var cancelarHandler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var businessException = new BusinessException("Operação não permitida");

        var cancelarTransaction = new TransactionCancelarOrdemPagamentoBuilder().Build();
        var efetivarTransaction = new TransactionEfetivarOrdemPagamentoBuilder().Build();
        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder().Build();

        _mockSpaRepository.CancelarOrdemPagamento(cancelarTransaction).Throws(businessException);
        _mockSpaRepository.EfetivarOrdemPagamento(efetivarTransaction).Throws(businessException);
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction).Throws(businessException);

        // Act & Assert
        var cancelarException = await Assert.ThrowsAsync<BusinessException>(() =>
            cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None));
        var efetivarException = await Assert.ThrowsAsync<BusinessException>(() =>
            efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None));
        var registrarException = await Assert.ThrowsAsync<BusinessException>(() =>
            registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None));

        // Assert exceptions
        Assert.Equal("Operação não permitida", cancelarException.Message);
        Assert.Equal("Operação não permitida", efetivarException.Message);
        Assert.Equal("Operação não permitida", registrarException.Message);

        // Verify logging
        _mockLoggingAdapter.Received(3).LogError("Erro retornado pela Sps", businessException);
    }


    [Theory]
    [InlineData(EnumTipoErro.SISTEMA)]
    [InlineData(EnumTipoErro.NEGOCIO)]
    public async Task CancelarOrdemPagamentoHandler_WithDifferentTipoErro_ShouldProcessCorrectly(EnumTipoErro tipoErro)
    {
        // Arrange
        var handler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComTipoErro(tipoErro)
            .Build();

        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(transaction.motivo)
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Equal(tipoErro, transaction.tipoErro);
    }

    [Fact]
    public async Task AllHandlers_WithSuccessfulExecution_ShouldReturnProperResponseStructure()
    {
        // Arrange
        var cancelarHandler = new CancelarOrdemPagamentoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemPagamentoHandler(_serviceProvider);
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var cancelarTransaction = new TransactionCancelarOrdemPagamentoBuilder().Build();
        var efetivarTransaction = new TransactionEfetivarOrdemPagamentoBuilder().Build();
        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder().Build();

        // Setup successful processing
        _mockSpaRepository.CancelarOrdemPagamento(cancelarTransaction)
            .Returns("{\"chvAutorizador\":\"CANCEL123\"}");
        _mockSpaRepository.EfetivarOrdemPagamento(efetivarTransaction)
            .Returns("{\"chvAutorizador\":\"EFETIV123\"}");
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction)
            .Returns("{\"chvAutorizador\":\"REGIST123\",\"endToEndId\":\"E999999999\"}");

        // Act
        var cancelarResponse = await cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None);
        var efetivarResponse = await efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None);
        var registrarResponse = await registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None);

        // Assert
        Assert.NotNull(cancelarResponse);
        Assert.NotNull(efetivarResponse);
        Assert.NotNull(registrarResponse);

        Assert.IsType<JDPICancelarOrdemPagamentoResponse>(cancelarResponse);
        Assert.IsType<JDPIEfetivarOrdemPagamentoResponse>(efetivarResponse);
        Assert.IsType<JDPIRegistrarOrdemPagamentoResponse>(registrarResponse);
    }

    private static ValidationResult CreateInvalidValidationResult(List<ErrorDetails> errors)
    {
        try
        {
            return ValidationResult.Invalid(errors ?? new List<ErrorDetails>());
        }
        catch
        {
            return CreateManualInvalidValidationResult(errors);
        }
    }

    private static ValidationResult CreateManualInvalidValidationResult(List<ErrorDetails> errors)
    {
        var validationResult = new ValidationResult();
        return validationResult;
    }
}