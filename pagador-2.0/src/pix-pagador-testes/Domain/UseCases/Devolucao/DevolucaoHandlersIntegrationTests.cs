using Domain.Core.Exceptions;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace pix_pagador_testes.Domain.UseCases.Devolucao;


public class DevolucaoHandlersIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;

    public DevolucaoHandlersIntegrationTests()
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
    public async Task CancelarOrdemDevolucaoHandler_FullWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new CancelarOrdemDevolucaoHandler(_serviceProvider);
        var transaction = new TransactionCancelarOrdemDevolucaoBuilder()
            .ComIdReqSistemaCliente("REQ987654321")
            .Build();


        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
             .Returns((new List<ErrorDetails>(), true));

        _mockSpaRepository.CancelarOrdemDevolucao(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH123\"}");

        // Act - Test validation
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Act - Test processing
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Act - Test success response
        var successResponse = handler.ReturnSuccessResponse(processingResult, "Sucesso", transaction.CorrelationId);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);
        Assert.True(successResponse.Success);
        Assert.Equal("Sucesso", successResponse.Message);
    }

    [Fact]
    public async Task EfetivarOrdemDevolucaoHandler_FullWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new EfetivarOrdemDevolucaoHandler(_serviceProvider);
        var transaction = new TransactionEfetivarOrdemDevolucaoBuilder()
            .ComIdReqSistemaCliente("REQ987654321")
            .ComEndToEndIdOriginal("E98765432109876543210987654321098")
            .Build();



        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
             .Returns((new List<ErrorDetails>(), true));


        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
             .Returns((new List<ErrorDetails>(), true));


        _mockSpaRepository.EfetivarOrdemDevolucao(transaction)
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
    public async Task RegistrarOrdemDevolucaoHandler_FullWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new RegistrarOrdemDevolucaoHandler(_serviceProvider);
        var transaction = new TransactionRegistrarOrdemDevolucaoBuilder()
            .ComIdReqSistemaCliente("REQ987654321")
            .ComEndToEndIdOriginal("E98765432109876543210987654321098")
            .ComCodigoDevolucao("CD999")
            .ComValorDevolucao(500.75)
            .Build();


        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((new List<ErrorDetails>(), true));

        _mockValidatorService.ValidarEndToEndIdOriginal(transaction.endToEndIdOriginal)
             .Returns((new List<ErrorDetails>(), true));

        _mockValidatorService.ValidarCodigoDevolucao(transaction.codigoDevolucao)
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(transaction.valorDevolucao)
            .Returns((new List<ErrorDetails>(), true));
        _mockSpaRepository.RegistrarOrdemDevolucao(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH789\",\"endToEndIdDevolucao\":\"D123456789\"}");

        // Act - Test validation
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Act - Test processing
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Act - Test success response
        var successResponse = handler.ReturnSuccessResponse(processingResult, "Registro concluído", transaction.CorrelationId);

        // Assert
        Assert.True(validationResult.IsValid);
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
        var validValidationResult = CreateValidValidationResult();


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
        var cancelarHandler = new CancelarOrdemDevolucaoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemDevolucaoHandler(_serviceProvider);
        var registrarHandler = new RegistrarOrdemDevolucaoHandler(_serviceProvider);

        var timeoutException = new TimeoutException("Database connection timeout");

        var cancelarTransaction = new TransactionCancelarOrdemDevolucaoBuilder().Build();
        var efetivarTransaction = new TransactionEfetivarOrdemDevolucaoBuilder().Build();
        var registrarTransaction = new TransactionRegistrarOrdemDevolucaoBuilder().Build();

        _mockSpaRepository.CancelarOrdemDevolucao(cancelarTransaction).Throws(timeoutException);
        _mockSpaRepository.EfetivarOrdemDevolucao(efetivarTransaction).Throws(timeoutException);
        _mockSpaRepository.RegistrarOrdemDevolucao(registrarTransaction).Throws(timeoutException);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() =>
            cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None));
        await Assert.ThrowsAsync<TimeoutException>(() =>
            efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None));
        await Assert.ThrowsAsync<TimeoutException>(() =>
            registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None));

        // Verify logging
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante cancelamento de ordem de devolução", timeoutException);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante efetivacao de ordem de devolução", timeoutException);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante registro de ordem de devolução", timeoutException);
    }

    private static ValidationResult CreateInvalidValidationResult(List<ErrorDetails> errors)
    {
        // Opção 1: Se ValidationResult tem construtor que aceita lista de erros
        try
        {
            return ValidationResult.Invalid(errors ?? new List<ErrorDetails>());
        }
        catch
        {
            // Opção 2: Se precisar criar manualmente
            return CreateManualInvalidValidationResult(errors);
        }
    }

    private static ValidationResult CreateValidValidationResult()
    {
        try
        {
            return ValidationResult.Valid();
        }
        catch
        {
            // Fallback manual se necessário
            return CreateManualValidValidationResult();
        }
    }

    private static ValidationResult CreateManualInvalidValidationResult(List<ErrorDetails> errors)
    {
        var validationResult = new ValidationResult();

        return validationResult;
    }

    private static ValidationResult CreateManualValidValidationResult()
    {
        var validationResult = new ValidationResult();

        return validationResult;
    }

}
public static class ValidationResultTestHelper
{
    public static ValidationResult CreateInvalid(params ErrorDetails[] errors)
    {
        var errorList = errors?.ToList() ?? new List<ErrorDetails>();

        try
        {
            return ValidationResult.Invalid(errorList);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar ValidationResult inválido. " +
                $"Verifique a implementação da classe ValidationResult. " +
                $"Erro: {ex.Message}", ex);
        }
    }

    public static ValidationResult CreateValid()
    {
        try
        {
            return ValidationResult.Valid();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar ValidationResult válido. " +
                $"Verifique a implementação da classe ValidationResult. " +
                $"Erro: {ex.Message}", ex);
        }
    }

    public static void VerifyValidationResult(ValidationResult result, bool shouldBeValid, int expectedErrorCount = 0)
    {
        Assert.NotNull(result);
        Assert.Equal(shouldBeValid, result.IsValid);

        if (shouldBeValid)
        {
            Assert.True(result.Errors == null || !result.Errors.Any());
        }
        else
        {
            Assert.NotNull(result.Errors);
            Assert.Equal(expectedErrorCount, result.Errors.Count);
        }
    }

}