using api_crud_template_testes.Fixtures;
using Domain.Core.SharedKernel.Transactions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace api_crud_template_testes.Unit.Factories;

public class TransactionFactoryTests
{
    private readonly TransactionFactory _factory;

    public TransactionFactoryTests()
    {
        _factory = new TransactionFactory();
    }

    [Fact]
    public void CreateUserTransaction_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        // Act
        var transaction = _factory.CreateUserTransaction(context, request, correlationId);

        // Assert
        transaction.Should().NotBeNull();
        transaction.CorrelationId.Should().Be(correlationId);
        transaction.Code.Should().Be(1);
        transaction.NewUser.Should().NotBeNull();
        transaction.NewUser.CPF.Should().Be(request.CPF);
        transaction.NewUser.Nome.Should().Be(request.Nome);
        transaction.NewUser.Email.Should().Be(request.Email);
        transaction.NewUser.Login.Should().Be(request.Login);
        transaction.NewUser.Password.Should().Be(request.Password);
    }

    [Fact]
    public void CreateUserTransaction_InMockEnvironment_ShouldSetDefaultValues()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Mock");
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        // Act
        var transaction = _factory.CreateUserTransaction(context, request, correlationId);

        // Assert
        transaction.canal.Should().Be(1);
        transaction.chaveIdempotencia.Should().Be("teste");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void CreateUserTransaction_WithCanalInClaim_ShouldUseCanalFromClaim()
    {
        // Arrange
        var expectedCanal = 5;
        var context = CreateMockHttpContextWithClaims(
            new Claim("Canal", expectedCanal.ToString())
        );
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Chave-Idempotencia"] = "test-idempotency-key"
        });

        // Act
        var transaction = _factory.CreateUserTransaction(context, request, correlationId);

        // Assert
        transaction.canal.Should().Be(expectedCanal);
        transaction.chaveIdempotencia.Should().Be("test-idempotency-key");
    }

    [Fact]
    public void CreateUserTransaction_WithCanalInHeader_ShouldUseCanalFromHeader()
    {
        // Arrange
        var expectedCanal = 3;
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Canal"] = expectedCanal.ToString(),
            ["Chave-Idempotencia"] = "test-idempotency-key"
        });

        // Act
        var transaction = _factory.CreateUserTransaction(context, request, correlationId);

        // Assert
        transaction.canal.Should().Be(expectedCanal);
    }

    [Fact]
    public void CreateUserTransaction_WithoutCanal_ShouldThrowArgumentException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Chave-Idempotencia"] = "test-key"
        });

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _factory.CreateUserTransaction(context, request, correlationId));

        exception.Message.Should().Contain("Canal não encontrado");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void CreateUserTransaction_WithInvalidCanal_ShouldThrowFormatException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Canal"] = "invalid-number",
            ["Chave-Idempotencia"] = "test-key"
        });

        // Act & Assert
        var exception = Assert.Throws<FormatException>(
            () => _factory.CreateUserTransaction(context, request, correlationId));

        exception.Message.Should().Contain("Canal deve ser um número válido");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void CreateUserTransaction_WithoutChaveIdempotencia_ShouldThrowArgumentException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Canal"] = "1"
        });

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _factory.CreateUserTransaction(context, request, correlationId));

        exception.Message.Should().Contain("Chave-Idempotencia");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void CreateUserTransaction_WithEmptyChaveIdempotencia_ShouldThrowArgumentException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Canal"] = "1",
            ["Chave-Idempotencia"] = ""
        });

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _factory.CreateUserTransaction(context, request, correlationId));

        exception.Message.Should().Contain("Chave-Idempotencia");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("999")]
    [InlineData("32767")] // Max short value
    public void CreateUserTransaction_WithValidCanalValues_ShouldSetCanal(string canalString)
    {
        // Arrange
        var expectedCanal = short.Parse(canalString);
        var context = CreateMockHttpContext();
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Canal"] = canalString,
            ["Chave-Idempotencia"] = "test-key"
        });

        // Act
        var transaction = _factory.CreateUserTransaction(context, request, correlationId);

        // Assert
        transaction.canal.Should().Be(expectedCanal);
    }

    [Fact]
    public void CreateUserTransaction_ShouldGenerateUserWithUniqueId()
    {
        // Arrange
        var context = CreateMockHttpContext();
        var request1 = TestFixtures.Users.ValidCreateUserRequest;
        var request2 = TestFixtures.Users.ValidCreateUserRequest;
        var correlationId = TestFixtures.CorrelationIds.Valid;

        context.Request.Headers.Returns(new HeaderDictionary
        {
            ["Canal"] = "1",
            ["Chave-Idempotencia"] = "test-key"
        });

        // Act
        var transaction1 = _factory.CreateUserTransaction(context, request1, correlationId);
        var transaction2 = _factory.CreateUserTransaction(context, request2, correlationId);

        // Assert
        transaction1.NewUser.Id.Should().NotBe(transaction2.NewUser.Id);
    }

    private static HttpContext CreateMockHttpContext()
    {
        var context = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var user = Substitute.For<ClaimsPrincipal>();
        var headers = Substitute.For<IHeaderDictionary>();

        context.Request.Returns(request);
        context.User.Returns(user);
        request.Headers.Returns(headers);

        return context;
    }

    private static HttpContext CreateMockHttpContextWithClaims(params Claim[] claims)
    {
        var context = CreateMockHttpContext();
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        context.User.Returns(user);

        return context;
    }
}