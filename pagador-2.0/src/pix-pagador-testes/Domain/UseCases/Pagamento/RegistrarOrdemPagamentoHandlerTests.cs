#region RegistrarOrdemPagamentoHandlerTests

using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.JDPI;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using pix_pagador_testes.TestUtilities.Builders;

namespace pix_pagador_testes.Domain.UseCases.Handlers.Pagamento;

public class RegistrarOrdemPagamentoHandlerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IValidatorService _mockValidatorService;
    private readonly ISPARepository _mockSpaRepository;
    private readonly ILoggingAdapter _mockLoggingAdapter;
    private readonly RegistrarOrdemPagamentoHandler _handler;

    public RegistrarOrdemPagamentoHandlerTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockValidatorService = Substitute.For<IValidatorService>();
        _mockSpaRepository = Substitute.For<ISPARepository>();
        _mockLoggingAdapter = Substitute.For<ILoggingAdapter>();

        // Setup ServiceProvider
        _mockServiceProvider.GetService<IValidatorService>().Returns(_mockValidatorService);
        _mockServiceProvider.GetService<ISPARepository>().Returns(_mockSpaRepository);
        _mockServiceProvider.GetService(typeof(ILoggingAdapter)).Returns(_mockLoggingAdapter);

        _handler = new RegistrarOrdemPagamentoHandler(_mockServiceProvider);
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        // Act
        var instance = new RegistrarOrdemPagamentoHandler(_mockServiceProvider);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithValidTransaction_ShouldReturnValid()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(CreateValidPagador())
            .ComRecebedor(CreateValidRecebedor())
            .ComValor(100.50)
            .Build();

        // Configurar mocks para retornar tuplas (não ValidationResult)
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
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(null)
            .ComRecebedor(null)
            .ComValor(-10)
            //.ComTpIniciacao((EnumTpIniciacao)999) // Valor inválido
            .Build();

        var clienteErrors = new List<ErrorDetails>
        {
            new ErrorDetails("idReqSistemaCliente", "Id do sistema cliente é obrigatório")
        };
        var valorErrors = new List<ErrorDetails>
        {
            new ErrorDetails("valor", "Valor deve ser maior que zero")
        };
        var iniciacaoErrors = new List<ErrorDetails>
        {
            new ErrorDetails("tpIniciacao", "Tipo de iniciação inválido")
        };

        // Configurar mocks para retornar tuplas com erros
        _mockValidatorService.ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente)
            .Returns((clienteErrors, false));
        _mockValidatorService.ValidarValor(transaction.valor)
            .Returns((valorErrors, false));
        _mockValidatorService.ValidarTpIniciacao(transaction.tpIniciacao)
            .Returns((iniciacaoErrors, false));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5); // Pelo menos 5 erros: cliente, pagador null, recebedor null, valor, iniciacao
        Assert.Contains(result.Errors, e => e.campo == "idReqSistemaCliente");
        Assert.Contains(result.Errors, e => e.campo == "pagador");
        Assert.Contains(result.Errors, e => e.campo == "recebedor");
        Assert.Contains(result.Errors, e => e.campo == "valor");
        Assert.Contains(result.Errors, e => e.campo == "tpIniciacao");
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithNullPagador_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(null)
            .Build();

        SetupAllValidationsSuccess();

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "pagador");
        Assert.Contains(result.Errors, e => e.mensagens == "Dados do pagador são obrigatórios");
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithNullRecebedor_ShouldReturnInvalid()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComRecebedor(null)
            .Build();

        SetupAllValidationsSuccess();

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "recebedor");
        Assert.Contains(result.Errors, e => e.mensagens == "Dados do recebedor são obrigatórios");
    }

    [Theory]
    [InlineData("REQ123", 100.50, EnumTpIniciacao.QR_CODE_DINAMICO)]
    [InlineData("REQ456", 250.75, EnumTpIniciacao.MANUAL)]
    [InlineData("REQ789", 50.25, EnumTpIniciacao.QR_CODE_ESTATICO)]
    public async Task ExecuteSpecificValidations_WithVariousValidInputs_ShouldReturnValid(
        string idReqSistemaCliente, double valor, EnumTpIniciacao tpIniciacao)
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComValor(valor)
            //.ComTpIniciacao(tpIniciacao)
            .ComPagador(CreateValidPagador())
            .ComRecebedor(CreateValidRecebedor())
            .Build();

        SetupAllValidationsSuccess();

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithValidTransaction_ShouldReturnSuccessResponse()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(CreateValidPagador())
            .ComRecebedor(CreateValidRecebedor())
            .ComValor(100.50)
            .Build();

        var expectedResult = "{\"chvAutorizador\":\"AUTH123\",\"endToEndId\":\"E123456789\"}";
        _mockSpaRepository.RegistrarOrdemPagamento(transaction).Returns(expectedResult);

        // Act
        var result = await _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JDPIRegistrarOrdemPagamentoResponse>(result);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithBusinessException_ShouldLogAndRethrow()
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .Build();

        var businessException = new BusinessException("Pagamento já registrado");
        _mockSpaRepository.RegistrarOrdemPagamento(transaction).Throws(businessException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Pagamento já registrado", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro retornado pela Sps", businessException);
    }

    [Fact]
    public async Task ExecuteTransactionProcessing_WithGenericException_ShouldLogAndRethrow()
    {

        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .Build();

        var genericException = new Exception("Falha na comunicação com o banco");
        _mockSpaRepository.RegistrarOrdemPagamento(transaction).Throws(genericException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.ExecuteTransactionProcessing(transaction, CancellationToken.None));

        Assert.Equal("Falha na comunicação com o banco", exception.Message);
        _mockLoggingAdapter.Received(1).LogError("Erro de database durante registro de ordem de pagamento", genericException);
    }

    [Fact]
    public void ReturnSuccessResponse_WithValidParameters_ShouldReturnBaseReturnSuccess()
    {
        // Arrange
        var response = new JDPIRegistrarOrdemPagamentoResponse();
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
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(CreateValidPagador())
            .ComRecebedor(CreateValidRecebedor())
            .ComValor(100.50)
            .Build();

        SetupAllValidationsSuccess();

        // Act
        await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        _mockValidatorService.Received(1).ValidarIdReqSistemaCliente(transaction.idReqSistemaCliente);
        _mockValidatorService.Received(1).ValidarPagador(transaction.pagador);
        _mockValidatorService.Received(1).ValidarRecebedor(transaction.recebedor);
        _mockValidatorService.Received(1).ValidarValor(transaction.valor);
        _mockValidatorService.Received(1).ValidarTpIniciacao(transaction.tpIniciacao);
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithInvalidPagador_ShouldReturnInvalid()
    {
        // Arrange
        var invalidPagador = CreateValidPagador();
        invalidPagador.cpfCnpj = 0; // Invalid CPF/CNPJ

        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComPagador(invalidPagador)
            .Build();

        var pagadorErrors = new List<ErrorDetails>
        {
            new ErrorDetails("pagador.cpfCnpj", "CPF/CNPJ do pagador é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(invalidPagador)
            .Returns((pagadorErrors, false));
        _mockValidatorService.ValidarRecebedor(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(Arg.Any<double>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarTpIniciacao(Arg.Any<EnumTpIniciacao>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "pagador.cpfCnpj");
    }

    [Fact]
    public async Task ExecuteSpecificValidations_WithInvalidRecebedor_ShouldReturnInvalid()
    {
        // Arrange
        var invalidRecebedor = CreateValidRecebedor();
        invalidRecebedor.ispb = 0; // Invalid ISPB

        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComRecebedor(invalidRecebedor)
            .Build();

        var recebedorErrors = new List<ErrorDetails>
        {
            new ErrorDetails("recebedor.ispb", "ISPB do recebedor é obrigatório")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarRecebedor(invalidRecebedor)
            .Returns((recebedorErrors, false));
        _mockValidatorService.ValidarValor(Arg.Any<double>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarTpIniciacao(Arg.Any<EnumTpIniciacao>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "recebedor.ispb");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.50)]
    [InlineData(-100)]
    public async Task ExecuteSpecificValidations_WithInvalidValues_ShouldReturnInvalid(double invalidValue)
    {
        // Arrange
        var transaction = new TransactionRegistrarOrdemPagamentoBuilder()
            .ComValor(invalidValue)
            .Build();

        var valorErrors = new List<ErrorDetails>
        {
            new ErrorDetails("valor", "Valor deve ser maior que zero")
        };

        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarRecebedor(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(invalidValue)
            .Returns((valorErrors, false));
        _mockValidatorService.ValidarTpIniciacao(Arg.Any<EnumTpIniciacao>())
            .Returns((new List<ErrorDetails>(), true));

        // Act
        var result = await _handler.ExecuteSpecificValidations(transaction, CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.campo == "valor");
    }

    // Helper methods
    private void SetupAllValidationsSuccess()
    {
        _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarPagador(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarRecebedor(Arg.Any<JDPIDadosConta>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarValor(Arg.Any<double>())
            .Returns((new List<ErrorDetails>(), true));
        _mockValidatorService.ValidarTpIniciacao(Arg.Any<EnumTpIniciacao>())
            .Returns((new List<ErrorDetails>(), true));
    }

    private static JDPIDadosConta CreateValidPagador()
    {
        return new JDPIDadosConta
        {
            ispb = 12345678,
            cpfCnpj = 12345678901,
            nome = "João Silva Santos",
            tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
            tpConta = EnumTipoConta.CORRENTE,
            nrAgencia = "1234",
            nrConta = "567890123"
        };
    }

    private static JDPIDadosConta CreateValidRecebedor()
    {
        return new JDPIDadosConta
        {
            ispb = 12345678,
            cpfCnpj = 12345678000195,
            nome = "Empresa Teste Ltda",
            tpPessoa = EnumTipoPessoa.PESSOA_FISICA,
            tpConta = EnumTipoConta.CORRENTE,
            nrAgencia = "1234",
            nrConta = "567890123"
        };
    }
}

#endregion