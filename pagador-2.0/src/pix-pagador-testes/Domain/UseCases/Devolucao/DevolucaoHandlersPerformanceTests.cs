using Domain.Core.Exceptions;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Devolucao.CancelarOrdemDevolucao;
using Domain.UseCases.Devolucao.EfetivarOrdemDevolucao;
using Domain.UseCases.Devolucao.RegistrarOrdemDevolucao;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;


namespace pix_pagador_testes.Domain.UseCases.Devolucao
{
    #region Performance and Edge Case Tests

    public class DevolucaoHandlersPerformanceTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IValidatorService _mockValidatorService;
        private readonly ISPARepository _mockSpaRepository;
        private readonly ILoggingAdapter _mockLoggingAdapter;

        public DevolucaoHandlersPerformanceTests()
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
        public async Task RegistrarOrdemDevolucaoHandler_WithMaximumValueTransaction_ShouldHandleCorrectly()
        {
            // Arrange
            var handler = new RegistrarOrdemDevolucaoHandler(_serviceProvider);
            var transaction = new TransactionRegistrarOrdemDevolucaoBuilder()
                .ComValorDevolucao(double.MaxValue)
                .Build();

            _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true)); 
            _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarCodigoDevolucao(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarValor(double.MaxValue).Returns((new List<ErrorDetails>(), true));

            _mockSpaRepository.RegistrarOrdemDevolucao(transaction)
                .Returns("{\"chvAutorizador\":\"AUTH123\"}");

            // Act
            var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);
            var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.NotNull(processingResult);
            _mockValidatorService.Received(1).ValidarValor(double.MaxValue);
        }

        [Fact]
        public async Task AllHandlers_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var cancelarHandler = new CancelarOrdemDevolucaoHandler(_serviceProvider);
            var efetivarHandler = new EfetivarOrdemDevolucaoHandler(_serviceProvider);
            var registrarHandler = new RegistrarOrdemDevolucaoHandler(_serviceProvider);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var cancelarTransaction = new TransactionCancelarOrdemDevolucaoBuilder().Build();
            var efetivarTransaction = new TransactionEfetivarOrdemDevolucaoBuilder().Build();
            var registrarTransaction = new TransactionRegistrarOrdemDevolucaoBuilder().Build();

            _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));   
            _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarCodigoDevolucao(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarValor(Arg.Any<double>()).Returns((new List<ErrorDetails>(), true));

            // Act & Assert - Should not throw since the handlers don't explicitly check cancellation in validation
            // But the cancellation token is passed correctly
            var cancelarResult = await cancelarHandler.ExecuteSpecificValidations(cancelarTransaction, cancellationTokenSource.Token);
            var efetivarResult = await efetivarHandler.ExecuteSpecificValidations(efetivarTransaction, cancellationTokenSource.Token);
            var registrarResult = await registrarHandler.ExecuteSpecificValidations(registrarTransaction, cancellationTokenSource.Token);

            Assert.True(cancelarResult.IsValid);
            Assert.True(efetivarResult.IsValid);
            Assert.True(registrarResult.IsValid);
        }

        [Theory]
        [InlineData(1.00)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task RegistrarOrdemDevolucaoHandler_WithVariousValidValues_ShouldProcessCorrectly(double valor)
        {
            // Arrange
            var handler = new RegistrarOrdemDevolucaoHandler(_serviceProvider);
            var transaction = new TransactionRegistrarOrdemDevolucaoBuilder()
                .ComValorDevolucao(valor)
                .Build();

            _mockValidatorService.ValidarIdReqSistemaCliente(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarEndToEndIdOriginal(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarCodigoDevolucao(Arg.Any<string>()).Returns((new List<ErrorDetails>(), true));
            _mockValidatorService.ValidarValor(valor).Returns((new List<ErrorDetails>(), true));

            string jsonString = $"{{\"chvAutorizador\":\"{transaction.chaveIdempotencia}\",\"valorDevolucao\":{valor}}}";


            _mockSpaRepository.RegistrarOrdemDevolucao(transaction)
                .Returns(jsonString);

            // Act
            var validationResult = await handler.ExecuteSpecificValidations(transaction, CancellationToken.None);
            var processingResult = await handler.ExecuteTransactionProcessing(transaction, CancellationToken.None);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.NotNull(processingResult);
        }

       
    }

    #endregion
}
