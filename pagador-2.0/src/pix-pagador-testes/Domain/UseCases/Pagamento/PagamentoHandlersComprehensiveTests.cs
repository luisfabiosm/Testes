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

/// <summary>
/// Testes abrangentes que cobrem cenários complexos e casos de uso reais
/// dos handlers de Pagamento usando NSubstitute
/// </summary>
public class PagamentoHandlersComprehensiveTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;

    public PagamentoHandlersComprehensiveTests()
    {
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _serviceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _serviceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);
    }

    #region Cenários de Uso Real

    [Fact]
    public async Task FluxoCompleto_RegistrarEfetivarPagamento_ShouldExecuteSuccessfully()
    {
        // Arrange - Simula um fluxo completo de registro e efetivação
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
        var efetivarHandler = new EfetivarOrdemPagamentoHandler(_serviceProvider);

        var correlationId = Guid.NewGuid().ToString();
        var endToEndId = "E12345678901234567890123456789012";

        // Transação de registro
        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ_FLOW_001")
            .ComCorrelationId(correlationId)
            .ComValor(1500.00)
            .ComEndToEndId(endToEndId)
            .Build();

        // Transação de efetivação
        var efetivarTransaction = new TransactionEfetivarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ_FLOW_001")
            .ComCorrelationId(correlationId)
            .ComEndToEndId(endToEndId)
            .Build();

        // Setup validações para registro
        SetupAllValidationsSuccess();

        // Setup repositório
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction)
            .Returns($"{{\"chvAutorizador\":\"AUTH123\",\"endToEndId\":\"{endToEndId}\"}}");
        _mockSpaRepository.EfetivarOrdemPagamento(efetivarTransaction)
            .Returns("{\"chvAutorizador\":\"AUTH123\",\"status\":\"EFETIVADO\"}");

        // Act - Executar fluxo completo
        var registroValidation = await registrarHandler.ExecuteSpecificValidations(registrarTransaction, CancellationToken.None);
        var registroResult = await registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None);

        var efetivacoValidation = await efetivarHandler.ExecuteSpecificValidations(efetivarTransaction, CancellationToken.None);
        var efetivacoResult = await efetivarHandler.ExecuteTransactionProcessing(efetivarTransaction, CancellationToken.None);

        // Assert
        Assert.True(registroValidation.IsValid);
        Assert.NotNull(registroResult);
        Assert.True(efetivacoValidation.IsValid);
        Assert.NotNull(efetivacoResult);

        // Verify interactions
        _mockSpaRepository.Received(1).RegistrarOrdemPagamento(registrarTransaction);
        _mockSpaRepository.Received(1).EfetivarOrdemPagamento(efetivarTransaction);
    }

    [Fact]
    public async Task FluxoCompleto_RegistrarCancelarPagamento_ShouldExecuteSuccessfully()
    {
        // Arrange - Simula registro seguido de cancelamento
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
        var cancelarHandler = new CancelarOrdemPagamentoHandler(_serviceProvider);

        var correlationId = Guid.NewGuid().ToString();
        var agendamentoId = "AGENDA_123";

        var registrarTransaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ_CANCEL_001")
            .ComCorrelationId(correlationId)
            .ComAgendamentoID(agendamentoId)
            .Build();

        var cancelarTransaction = new TransactionCancelarOrdemPagamentoBuilder()
            .ComIdReqSistemaCliente("REQ_CANCEL_001")
            .ComCorrelationId(correlationId)
            .ComAgendamentoID(agendamentoId)
            .ComMotivo("Cliente solicitou cancelamento")
            .Build();

        // Setup validações
        SetupAllValidationsSuccess();

        // Setup repositório
        _mockSpaRepository.RegistrarOrdemPagamento(registrarTransaction)
            .Returns("{\"chvAutorizador\":\"AUTH456\",\"status\":\"AGENDADO\"}");
        _mockSpaRepository.CancelarOrdemPagamento(cancelarTransaction)
            .Returns("{\"chvAutorizador\":\"AUTH456\",\"status\":\"CANCELADO\"}");

        // Act
        var registroResult = await registrarHandler.ExecuteTransactionProcessing(registrarTransaction, CancellationToken.None);
        var cancelamentoResult = await cancelarHandler.ExecuteTransactionProcessing(cancelarTransaction, CancellationToken.None);

        // Assert
        Assert.NotNull(registroResult);
        Assert.NotNull(cancelamentoResult);

        _mockSpaRepository.Received(1).RegistrarOrdemPagamento(registrarTransaction);
        _mockSpaRepository.Received(1).CancelarOrdemPagamento(cancelarTransaction);
    }

    #endregion

    #region Testes de Robustez



    [Fact]
    public async Task AllHandlers_WithVeryLargeTransactionData_ShouldHandleEfficiently()
    {
        // Arrange - Testa com dados grandes
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var largeString = new string('A', 10000); // String muito grande
        var largePagador = PagamentoTestDataFactory.CreateValidPagadorPessoaFisica();
        largePagador.nome = largeString;

        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(largePagador)
            .ComInfEntreClientes(largeString)
            .Build();

        SetupAllValidationsSuccess();
        _mockSpaRepository.RegistrarOrdemPagamento(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH789\"}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = await registrarHandler.ExecuteTransactionProcessing(transaction, CancellationToken.None);
        var endTime = DateTime.UtcNow;

        // Assert - Deve processar em tempo razoável mesmo com dados grandes
        Assert.NotNull(result);
        Assert.True((endTime - startTime).TotalSeconds < 10, "Processing took too long for large data");
    }

    #endregion

    #region Testes de Concorrência

    [Fact]
    public async Task AllHandlers_WithHighConcurrentLoad_ShouldMaintainPerformance()
    {
        // Arrange
        var registrarHandler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
        var concurrentOperations = 50;
        var tasks = new List<Task<bool>>();

        SetupAllValidationsSuccess();

        // Setup repositório para responder rapidamente
        _mockSpaRepository.RegistrarOrdemPagamento(Arg.Any<TransactionRegistrarOrdemPagamento>())
            .Returns(callInfo =>
            {
                var transaction = callInfo.Arg<TransactionRegistrarOrdemPagamento>();
                return $"{{\"chvAutorizador\":\"AUTH_{transaction.CorrelationId}\",\"endToEndId\":\"{transaction.endToEndId}\"}}";
            });

        // Act - Criar múltiplas operações concorrentes
        for (int i = 0; i < concurrentOperations; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
                        .ComIdReqSistemaCliente($"REQ_CONCURRENT_{index}")
                        .ComCorrelationId(Guid.NewGuid().ToString())
                        .Build();

                    var validationResult = await registrarHandler.ExecuteSpecificValidations(transaction, CancellationToken.None);
                    var processingResult = await registrarHandler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

                    return validationResult.IsValid && processingResult != null;
                }
                catch
                {
                    return false;
                }
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Todas as operações devem ter sucesso
        Assert.True(results.All(r => r), "Some concurrent operations failed");

        // Verifica que todas as chamadas foram feitas
        _mockSpaRepository.Received(concurrentOperations).RegistrarOrdemPagamento(Arg.Any<TransactionRegistrarOrdemPagamento>());
    }

    #endregion

    #region Testes de Compatibilidade

    [Theory]
    [InlineData(EnumTipoPessoa.PESSOA_FISICA, EnumTipoConta.CORRENTE)]
    [InlineData(EnumTipoPessoa.PESSOA_FISICA, EnumTipoConta.POUPANCA)]
    [InlineData(EnumTipoPessoa.PESSOA_JURIDICA, EnumTipoConta.CORRENTE)]
    [InlineData(EnumTipoPessoa.PESSOA_JURIDICA, EnumTipoConta.POUPANCA)]
    public async Task RegistrarOrdemPagamento_WithAllValidCombinations_ShouldProcessCorrectly(
        EnumTipoPessoa tipoPessoa, EnumTipoConta tipoConta)
    {
        // Arrange
        var handler = new RegistrarOrdemPagamentoHandler(_serviceProvider);

        var pagador = PagamentoTestDataFactory.CreateValidPagadorPessoaFisica();
        pagador.tpPessoa = tipoPessoa;
        pagador.tpConta = tipoConta;

        if (tipoPessoa == EnumTipoPessoa.PESSOA_JURIDICA)
        {
            pagador.cpfCnpj = 12345678000195; // CNPJ
            pagador.nome = "Empresa Teste Ltda";
        }

        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(pagador)
            .Build();

        SetupAllValidationsSuccess();
        _mockSpaRepository.RegistrarOrdemPagamento(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH999\"}");

        // Act
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);

        _mockValidatorService.Received(1).ValidarPagador(
            Arg.Is<JDPIDadosConta>(p => p.tpPessoa == tipoPessoa && p.tpConta == tipoConta));
    }

    [Theory]
    [InlineData(EnumTpIniciacao.CHAVE, "user@example.com")]
    [InlineData(EnumTpIniciacao.MANUAL, "+5511999887766")]
    [InlineData(EnumTpIniciacao.QR_CODE_DINAMICO, "12345678901")]
    public async Task RegistrarOrdemPagamento_WithDifferentTpIniciacaoAndChave_ShouldProcessCorrectly(
        EnumTpIniciacao tpIniciacao, string chave)
    {
        // Arrange
        var handler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComTpIniciacao(tpIniciacao)
            .ComChave(chave)
            .Build();

        SetupAllValidationsSuccess();
        _mockSpaRepository.RegistrarOrdemPagamento(transaction)
            .Returns($"{{\"chvAutorizador\":\"AUTH_{tpIniciacao}\",\"chave\":\"{chave}\"}}");

        // Act
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);
        Assert.Equal(tpIniciacao, transaction.tpIniciacao);
        Assert.Equal(chave, transaction.chave);
    }

    #endregion

    #region Testes de Integração com Validações Complexas

    [Fact]
    public async Task RegistrarOrdemPagamento_WithOptionalFieldsValidation_ShouldHandleCorrectly()
    {
        // Arrange - Testa validação de campos opcionais
        var handler = new RegistrarOrdemPagamentoHandler(_serviceProvider);
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPrioridadePagamento(EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA)
            .ComFinalidade(EnumTipoFinalidade.COMPRA_OU_TRANSFERENCIA)
            .Build();

        // Setup validações incluindo campos opcionais
        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(Arg.Any<JDPIDadosConta>()).Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarRecebedor(Arg.Any<JDPIDadosConta>()).Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(Arg.Any<double>()).Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarTpIniciacao(Arg.Any<EnumTpIniciacao>()).Returns((new List<ErrorDetails>(), true));

        // Validações específicas para campos opcionais
        _mockValidatorService.ValidarPrioridadePagamento(EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA)
            .Returns((new List<ErrorDetails>(), true));

        _mockSpaRepository.RegistrarOrdemPagamento(transaction)
            .Returns("{\"chvAutorizador\":\"AUTH_PRIORITY\"}");

        // Act
        var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);
        var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(processingResult);

        // Verifica que a validação de prioridade foi chamada
        _mockValidatorService.Received(1).ValidarPrioridadePagamento(EnumPrioridadePagamento.LIQUIDACAO_PRIORITARIA);
    }

    #endregion

    #region Helper Methods

    private void SetupAllValidationsSuccess()
    {
        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarMotivo(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarEndToEndId(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarRecebedor(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(Arg.Any<double>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarTpIniciacao(Arg.Any<EnumTpIniciacao>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPrioridadePagamento(Arg.Any<EnumPrioridadePagamento?>())
            .Returns((new List<ErrorDetails>(), true));
    }

    #endregion
}