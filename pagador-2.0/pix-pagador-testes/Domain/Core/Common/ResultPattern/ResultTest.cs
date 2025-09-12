using Domain.Core.Common.ResultPattern;
using System;
using Xunit;

namespace pix_pagador_testes.Domain.Core.Common.ResultPattern
{
    public class ResultTests
    {
        #region Success Tests

        [Fact]
        public void Success_WithValue_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var expectedValue = "test value";

            // Act
            var result = Result<string>.Success(expectedValue);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, result.Value);
            Assert.Null(result.Error);
            Assert.Equal(0, result.ErrorCode);
        }

        [Fact]
        public void Success_WithNullValue_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = Result<string>.Success(null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Null(result.Error);
            Assert.Equal(0, result.ErrorCode);
        }

        [Fact]
        public void Success_WithComplexObject_ShouldCreateSuccessfulResult()
        {
            // Arrange
            var person = new { Name = "John", Age = 30 };

            // Act
            var result = Result<object>.Success(person);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(person, result.Value);
            Assert.Null(result.Error);
            Assert.Equal(0, result.ErrorCode);
        }

        #endregion

        #region Failure Tests

        [Fact]
        public void Failure_WithErrorMessage_ShouldCreateFailedResult()
        {
            // Arrange
            var errorMessage = "Something went wrong";

            // Act
            var result = Result<string>.Failure(errorMessage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(default(string), result.Value);
            Assert.Equal(errorMessage, result.Error);
            Assert.Equal(-1, result.ErrorCode);
        }

        [Fact]
        public void Failure_WithErrorMessageAndCode_ShouldCreateFailedResult()
        {
            // Arrange
            var errorMessage = "Validation failed";
            var errorCode = 400;

            // Act
            var result = Result<int>.Failure(errorMessage, errorCode);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(default(int), result.Value);
            Assert.Equal(errorMessage, result.Error);
            Assert.Equal(errorCode, result.ErrorCode);
        }

        [Fact]
        public void Failure_WithNullErrorMessage_ShouldCreateFailedResult()
        {
            // Act
            var result = Result<string>.Failure(null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(default(string), result.Value);
            Assert.Null(result.Error);
            Assert.Equal(-1, result.ErrorCode);
        }

        [Fact]
        public void Failure_WithEmptyErrorMessage_ShouldCreateFailedResult()
        {
            // Act
            var result = Result<string>.Failure(string.Empty);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(default(string), result.Value);
            Assert.Equal(string.Empty, result.Error);
            Assert.Equal(-1, result.ErrorCode);
        }

        #endregion

        #region Match Tests

        [Fact]
        public void Match_WhenSuccess_ShouldExecuteOnSuccessFunction()
        {
            // Arrange
            var value = 42;
            var result = Result<int>.Success(value);

            // Act
            var output = result.Match(
                onSuccess: v => $"Success: {v}",
                onFailure: (error, code) => $"Failure: {error} ({code})"
            );

            // Assert
            Assert.Equal("Success: 42", output);
        }

        [Fact]
        public void Match_WhenFailure_ShouldExecuteOnFailureFunction()
        {
            // Arrange
            var result = Result<int>.Failure("Error occurred", 500);

            // Act
            var output = result.Match(
                onSuccess: v => $"Success: {v}",
                onFailure: (error, code) => $"Failure: {error} ({code})"
            );

            // Assert
            Assert.Equal("Failure: Error occurred (500)", output);
        }

        [Fact]
        public void Match_WithDifferentReturnType_ShouldWork()
        {
            // Arrange
            var result = Result<string>.Success("hello");

            // Act
            var length = result.Match(
                onSuccess: s => s?.Length ?? 0,
                onFailure: (error, code) => -1
            );

            // Assert
            Assert.Equal(5, length);
        }

        #endregion

        #region Map Tests

        [Fact]
        public void Map_WhenSuccess_ShouldTransformValue()
        {
            // Arrange
            var result = Result<int>.Success(5);

            // Act
            var mappedResult = result.Map(x => x * 2);

            // Assert
            Assert.True(mappedResult.IsSuccess);
            Assert.Equal(10, mappedResult.Value);
            Assert.Null(mappedResult.Error);
            Assert.Equal(0, mappedResult.ErrorCode);
        }

        [Fact]
        public void Map_WhenFailure_ShouldReturnFailureWithSameError()
        {
            // Arrange
            var result = Result<int>.Failure("Original error", 404);

            // Act
            var mappedResult = result.Map(x => x * 2);

            // Assert
            Assert.False(mappedResult.IsSuccess);
            Assert.Equal(default(int), mappedResult.Value);
            Assert.Equal("Original error", mappedResult.Error);
            Assert.Equal(404, mappedResult.ErrorCode);
        }

        [Fact]
        public void Map_WithTypeConversion_ShouldWork()
        {
            // Arrange
            var result = Result<int>.Success(42);

            // Act
            var stringResult = result.Map(x => x.ToString());

            // Assert
            Assert.True(stringResult.IsSuccess);
            Assert.Equal("42", stringResult.Value);
        }

        [Fact]
        public void Map_WhenMapperThrows_ShouldPropagateException()
        {
            // Arrange
            var result = Result<int>.Success(0);

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() =>
                result.Map(x => 10 / x));
        }

        #endregion

        #region Implicit Conversion Tests

        [Fact]
        public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
        {
            // Act
            Result<string> result = "test value";

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("test value", result.Value);
            Assert.Null(result.Error);
            Assert.Equal(0, result.ErrorCode);
        }

        [Fact]
        public void ImplicitConversion_FromNull_ShouldCreateSuccessResultWithNull()
        {
            // Act
            Result<string> result = (string)null;

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Null(result.Error);
            Assert.Equal(0, result.ErrorCode);
        }

        [Fact]
        public void ImplicitConversion_WithComplexType_ShouldWork()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            Result<List<int>> result = list;

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(list, result.Value);
        }

        #endregion

        #region GetValueOrThrow Tests

        [Fact]
        public void GetValueOrThrow_WhenSuccess_ShouldReturnValue()
        {
            // Arrange
            var result = Result<string>.Success("test value");

            // Act
            var value = result.GetValueOrThrow((error, code) => new InvalidOperationException(error));

            // Assert
            Assert.Equal("test value", value);
        }

        [Fact]
        public void GetValueOrThrow_WhenFailure_ShouldThrowSpecifiedException()
        {
            // Arrange
            var result = Result<string>.Failure("Something failed", 400);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                result.GetValueOrThrow((error, code) => new ArgumentException($"{error} (Code: {code})")));

            Assert.Equal("Something failed (Code: 400)", exception.Message);
        }


        [Fact]
        public void GetValueOrThrow_WithCustomExceptionFactory_ShouldUseCustomException()
        {
            // Arrange
            var result = Result<int>.Failure("Custom error", 404);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                result.GetValueOrThrow((error, code) => new NotSupportedException($"Custom: {error}")));

            Assert.Equal("Custom: Custom error", exception.Message);
        }

        #endregion

        #region Chain Operations Tests

        [Fact]
        public void ChainedOperations_SuccessPath_ShouldWorkCorrectly()
        {
            // Arrange
            var initialValue = 10;

            // Act
            var result = Result<int>.Success(initialValue)
                .Map(x => x * 2)
                .Bind(x => x > 15 ? Result<string>.Success($"Large: {x}") : Result<string>.Failure("Too small"))
                .Map(s => s.ToUpper());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("LARGE: 20", result.Value);
        }

        [Fact]
        public void ChainedOperations_FailurePath_ShouldShortCircuit()
        {
            // Arrange
            var initialValue = 5;

            // Act
            var result = Result<int>.Success(initialValue)
                .Map(x => x * 2)
                .Bind(x => x > 15 ? Result<string>.Success($"Large: {x}") : Result<string>.Failure("Too small", 400))
                .Map(s => s.ToUpper()); // Não deve ser executado

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Too small", result.Error);
            Assert.Equal(400, result.ErrorCode);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Result_WithValueType_ShouldHandleDefaults()
        {
            // Arrange & Act
            var successResult = Result<int>.Success(0);
            var failureResult = Result<int>.Failure("Error");

            // Assert
            Assert.True(successResult.IsSuccess);
            Assert.Equal(0, successResult.Value);

            Assert.False(failureResult.IsSuccess);
            Assert.Equal(0, failureResult.Value); // default(int)
        }

        [Fact]
        public void Result_WithNullableType_ShouldWork()
        {
            // Act
            var successWithValue = Result<int?>.Success(42);
            var successWithNull = Result<int?>.Success(null);
            var failure = Result<int?>.Failure("Error");

            // Assert
            Assert.True(successWithValue.IsSuccess);
            Assert.Equal(42, successWithValue.Value);

            Assert.True(successWithNull.IsSuccess);
            Assert.Null(successWithNull.Value);

            Assert.False(failure.IsSuccess);
            Assert.Null(failure.Value); // default(int?)
        }

        #endregion
    }
}