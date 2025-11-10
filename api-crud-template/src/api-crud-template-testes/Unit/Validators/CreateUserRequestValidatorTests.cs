using api_crud_template_testes.Fixtures;
using Domain.Core.Models.Entities;
using Domain.UseCases.CreateUser;
using FluentAssertions;
using Xunit;

namespace api_crud_template_testes.Unit.Validators;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator;

    public CreateUserRequestValidatorTests()
    {
        _validator = new CreateUserRequestValidator();
    }

    [Fact]
    public void Validate_WithValidTransaction_ShouldReturnSuccess()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyNome_ShouldReturnError()
    {
        // Arrange
        var transaction = TestFixtures.Users.InvalidTransactionEmptyUser;

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Nome" && e.Message.Contains("obrigatório"));
    }


    [Fact]
    public void Validate_WithLongNome_ShouldReturnError()
    {
        // Arrange
        var transaction = TestFixtures.Users.InvalidTransactionUserLomgName;
        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Nome" &&
            e.Message.Contains("no máximo 100 caracteres"));
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldReturnError()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        transaction.NewUser = transaction.NewUser with { Email = string.Empty };

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.Message.Contains("obrigatório"));
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldReturnError()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        transaction.NewUser = transaction.NewUser with { Email = "invalid-email" };

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Email" &&
            e.Message.Contains("formato válido"));
    }

    [Theory]
    [InlineData("test@email.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("123@test.org")]
    public void Validate_WithValidEmails_ShouldReturnSuccess(string email)
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;
        transaction.NewUser = transaction.NewUser with { Email = email };

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidCpfFormat_ShouldReturnError()
    {
        // Arrange
        var transaction = TestFixtures.Users.InvalidTransactionCPFInvalido;

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "CPF" &&
            e.Message.Contains("formato válido"));
    }

    [Theory]
    [InlineData("123.456.789-01")]
    [InlineData("987.654.321-00")]
    [InlineData("111.222.333-44")]
    public void Validate_WithValidCpfFormat_ShouldReturnSuccess(string cpf)
    {
        // Arrange
        TestFixtures.Users.cpf = cpf;
        var transaction = TestFixtures.Users.ValidTransactionUserParameters;
       

        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithValidTransaction_ShouldReturnSuccess()
    {
        // Arrange
        var transaction = TestFixtures.Users.ValidTransaction;

        // Act
        var result = await _validator.ValidateAsync(transaction);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var transaction = TestFixtures.Users.InvalidTransactionMultipleErros;
 
        // Act
        var result = _validator.Validate(transaction);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "CPF");
    }
}