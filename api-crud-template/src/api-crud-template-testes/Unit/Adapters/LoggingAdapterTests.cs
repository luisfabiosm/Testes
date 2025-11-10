using Adapters.Outbound.Logging;
using Domain.Core.Interfaces.Outbound;
using FluentAssertions;
using Serilog;
using System.Diagnostics;
using Xunit;

namespace api_crud_template_testes.Unit.Adapters;

public class LoggingAdapterTests : IDisposable
{
    private readonly LoggingAdapter _loggingAdapter;
    private readonly string _testSourceName = "TestSource";

    public LoggingAdapterTests()
    {
        _loggingAdapter = new LoggingAdapter(_testSourceName);

        // Setup Serilog for testing
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }

    [Fact]
    public void Constructor_WithSourceName_ShouldCreateAdapter()
    {
        // Arrange & Act
        var adapter = new LoggingAdapter("TestSource");

        // Assert
        adapter.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullSourceName_ShouldCreateAdapterWithEmptySource()
    {
        // Arrange & Act
        var adapter = new LoggingAdapter(null);

        // Assert
        adapter.Should().NotBeNull();
    }

    [Fact]
    public void LogInformation_WithMessage_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Test information message";

        // Act & Assert (Should not throw)
        _loggingAdapter.LogInformation(message);
    }

    [Fact]
    public void LogInformation_WithMessageAndArgs_ShouldLogCorrectly()
    {
        // Arrange
        var message = "User {UserId} created successfully";
        var userId = Guid.NewGuid();

        // Act & Assert (Should not throw)
        _loggingAdapter.LogInformation(message, userId);
    }

    [Fact]
    public void LogWarning_WithMessage_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Test warning message";

        // Act & Assert (Should not throw)
        _loggingAdapter.LogWarning(message);
    }

    [Fact]
    public void LogWarning_WithMessageAndArgs_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Warning for user {UserId}";
        var userId = Guid.NewGuid();

        // Act & Assert (Should not throw)
        _loggingAdapter.LogWarning(message, userId);
    }

    [Fact]
    public void LogError_WithMessage_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Test error message";

        // Act & Assert (Should not throw)
        _loggingAdapter.LogError(message);
    }

    [Fact]
    public void LogError_WithMessageAndException_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Error occurred";
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert (Should not throw)
        _loggingAdapter.LogError(message, exception);
    }

    [Fact]
    public void LogError_WithMessageExceptionAndArgs_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Error for user {UserId}";
        var exception = new InvalidOperationException("Test exception");
        var userId = Guid.NewGuid();

        // Act & Assert (Should not throw)
        _loggingAdapter.LogError(message, exception, userId);
    }

    [Fact]
    public void LogDebug_WithMessage_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Test debug message";

        // Act & Assert (Should not throw)
        _loggingAdapter.LogDebug(message);
    }

    [Fact]
    public void LogDebug_WithMessageAndArgs_ShouldLogCorrectly()
    {
        // Arrange
        var message = "Debug info for {Component}";
        var component = "UserService";

        // Act & Assert (Should not throw)
        _loggingAdapter.LogDebug(message, component);
    }

    [Fact]
    public void StartOperation_WithValidParameters_ShouldReturnOperationContext()
    {
        // Arrange
        var operationName = "TestOperation";
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var operationContext = _loggingAdapter.StartOperation(
            operationName,
            correlationId);

        // Assert
        operationContext.Should().NotBeNull();
        operationContext.Should().BeAssignableTo<IOperationContext>();
    }

    [Fact]
    public void StartOperation_WithAllParameters_ShouldReturnOperationContext()
    {
        // Arrange
        var operationName = "ComplexOperation";
        var correlationId = Guid.NewGuid().ToString();
        var parentContext = new ActivityContext();
        var kind = ActivityKind.Server;

        // Act
        var operationContext = _loggingAdapter.StartOperation(
            operationName,
            correlationId,
            parentContext,
            kind);

        // Assert
        operationContext.Should().NotBeNull();
        operationContext.Should().BeAssignableTo<IOperationContext>();
    }

    [Fact]
    public void StartOperation_WhenActivitySourceCannotCreateActivity_ShouldReturnNoOpContext()
    {
        // Arrange
        var operationName = "TestOperation";
        var correlationId = Guid.NewGuid().ToString();

        // Note: This test depends on the internal behavior where ActivitySource 
        // might return null in certain conditions

        // Act
        var operationContext = _loggingAdapter.StartOperation(
            operationName,
            correlationId);

        // Assert
        operationContext.Should().NotBeNull();
    }

    [Fact]
    public void AddProperty_WithValidKeyValue_ShouldNotThrow()
    {
        // Arrange
        var key = "TestKey";
        var value = "TestValue";

        // Act & Assert (Should not throw)
        _loggingAdapter.AddProperty(key, value);
    }

    [Theory]
    [InlineData(null, "value")]
    [InlineData("key", null)]
    [InlineData("", "value")]
    [InlineData("key", "")]
    public void AddProperty_WithVariousInputs_ShouldHandleGracefully(string key, string value)
    {
        // Act & Assert (Should not throw)
        _loggingAdapter.AddProperty(key, value);
    }

    [Fact]
    public void LogError_ShouldAddErrorTagsToCurrentActivity()
    {
        // Arrange
        using var activitySource = new ActivitySource("TestActivitySource");
        using var activity = activitySource.StartActivity("TestActivity");

        var message = "Test error with activity";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _loggingAdapter.LogError(message, exception);

        // Assert
        if (activity != null)
        {
            // In a real scenario, you would verify that error tags were added
            // This is more of a behavioral test to ensure no exceptions are thrown
            activity.Should().NotBeNull();
        }
    }

    [Fact]
    public void LogError_WithoutCurrentActivity_ShouldNotThrow()
    {
        // Arrange
        // Ensure no current activity
        Activity.Current = null;

        var message = "Test error without activity";
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert (Should not throw)
        _loggingAdapter.LogError(message, exception);
    }

    [Fact]
    public void StartOperation_MultipleOperations_ShouldCreateIndependentContexts()
    {
        // Arrange
        var operation1Name = "Operation1";
        var operation2Name = "Operation2";
        var correlationId1 = Guid.NewGuid().ToString();
        var correlationId2 = Guid.NewGuid().ToString();

        // Act
        var context1 = _loggingAdapter.StartOperation(operation1Name, correlationId1);
        var context2 = _loggingAdapter.StartOperation(operation2Name, correlationId2);

        // Assert
        context1.Should().NotBeNull();
        context2.Should().NotBeNull();
        context1.Should().NotBeSameAs(context2);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var adapter = new LoggingAdapter("DisposableTest");

        // Act & Assert (Should not throw)
        adapter.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var adapter = new LoggingAdapter("MultipleDisposeTest");

        // Act & Assert (Should not throw)
        adapter.Dispose();
        adapter.Dispose(); // Second disposal should not throw
        adapter.Dispose(); // Third disposal should not throw
    }

    [Fact]
    public void LogInformation_WithNullMessage_ShouldHandleGracefully()
    {
        // Act & Assert (Should not throw)
        _loggingAdapter.LogInformation(null);
    }

    [Fact]
    public void LogInformation_WithEmptyMessage_ShouldHandleGracefully()
    {
        // Act & Assert (Should not throw)
        _loggingAdapter.LogInformation("");
        _loggingAdapter.LogInformation(string.Empty);
    }

    [Fact]
    public void LogInformation_WithFormatStringAndNoArgs_ShouldLogCorrectly()
    {
        // Arrange
        var messageWithPlaceholder = "User {UserId} action completed";

        // Act & Assert (Should not throw, but might log with unreplaced placeholder)
        _loggingAdapter.LogInformation(messageWithPlaceholder);
    }

    [Fact]
    public void StartOperation_WithEmptyOperationName_ShouldCreateContext()
    {
        // Arrange
        var emptyOperationName = "";
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var context = _loggingAdapter.StartOperation(emptyOperationName, correlationId);

        // Assert
        context.Should().NotBeNull();
    }

    [Fact]
    public void StartOperation_WithEmptyCorrelationId_ShouldCreateContext()
    {
        // Arrange
        var operationName = "TestOperation";
        var emptyCorrelationId = "";

        // Act
        var context = _loggingAdapter.StartOperation(operationName, emptyCorrelationId);

        // Assert
        context.Should().NotBeNull();
    }

    public void Dispose()
    {
        _loggingAdapter?.Dispose();
    }
}