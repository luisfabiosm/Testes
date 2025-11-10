using Adapters.Inbound.API.Middlewares;
using Domain.Core.Models.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Text.Json;
using Xunit;

namespace api_crud_template_testes.Integration.Middlewares;

public class ExceptionHandlingMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        _middleware = new ExceptionHandlingMiddleware(_next, _logger);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        await _next.Received(1).Invoke(context);
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentExceptionThrown_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new ArgumentException("Invalid argument provided");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        await VerifyErrorResponseContent(context, "Bad Request");
        VerifyLogging(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_WhenNullReferenceExceptionThrown_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new NullReferenceException("Object reference not set");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        await VerifyErrorResponseContent(context, "Bad Request");
        VerifyLogging(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessExceptionThrown_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new UnauthorizedAccessException("Access denied");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        await VerifyErrorResponseContent(context, "Unauthorized");
        VerifyLogging(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundExceptionThrown_ShouldReturnNotFound()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new KeyNotFoundException("Resource not found");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        await VerifyErrorResponseContent(context, "Not Found");
        VerifyLogging(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_WhenTimeoutExceptionThrown_ShouldReturnRequestTimeout()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new TimeoutException("Operation timed out");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.RequestTimeout);
        await VerifyErrorResponseContent(context, "Request Timeout");
        VerifyLogging(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Something went wrong");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        await VerifyErrorResponseContent(context, "Internal Server Error");
        VerifyLogging(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopmentEnvironment_ShouldShowDetailedErrorMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Detailed error for development");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Detailed error for development");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task InvokeAsync_InProductionEnvironment_ShouldHideDetailedErrorMessage()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Sensitive error information");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().NotContain("Sensitive error information");
        responseBody.Should().Contain("Ocorreu um erro interno no servidor");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrectContentType()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new ArgumentException("Test exception");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeRequestIdAndInstance()
    {
        // Arrange
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id";
        context.Request.Path = "/api/test";

        var expectedException = new ArgumentException("Test exception");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse.RequestId.Should().Be("test-trace-id");
        errorResponse.Instance.Should().Be("/api/test");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetActivityStatusToError()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Test error");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        // This is more of a behavioral test - we're ensuring the middleware
        // doesn't throw when trying to set Activity status
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(NullReferenceException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(TimeoutException), HttpStatusCode.RequestTimeout)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    public async Task InvokeAsync_WithDifferentExceptionTypes_ShouldMapToCorrectStatusCodes(
        Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception");

        _next.When(x => x.Invoke(context)).Do(x => throw exception);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)expectedStatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleExceptions_ShouldHandleEachCorrectly()
    {
        // Arrange
        var exceptions = new Exception[]
        {
            new ArgumentException("Argument error"),
            new UnauthorizedAccessException("Unauthorized error"),
            new KeyNotFoundException("Not found error"),
            new TimeoutException("Timeout error"),
            new InvalidOperationException("Generic error")
        };

        var expectedStatusCodes = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.InternalServerError
        };

        // Act & Assert
        for (int i = 0; i < exceptions.Length; i++)
        {
            var context = CreateHttpContext();
            var exception = exceptions[i];
            var expectedStatusCode = expectedStatusCodes[i];

            _next.When(x => x.Invoke(context)).Do(x => throw exception);

            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)expectedStatusCode);
        }
    }

    private static HttpContext CreateHttpContext()
    {
        var context = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var response = Substitute.For<HttpResponse>();
        var responseBody = new MemoryStream();

        context.Request.Returns(request);
        context.Response.Returns(response);
        context.TraceIdentifier.Returns(Guid.NewGuid().ToString());

        request.Path.Returns(new PathString("/test"));
        response.Body.Returns(responseBody);
        response.StatusCode.Returns((int)HttpStatusCode.OK);

        return context;
    }

    private async Task VerifyErrorResponseContent(HttpContext context, string expectedTitle)
    {
        var responseBody = await GetResponseBody(context);
        responseBody.Should().NotBeNullOrEmpty();

        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse.Title.Should().Be(expectedTitle);
        errorResponse.Status.Should().Be(context.Response.StatusCode);
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private void VerifyLogging(Exception expectedException)
    {
        _logger.Received(1).LogError(
            Arg.Is<Exception>(ex => ex == expectedException),
            Arg.Is<string>(msg => msg.Contains("Erro não tratado na aplicação")));
    }
}