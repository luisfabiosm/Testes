using api_crud_template_testes.Fixtures;
using Domain.Core.Interfaces.Outbound;
using Domain.Core.SharedKernel.ResultPattern;
using Domain.Core.SharedKernel.Validation;
using Domain.UseCases.CreateUser;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace api_crud_template_testes.Unit.Processors;

public class CreateUserProcessorTests 
{
    private readonly IUserRepository _userRepository;
    private readonly CreateUserRequestValidator _validator;
    private readonly ILogger<CreateUserProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CreateUserProcessor _processor;
    private readonly ServiceCollection _services;

    public CreateUserProcessorTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _validator = Substitute.For<CreateUserRequestValidator>();
        _logger = Substitute.For<ILogger<CreateUserProcessor>>();

        _services = new ServiceCollection();
        _services.AddSingleton(_userRepository);
        _services.AddSingleton(_validator);
        _services.AddSingleton(_logger);

        _serviceProvider = _services.BuildServiceProvider();
        _processor = new CreateUserProcessor(_serviceProvider);
    }

    [Fact]
    public async Task ProcessAsync_WithValidTransaction_ShouldReturnSuccess()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedUserId = transaction.NewUser.Id;

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        _userRepository.CreateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedUserId));

        // Act
        var result = await _processor.ProcessAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(expectedUserId);
        result.Value.Email.Should().Be(transaction.NewUser.Email);
        result.Value.Nome.Should().Be(transaction.NewUser.Nome);
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidData_ShouldReturnFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var validationErrors = new List<ValidationError>
        {
            new("Email", "Email é obrigatório", ""),
            new("Nome", "Nome é obrigatório", "")
        };

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Failure(validationErrors));

        // Act
        var result = await _processor.ProcessAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Dados inválidos");
        result.Error.Should().Contain("Email é obrigatório");
        result.Error.Should().Contain("Nome é obrigatório");
    }

    [Fact]
    public async Task ProcessAsync_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Success(true)); // Email já existe

        // Act
        var result = await _processor.ProcessAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email já está em uso");

        // Verify that CreateAsync was never called
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<TransactionCreateUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenRepositoryFailsToCheckEmail_ShouldReturnFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedError = "Database connection failed";

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(expectedError));

        // Act
        var result = await _processor.ProcessAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro ao verificar email existente");
        result.Error.Should().Contain(expectedError);
    }

    [Fact]
    public async Task ProcessAsync_WhenRepositoryFailsToCreate_ShouldReturnFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedError = "Failed to insert user";

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        _userRepository.CreateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(expectedError));

        // Act
        var result = await _processor.ProcessAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro ao salvar usuário");
        result.Error.Should().Contain(expectedError);
    }

    [Fact]
    public async Task ProcessAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var cancellationToken = new CancellationToken();

        _validator.ValidateAsync(transaction, cancellationToken)
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, cancellationToken)
            .Returns(Result.Success(false));

        _userRepository.CreateAsync(transaction, cancellationToken)
            .Returns(Result.Success(transaction.NewUser.Id));

        // Act
        var result = await _processor.ProcessAsync(transaction, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify all methods were called with the correct cancellation token
        await _validator.Received(1).ValidateAsync(transaction, cancellationToken);
        await _userRepository.Received(1).ExistsByEmailAsync(transaction.NewUser.Email, cancellationToken);
        await _userRepository.Received(1).CreateAsync(transaction, cancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_WhenExceptionThrown_ShouldReturnFailureWithExceptionMessage()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedException = new InvalidOperationException("Unexpected error occurred");

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, Arg.Any<CancellationToken>())
            .Throws(expectedException);

        // Act
        var result = await _processor.ProcessAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro interno no processador");
        result.Error.Should().Contain(expectedException.Message);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallValidationFirst()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var validationError = new ValidationError("Test", "Validation failed", null);

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Failure(validationError));

        // Act
        await _processor.ProcessAsync(transaction);

        // Assert
        await _validator.Received(1).ValidateAsync(transaction, Arg.Any<CancellationToken>());
        // Repository methods should not be called if validation fails
        await _userRepository.DidNotReceive().ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<TransactionCreateUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldLogInformationOnSuccess()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Success());

        _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        _userRepository.CreateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(transaction.NewUser.Id));

        // Act
        await _processor.ProcessAsync(transaction);

        // Assert
        _logger.Received().LogInformation(
            Arg.Is<string>(s => s.Contains("processado com sucesso")),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldLogWarningOnValidationFailure()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        var validationError = new ValidationError("Email", "Email é obrigatório", "");

        _validator.ValidateAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(ValidationResult.Failure(validationError));

        // Act
        await _processor.ProcessAsync(transaction);

        // Assert
        _logger.Received().LogWarning(
            Arg.Is<string>(s => s.Contains("Dados inválidos")),
            Arg.Any<object[]>());
    }

  
}