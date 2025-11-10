using Adapters.Inbound.API.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace api_crud_template_testes.Integration.Middlewares;

public class CorrelationIdMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _middleware = new CorrelationIdMiddleware(_next);
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdInHeader_ShouldUseExistingId()
    {
        // Arrange
        var existingCorrelationId = Guid.NewGuid().ToString();
        var context = CreateHttpContext();

        context.Request.Headers["X-Correlation-ID"] = existingCorrelationId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(existingCorrelationId);
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(existingCorrelationId);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCorrelationIdInHeader_ShouldGenerateNewId()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
        var correlationId = context.Items["CorrelationId"] as string;

        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();

        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);

        await _next.Received(1).Invoke(context);
    }



    [Fact]
    public async Task InvokeAsync_WhenMultipleCorrelationIdsInHeader_ShouldUseFirst()
    {
        // Arrange
        var firstId = Guid.NewGuid().ToString();
        var secondId = Guid.NewGuid().ToString();
        var context = CreateHttpContext();

        context.Request.Headers["X-Correlation-ID"] = new Microsoft.Extensions.Primitives.StringValues(new[] { firstId, secondId });

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(firstId);
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(firstId);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextMiddlewareThrows_ShouldPropagateException()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Test exception");

        _next.When(x => x.Invoke(context)).Do(x => throw expectedException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _middleware.InvokeAsync(context));

        thrownException.Should().Be(expectedException);
    }

    [Theory]
    [InlineData("custom-correlation-123")]
    [InlineData("abc-def-ghi")]
    [InlineData("12345")]
    public async Task InvokeAsync_WithCustomCorrelationIdFormats_ShouldAcceptAnyFormat(string customId)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = customId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(customId);
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(customId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponseHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-ID"] = expectedCorrelationId;

        // Simulate response starting
        var responseStartingCalled = false;
        context.Response.OnStarting(Arg.Do<Func<Task>>(callback =>
        {
            responseStartingCalled = true;
            callback().Wait();
        }));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_GeneratedCorrelationIds_ShouldBeUnique()
    {
        // Arrange
        var context1 = CreateHttpContext();
        var context2 = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);

        // Assert
        var correlationId1 = context1.Items["CorrelationId"] as string;
        var correlationId2 = context2.Items["CorrelationId"] as string;

        correlationId1.Should().NotBe(correlationId2);
        correlationId1.Should().NotBeNullOrEmpty();
        correlationId2.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleNullCorrelationIdGracefully()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = new Microsoft.Extensions.Primitives.StringValues((string)null);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
        var correlationId = context.Items["CorrelationId"] as string;

        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    private static HttpContext CreateHttpContext()
    {
        var context = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var response = Substitute.For<HttpResponse>();
        var headers = new HeaderDictionary();
        var responseHeaders = new HeaderDictionary();
        var items = new Dictionary<object, object>();

        context.Request.Returns(request);
        context.Response.Returns(response);
        context.Items.Returns(items);

        request.Headers.Returns(headers);
        response.Headers.Returns(responseHeaders);

        // Setup OnStarting callback simulation
        response.OnStarting(Arg.Do<Func<Task>>(callback => callback()));

        return context;
    }
}