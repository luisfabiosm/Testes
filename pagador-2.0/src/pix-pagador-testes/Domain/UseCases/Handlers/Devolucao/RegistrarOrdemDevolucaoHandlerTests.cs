//using Domain.Core.Exceptions;
//using Domain.Core.Models.Response;
//using Domain.Core.Ports.Domain;
//using Domain.Core.Ports.Outbound;
//using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
//using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using pix_pagador_testes.TestUtilities.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;


//namespace pix_pagador_testes.Domain.UseCases.Handlers.Devolucao;


//public class RegistrarOrdemDevolucaoHandlerTests
//{
//    private readonly Mock<IServiceProvider> _mockServiceProvider;
//    private readonly Mock<IValidatorService> _mockValidatorService;
//    private readonly Mock<ISPARepository> _mockSpaRepository;
//    private readonly Mock<ILogger<RegistrarOrdemDevolucaoHandler>> _mockLogger;
//    private readonly RegistrarOrdemDevolucaoHandler _handler;
//    private readonly Mock<ILoggingAdapter> _mockLoggingAdapter;


//    public RegistrarOrdemDevolucaoHandlerTests()
//    {
//        _mockServiceProvider = new Mock<IServiceProvider>();
//        _mockValidatorService = new Mock<IValidatorService>();
//        _mockSpaRepository = new Mock<ISPARepository>();
//        _mockLogger = new Mock<ILogger<RegistrarOrdemDevolucaoHandler>>();
//        _mockLoggingAdapter = new Mock<ILoggingAdapter>();


//        SetupServiceProvider();
//        _handler = new RegistrarOrdemDevolucaoHandler(_mockServiceProvider.Object);
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
//    public async Task ExecuteSpecificValidations_ComCodigoDevolucaoInvalido_DeveRetornarErro()
//    {
//        // Arrange
//        var transaction = CreateValidTransaction();
//        var errors = new List<ErrorDetails> { new("codigoDevolucao", "Código devolução inválido") };

//        _mockValidatorService.Setup(v => v.ValidarCodigoDevolucao(It.IsAny<string>()))
//            .Returns((errors, false));
//        SetupValidationsSuccess(skipCodigoDevolucao: true);

//        // Act
//        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsValid);
//        Assert.Contains(result.Errors, e => e.campo == "codigoDevolucao");
//    }

//    [Fact]
//    public async Task ExecuteTransactionProcessing_ComSucesso_DeveRetornarResponse()
//    {
//        // Arrange
//        var transaction = TransactionBuilder.CreateRegistrarOrdemDevolucao().Build();
//        var expectedResult = new JDPIRegistrarOrdemDevolucaoResponse
//        {
//            chvAutorizador = "chave123",
//            CorrelationId = transaction.CorrelationId
//        };
//        var _repoResul = JsonSerializer.Serialize(expectedResult);

//        _mockSpaRepository.Setup(r => r.RegistrarOrdemDevolucao(transaction))
//            .ReturnsAsync(_repoResul);

//        // Act
//        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

//        // Assert
//        Assert.NotNull(_repoResul);
//        Assert.IsType<JDPIRegistrarOrdemDevolucaoResponse>(expectedResult);
//    }

//    [Fact]
//    public async Task ExecuteTransactionProcessing_ComExcecaoGenerica_DeveLancarExcecao()
//    {
//        // Arrange
//        var transaction = CreateValidTransaction();
//        var exception = new Exception("Erro genérico de banco");

//        _mockSpaRepository.Setup(r => r.RegistrarOrdemDevolucao(transaction))
//            .ThrowsAsync(exception);

//        // Act & Assert
//        await Assert.ThrowsAsync<Exception>(() =>
//            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));
//    }

//    private TransactionRegistrarOrdemDevolucao CreateValidTransaction()
//    {
//        return new TransactionRegistrarOrdemDevolucao
//        {
//            idReqSistemaCliente = "cliente123",
//            CorrelationId = "corr123",
//            endToEndIdOriginal = "E1234567890123456789012345678901",
//            codigoDevolucao = "MD06",
//            valorDevolucao = 50.0
//        };
//    }

//    private void SetupValidationsSuccess(bool skipCodigoDevolucao = false)
//    {
//        _mockValidatorService.Setup(v => v.ValidarIdReqSistemaCliente(It.IsAny<string>()))
//            .Returns((new List<ErrorDetails>(), true));
//        _mockValidatorService.Setup(v => v.ValidarEndToEndIdOriginal(It.IsAny<string>()))
//            .Returns((new List<ErrorDetails>(), true));

//        if (!skipCodigoDevolucao)
//        {
//            _mockValidatorService.Setup(v => v.ValidarCodigoDevolucao(It.IsAny<string>()))
//                .Returns((new List<ErrorDetails>(), true));
//        }

//        _mockValidatorService.Setup(v => v.ValidarValor(It.IsAny<double>()))
//            .Returns((new List<ErrorDetails>(), true));
//    }
//}
