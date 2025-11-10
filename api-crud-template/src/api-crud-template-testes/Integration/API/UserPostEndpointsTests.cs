using api_crud_template_testes.Fixtures;
using Domain.Core.Interfaces.Domain;
using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace api_crud_template_testes.Integration.API;

public class UserPostEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ICreateUserUseCase _mockUseCase;
    private readonly ITransactionFactory _mockTransactionFactory;

    public UserPostEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _mockUseCase = Substitute.For<ICreateUserUseCase>();
        _mockTransactionFactory = Substitute.For<ITransactionFactory>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Remove the existing services and add mocks
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICreateUserUseCase));
                if (descriptor != null) services.Remove(descriptor);

                descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITransactionFactory));
                if (descriptor != null) services.Remove(descriptor);

                services.AddScoped(_ => _mockUseCase);
                services.AddScoped(_ => _mockTransactionFactory);
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_CreateUser_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);
        var correlationId = TestFixtures.CorrelationIds.Valid;

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        _client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<CreateUserResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(expectedResponse.Id);
        apiResponse.Data.Email.Should().Be(expectedResponse.Email);

        // Verify correlation ID in response
        response.Headers.Should().ContainKey("X-Correlation-ID");
    }

    [Fact]
    public async Task POST_CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = TestFixtures.Users.InvalidCreateUserRequest;
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedError = "Dados inválidos: Email é obrigatório";

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CreateUserResponse>(expectedError));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("Dados inválidos");
    }

    [Fact]
    public async Task POST_CreateUser_WithoutCorrelationId_ShouldGenerateOne()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Correlation-ID");

        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task POST_CreateUser_WithoutAuthorization_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;

        // Act (não enviando header Authorization)
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_CreateUser_WithInvalidJwtToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-jwt-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_CreateUser_WhenUseCaseThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var transaction = TestFixtures.Users.ValidTransaction;

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Database connection failed"));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task POST_CreateUser_WithLargePayload_ShouldHandleCorrectly()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        request.Nome = new string('A', 10000); // Nome muito longo

        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        // O comportamento pode variar dependendo da validação
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_CreateUser_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        request.Nome = "José da Silva Ñoño";
        request.Email = "josé.ñoño@domínio.com.br";

        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_RootEndpoint_ShouldReturnApiInformation()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("CRUD Template API");
        content.Should().Contain("v1");
    }

    [Fact]
    public async Task GET_HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task POST_CreateUser_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        const int numberOfRequests = 10;
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        // Act
        var tasks = Enumerable.Range(0, numberOfRequests)
            .Select(_ => _client.PostAsJsonAsync("/api/users", request))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Verify that all requests had unique correlation IDs
        var correlationIds = responses
            .SelectMany(r => r.Headers.GetValues("X-Correlation-ID"))
            .ToArray();

        correlationIds.Should().HaveCount(numberOfRequests);
        correlationIds.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/json; charset=utf-8")]
    public async Task POST_CreateUser_WithDifferentContentTypes_ShouldWork(string contentType)
    {
        // Arrange
        var request = TestFixtures.Users.ValidCreateUserRequest;
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedResponse = CreateUserResponse.Create(transaction.NewUser);

        _mockTransactionFactory.CreateUserTransaction(Arg.Any<HttpContext>(), request, Arg.Any<string>())
            .Returns(transaction);

        _mockUseCase.ExecuteAsync(transaction, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedResponse));

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetValidJwtToken());

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, contentType);

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private string GetValidJwtToken()
    {
        // Este é um token JWT válido para testes (expira em 1 ano)
        // Em um ambiente real, você geraria tokens dinamicamente
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJhcGktY3J1ZC10ZW1wbGF0ZSIsImF1ZCI6ImFwaS1jcnVkLXRlbXBsYXRlIiwiaWF0IjoxNzU4MzExMjk3LCJleHAiOjE3ODk5MzM2OTd9.ekMhaR-lUtS1sNWsERLVDCE1FlAuLuMW9sIZIS4TeKY";
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

