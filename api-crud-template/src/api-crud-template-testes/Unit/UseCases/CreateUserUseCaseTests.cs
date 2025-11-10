using api_crud_template_testes.Fixtures;
using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.ResultPattern;
using Domain.UseCases.CreateUser;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace api_crud_template_testes.Unit.UseCases;

public class CreateUserUseCaseTests 
{
    private readonly CreateUserProcessor _processor;
    private readonly ILogger<CreateUserUseCase> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CreateUserUseCase _useCase;
    private readonly ServiceCollection _services;

    public CreateUserUseCaseTests()
    {
        _processor = Substitute.For<CreateUserProcessor>();
        _logger = Substitute.For<ILogger<CreateUserUseCase>>();

        _services = new ServiceCollection();
        _services.AddSingleton(_processor);
        _services.AddSingleton(_logger);

        _serviceProvider = _services.BuildServiceProvider();
        _useCase = new CreateUserUseCase(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTransaction_ShouldReturnSuccess()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        // Act
        var result = await _useCase.ExecuteAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(expectedResponse.Id);
        result.Value.Email.Should().Be(expectedResponse.Email);
        result.Value.Nome.Should().Be(expectedResponse.Nome);
        result.Value.Status.Should().Be(expectedResponse.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProcessorFails_ShouldReturnFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedError = "Processing failed";

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CreateUserResponse>(expectedError));

        // Act
        var result = await _useCase.ExecuteAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToProcessor()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var cancellationToken = new CancellationToken();
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _processor.ProcessAsync(transaction, cancellationToken)
            .Returns(Result.Success(expectedResponse));

        // Act
        var result = await _useCase.ExecuteAsync(transaction, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _processor.Received(1).ProcessAsync(transaction, cancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionThrown_ShouldReturnFailureWithExceptionMessage()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedException = new InvalidOperationException("Unexpected error in use case");

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Throws(expectedException);

        // Act
        var result = await _useCase.ExecuteAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro interno");
        result.Error.Should().Contain(expectedException.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationOnStart()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        // Act
        await _useCase.ExecuteAsync(transaction);

        // Assert
        _logger.Received().LogInformation(
            Arg.Is<string>(s => s.Contains("Iniciando criação de usuário")),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationOnSuccess()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        // Act
        await _useCase.ExecuteAsync(transaction);

        // Assert
        _logger.Received().LogInformation(
            Arg.Is<string>(s => s.Contains("Usuário criado com sucesso")),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogWarningOnFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedError = "Processing failed";

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CreateUserResponse>(expectedError));

        // Act
        await _useCase.ExecuteAsync(transaction);

        // Assert
        _logger.Received().LogWarning(
            Arg.Is<string>(s => s.Contains("Falha na criação do usuário")),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogErrorOnException()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedException = new InvalidOperationException("Test exception");

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Throws(expectedException);

        // Act
        await _useCase.ExecuteAsync(transaction);

        // Assert
        _logger.Received().LogError(
            Arg.Is<Exception>(ex => ex == expectedException),
            Arg.Is<string>(s => s.Contains("Erro inesperado ao criar usuário")));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullTransaction_ShouldHandleGracefully()
    {
        // Arrange
        TransactionCreateUser transaction = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NullReferenceException>(
            () => _useCase.ExecuteAsync(transaction));

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenProcessorReturnsNull_ShouldHandleGracefully()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns((Result<CreateUserResponse>)null);

        // Act
        var result = await _useCase.ExecuteAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro interno");
    }

    [Fact]
    public async Task ExecuteAsync_MultipleCallsWithSameTransaction_ShouldProcessEachCall()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _processor.ProcessAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        // Act
        var result1 = await _useCase.ExecuteAsync(transaction);
        var result2 = await _useCase.ExecuteAsync(transaction);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        // Verify processor was called twice
        await _processor.Received(2).ProcessAsync(transaction, Arg.Any<CancellationToken>());
    }

    
}