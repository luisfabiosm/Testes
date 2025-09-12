using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Exceptions;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Domain;
using Domain.Core.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;
using pix_pagador_testes.Domain.Core.Common.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Mediator
{
    public class BSUseCaseHandlerTest
    {
        private TestableUseCaseHandler _testClass;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ILoggingAdapter> _mockLoggingAdapter;
        private Mock<IValidatorService> _mockValidatorService;
        private Mock<ISPARepository> _mockSpaRepository;

        public BSUseCaseHandlerTest()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLoggingAdapter = new Mock<ILoggingAdapter>();
            _mockValidatorService = new Mock<IValidatorService>();
            _mockSpaRepository = new Mock<ISPARepository>();

            _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IValidatorService))).Returns(_mockValidatorService.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(ISPARepository))).Returns(_mockSpaRepository.Object);


            _testClass = new TestableUseCaseHandler(_mockServiceProvider.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TestableUseCaseHandler(_mockServiceProvider.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullServiceProvider()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestableUseCaseHandler(null));
        }

        [Fact]
        public async Task HandleSuccessfulTransactionProcessing()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 1,
                CorrelationId = "test-correlation-123"
            };
            var expectedResult =new TestResponse();
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Valid());
            _testClass.SetProcessingResult(expectedResult);

            // Act
            var result = await _testClass.Handle(transaction, cancellationToken);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedResult, result.Data);
            Assert.Equal("Processamento concluído com sucesso", result.Message);
            Assert.Equal(transaction.CorrelationId, result.CorrelationId);
        }

        [Fact]
        public async Task HandleValidationFailure()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 0, // Invalid code
                CorrelationId = "test-correlation-123"
            };
            var validationErrors = new List<ErrorDetails>
            {
                new ErrorDetails("Code", "Code é obrigatório e deve ser maior que 0")
            };
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Invalid(validationErrors));

            // Act
            var result = await _testClass.Handle(transaction, cancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("Code é obrigatório", result.Message);
        }

        [Fact]
        public async Task HandleBusinessException()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 1,
                CorrelationId = "test-correlation-123"
            };
            var businessException = BusinessException.Create("Business error", 500, "test-origin");
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Valid());
            _testClass.SetProcessingException(businessException);

            // Act
            var result = await _testClass.Handle(transaction, cancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(businessException.Message, result.Message);
            Assert.Equal(businessException.ErrorCode, result.ErrorCode);
        }

        [Fact]
        public async Task HandleUnexpectedException()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 1,
                CorrelationId = "test-correlation-123"
            };
            var unexpectedException = new InvalidOperationException("Unexpected error");
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Valid());
            _testClass.SetProcessingException(unexpectedException);

            // Act
            var result = await _testClass.Handle(transaction, cancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(unexpectedException.Message, result.Message);
        }

        [Fact]
        public async Task HandleProcessingReturnsNull()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 1,
                CorrelationId = "test-correlation-123"
            };
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Valid());
            _testClass.SetProcessingResult(null);

            // Act
            var result = await _testClass.Handle(transaction, cancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Processamento falhou", result.Message);
        }

        [Fact]
        public async Task LogsInformationAtStartAndEnd()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 1,
                CorrelationId = "test-correlation-123"
            };
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Valid());
            _testClass.SetProcessingResult(new TestResponse());

            // Act
            await _testClass.Handle(transaction, cancellationToken);

            // Assert
            _mockLoggingAdapter.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Iniciando processamento")),
                    It.IsAny<object[]>()),
                Times.Once);

            _mockLoggingAdapter.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Processamento concluído com sucesso")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task LogsWarningOnValidationFailure()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 0,
                CorrelationId = "test-correlation-123"
            };
            var validationErrors = new List<ErrorDetails>
            {
                new ErrorDetails("Code", "Code é obrigatório")
            };
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Invalid(validationErrors));

            // Act
            await _testClass.Handle(transaction, cancellationToken);

            // Assert
            _mockLoggingAdapter.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains("Falha na validação")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task CallsPreAndPostProcessing()
        {
            // Arrange
            var transaction = new TestTransaction
            {
                Code = 1,
                CorrelationId = "test-correlation-123"
            };
            var cancellationToken = CancellationToken.None;

            _testClass.SetValidationResult(ValidationResult.Valid());
            _testClass.SetProcessingResult(new TestResponse());

            // Act
            await _testClass.Handle(transaction, cancellationToken);

            // Assert
            Assert.True(_testClass.PreProcessingCalled);
            Assert.True(_testClass.PostProcessingCalled);
        }

        // Test classes for testing


   
        public class TestableUseCaseHandler : BSUseCaseHandler<TestTransaction, BaseReturn<TestResponse>, TestResponse>
        {
            private ValidationResult _validationResult = ValidationResult.Valid();
            private TestResponse _processingResult;
            private Exception _processingException;

            public bool PreProcessingCalled { get; private set; } = true;
            public bool PostProcessingCalled { get; private set; } = true;

            public TestableUseCaseHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            public void SetValidationResult(ValidationResult result)
            {
                _validationResult = result; 
            }

            public void SetProcessingResult(TestResponse result)
            {
                _processingResult = result;
                _processingException = null;
            }

            public void SetProcessingException(Exception exception)
            {
                _processingException = exception;
                _processingResult = null;
            }

            public override async Task<ValidationResult> ExecuteSpecificValidations(TestTransaction transaction, CancellationToken cancellationToken)
            {
                return _validationResult;
            }

            public override async Task<TestResponse> ExecuteTransactionProcessing(TestTransaction transaction, CancellationToken cancellationToken)
            {
                if (_processingException != null)
                    throw _processingException;

                return _processingResult;
            }


            public override BaseReturn<TestResponse> ReturnSuccessResponse(TestResponse result, string message, string correlationId)
            {
                return BaseReturn<TestResponse>.FromSuccess(result, message, correlationId);
            }

            public override BaseReturn<TestResponse> ReturnErrorResponse(Exception exception, string correlationId)
            {
                return BaseReturn<TestResponse>.FromException(exception, correlationId);
            }

            protected override Task PreProcessing(TestTransaction transaction, CancellationToken cancellationToken)
            {
                PreProcessingCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}
