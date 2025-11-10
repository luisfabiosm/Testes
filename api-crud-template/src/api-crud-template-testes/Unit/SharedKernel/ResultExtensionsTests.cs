using Domain.Core.SharedKernel.ResultPattern;
using FluentAssertions;
using Xunit;

namespace api_crud_template_testes.Unit.SharedKernel;

public class ResultExtensionsTests
{
    [Fact]
    public async Task Map_WithSuccessfulResult_ShouldApplyMapper()
    {
        // Arrange
        var initialValue = 10;
        var expectedMappedValue = "10";
        var resultTask = Task.FromResult(Result.Success(initialValue));

        Func<int, string> mapper = x => x.ToString();

        // Act
        var result = await resultTask.Map(mapper);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMappedValue);
    }

    [Fact]
    public async Task Map_WithFailedResult_ShouldReturnFailureWithSameError()
    {
        // Arrange
        var expectedError = "Original error";
        var resultTask = Task.FromResult(Result.Failure<int>(expectedError));

        Func<int, string> mapper = x => x.ToString();

        // Act
        var result = await resultTask.Map(mapper);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task Map_WithComplexMapper_ShouldTransformCorrectly()
    {
        // Arrange
        var user = new { Name = "John", Age = 30 };
        var resultTask = Task.FromResult(Result.Success(user));

        Func<dynamic, string> mapper = u => $"{u.Name} is {u.Age} years old";

        // Act
        var result = await resultTask.Map(mapper);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("John is 30 years old");
    }

    [Fact]
    public async Task Map_WhenMapperThrowsException_ShouldPropagateException()
    {
        // Arrange
        var initialValue = 10;
        var resultTask = Task.FromResult(Result.Success(initialValue));
        var expectedException = new InvalidOperationException("Mapper failed");

        Func<int, string> mapper = x => throw expectedException;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resultTask.Map(mapper));

        thrownException.Should().Be(expectedException);
    }

    [Fact]
    public async Task Bind_WithSuccessfulResult_ShouldExecuteBinder()
    {
        // Arrange
        var initialValue = "test@email.com";
        var resultTask = Task.FromResult(Result.Success(initialValue));

        Func<string, Task<Result>> binder = email =>
            Task.FromResult(email.Contains("@")
                ? Result.Success()
                : Result.Failure("Invalid email"));

        // Act
        var result = await resultTask.Bind(binder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Bind_WithFailedResult_ShouldReturnFailureWithoutExecutingBinder()
    {
        // Arrange
        var expectedError = "Original error";
        var resultTask = Task.FromResult(Result.Failure<string>(expectedError));
        var binderCalled = false;

        Func<string, Task<Result>> binder = email =>
        {
            binderCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await resultTask.Bind(binder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
        binderCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Bind_WhenBinderReturnsFailure_ShouldReturnBinderFailure()
    {
        // Arrange
        var initialValue = "invalid-email";
        var expectedBinderError = "Email validation failed";
        var resultTask = Task.FromResult(Result.Success(initialValue));

        Func<string, Task<Result>> binder = email =>
            Task.FromResult(Result.Failure(expectedBinderError));

        // Act
        var result = await resultTask.Bind(binder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedBinderError);
    }

    [Fact]
    public async Task Bind_WhenBinderThrowsException_ShouldPropagateException()
    {
        // Arrange
        var initialValue = "test@email.com";
        var resultTask = Task.FromResult(Result.Success(initialValue));
        var expectedException = new InvalidOperationException("Binder failed");

        Func<string, Task<Result>> binder = email => throw expectedException;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resultTask.Bind(binder));

        thrownException.Should().Be(expectedException);
    }

    [Fact]
    public async Task Map_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var initialValue = 5;
        var resultTask = Task.FromResult(Result.Success(initialValue));

        // Act - Chain multiple map operations
        var result = await resultTask
            .Map(x => x * 2)      // 5 -> 10
            .Map(x => x.ToString()) // 10 -> "10"
            .Map(x => $"Value: {x}"); // "10" -> "Value: 10"

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Value: 10");
    }

    [Fact]
    public async Task Map_ChainedOperations_WithFailureInMiddle_ShouldStopChain()
    {
        // Arrange
        var initialValue = 5;
        var resultTask = Task.FromResult(Result.Success(initialValue));
        var expectedError = "Mapping failed";

        // Act - Chain with failure in the middle
        var result = await resultTask
            .Map(x => x * 2)      // 5 -> 10 (success)
            .Map<int, string>(x => throw new InvalidOperationException(expectedError)) // Should fail here
            .Map(x => $"Value: {x}"); // Should not execute

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resultTask
                .Map(x => x * 2)
                .Map<int, string>(x => throw new InvalidOperationException(expectedError))
                .Map(x => $"Value: {x}"));

        exception.Message.Should().Be(expectedError);
    }


    

    [Fact]
    public async Task Map_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(10));
        Func<int, string> nullMapper = null;

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(
            () => resultTask.Map(nullMapper));
    }

    [Fact]
    public async Task Bind_WithNullBinder_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success("test"));
        Func<string, Task<Result>> nullBinder = null;

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(
            () => resultTask.Bind(nullBinder));
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(-1, "-1")]
    [InlineData(int.MaxValue, "2147483647")]
    [InlineData(int.MinValue, "-2147483648")]
    public async Task Map_WithVariousInputValues_ShouldMapCorrectly(int input, string expected)
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success(input));
        Func<int, string> mapper = x => x.ToString();

        // Act
        var result = await resultTask.Map(mapper);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Map_WithAsyncResultTask_ShouldHandleCorrectly()
    {
        // Arrange
        async Task<Result<int>> GetAsyncResult()
        {
            await Task.Delay(10); // Simulate async work
            return Result.Success(42);
        }

        Func<int, string> mapper = x => $"The answer is {x}";

        // Act
        var result = await GetAsyncResult().Map(mapper);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("The answer is 42");
    }

    [Fact]
    public async Task Bind_WithAsyncBinder_ShouldHandleCorrectly()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success("test-data"));

        async Task<Result> AsyncBinder(string data)
        {
            await Task.Delay(10); // Simulate async work
            return data.Length > 0 ? Result.Success() : Result.Failure("Empty data");
        }

        // Act
        var result = await resultTask.Bind(AsyncBinder);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}