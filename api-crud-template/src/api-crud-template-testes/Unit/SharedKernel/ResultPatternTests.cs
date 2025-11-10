using Domain.Core.SharedKernel.ResultPattern;
using FluentAssertions;
using Xunit;

namespace api_crud_template_testes.Unit.SharedKernel;

public class ResultPatternTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailureResult()
    {
        // Arrange
        var expectedError = "Something went wrong";

        // Act
        var result = Result.Failure(expectedError);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        var expectedValue = "Test Value";

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Failure_WithGenericType_ShouldCreateFailureResultWithDefaultValue()
    {
        // Arrange
        var expectedError = "Generic failure";

        // Act
        var result = Result.Failure<string>(expectedError);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
        // For reference types, default value is null
    }

    [Fact]
    public void Value_WhenResultIsFailure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<string>("Error occurred");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        exception.Message.Should().Contain("Cannot access value when result is failure");
    }

    [Fact]
    public void Value_WhenResultIsSuccess_ShouldReturnValue()
    {
        // Arrange
        var expectedValue = 42;
        var result = Result.Success(expectedValue);

        // Act
        var actualValue = result.Value;

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void Result_WithSuccessAndError_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new TestableResult(true, "Error message"));

        exception.Should().NotBeNull();
    }

    [Fact]
    public void Result_WithFailureAndEmptyError_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        var exception1 = Assert.Throws<InvalidOperationException>(
            () => new TestableResult(false, string.Empty));

        var exception2 = Assert.Throws<InvalidOperationException>(
            () => new TestableResult(false, null));

        exception1.Should().NotBeNull();
        exception2.Should().NotBeNull();
    }

    [Fact]
    public void ResultGeneric_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var intResult = Result.Success(123);
        var stringResult = Result.Success("Hello World");
        var boolResult = Result.Success(true);
        var guidResult = Result.Success(Guid.NewGuid());

        // Assert
        intResult.Value.Should().Be(123);
        stringResult.Value.Should().Be("Hello World");
        boolResult.Value.Should().BeTrue();
        guidResult.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ResultGeneric_WithValueTypes_ShouldHandleDefaultValues()
    {
        // Arrange & Act
        var intFailureResult = Result.Failure<int>("Integer error");
        var boolFailureResult = Result.Failure<bool>("Boolean error");
        var guidFailureResult = Result.Failure<Guid>("Guid error");

        // Assert
        intFailureResult.IsFailure.Should().BeTrue();
        boolFailureResult.IsFailure.Should().BeTrue();
        guidFailureResult.IsFailure.Should().BeTrue();

        // Values should throw when accessed
        Assert.Throws<InvalidOperationException>(() => intFailureResult.Value);
        Assert.Throws<InvalidOperationException>(() => boolFailureResult.Value);
        Assert.Throws<InvalidOperationException>(() => guidFailureResult.Value);
    }

    [Fact]
    public void ResultGeneric_WithReferenceTypes_ShouldHandleNullValues()
    {
        // Arrange & Act
        var stringFailureResult = Result.Failure<string>("String error");
        var objectFailureResult = Result.Failure<object>("Object error");

        // Assert
        stringFailureResult.IsFailure.Should().BeTrue();
        objectFailureResult.IsFailure.Should().BeTrue();

        // Values should throw when accessed
        Assert.Throws<InvalidOperationException>(() => stringFailureResult.Value);
        Assert.Throws<InvalidOperationException>(() => objectFailureResult.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Failure_WithNullOrEmptyError_ShouldThrowInvalidOperationException(string error)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => Result.Failure(error));

        exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Simple error")]
    [InlineData("Error with special chars: !@#$%^&*()")]
    [InlineData("Very long error message that contains a lot of text to test how the Result pattern handles longer error messages")]
    public void Failure_WithVariousErrorMessages_ShouldPreserveErrorMessage(string errorMessage)
    {
        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.Error.Should().Be(errorMessage);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ResultPattern_ShouldBeImmutable()
    {
        // Arrange
        var result1 = Result.Success("Original Value");
        var result2 = Result.Success("Original Value");

        // Act & Assert
        result1.IsSuccess.Should().Be(result2.IsSuccess);
        result1.Value.Should().Be(result2.Value);

        // Results should be independent
        result1.Should().NotBeSameAs(result2);
    }

    [Fact]
    public void ResultGeneric_WithNullValue_ShouldAllowNullForReferenceTypes()
    {
        // Act
        var result = Result.Success<string>(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // Helper class to test the protected constructor
    private class TestableResult : Result
    {
        public TestableResult(bool isSuccess, string error) : base(isSuccess, error)
        {
        }
    }
}