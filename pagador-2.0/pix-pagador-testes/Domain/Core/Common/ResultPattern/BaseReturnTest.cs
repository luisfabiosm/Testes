using Domain.Core.Common.ResultPattern;
using Domain.Core.Exceptions;
using T = System.String;

namespace pix_pagador_testes.Domain.Core.Common.ResultPattern
{
    public class BaseReturnTest
    {
        private BaseReturn<T> _testClass;
        private T _data;
        private string _message;
        private string _correlationId;
        private string _error;
        private int _errorCode;
        private SpsErroReturn _errorDetails;

        public BaseReturnTest()
        {
            _data = "TestValue1692786951";
            _message = "Success message";
            _correlationId = "test-correlation-id";
            _error = "Test error message";
            _errorCode = 400;
            _errorDetails = SpsErroReturn.Create(1, 100, "Test SPS error", "test-origin");

            _testClass = BaseReturn<T>.FromSuccess(_data, _message, _correlationId);
        }

        [Fact]
        public void CanConstructFromSuccess()
        {
            // Act
            var instance = BaseReturn<T>.FromSuccess(_data, _message, _correlationId);

            // Assert
            Assert.NotNull(instance);
            Assert.True(instance.Success);
            Assert.Equal(_data, instance.Data);
            Assert.Equal(_message, instance.Message);
            Assert.Equal(_correlationId, instance.CorrelationId);
        }

        [Fact]
        public void CanConstructFromFailure()
        {
            // Act
            var instance = BaseReturn<T>.FromFailure(_error, _errorCode, _correlationId, _errorDetails);

            // Assert
            Assert.NotNull(instance);
            Assert.False(instance.Success);
            Assert.Equal(_error, instance.Message);
            Assert.Equal(_errorCode, instance.ErrorCode);
            Assert.Equal(_correlationId, instance.CorrelationId);
            Assert.Equal(_errorDetails, instance.ErrorDetails);
        }

        [Fact]
        public void CanConstructFromException()
        {
            // Arrange
            var businessException = BusinessException.Create("Business error", 400, "test-origin");

            // Act
            var instance = BaseReturn<T>.FromException(businessException, _correlationId);

            // Assert
            Assert.NotNull(instance);
            Assert.False(instance.Success);
            Assert.Equal(businessException.Message, instance.Message);
            Assert.Equal(businessException.ErrorCode, instance.ErrorCode);
            Assert.Equal(_correlationId, instance.CorrelationId);
            Assert.NotNull(instance.ErrorDetails);
        }

        [Fact]
        public void CanConstructFromValidationValid()
        {
            // Arrange
            var validation = ValidationResult.Valid();

            // Act
            var instance = BaseReturn<T>.FromValidation(validation, _correlationId);

            // Assert
            Assert.NotNull(instance);
            Assert.True(instance.Success);
            Assert.Equal("Validation passed", instance.Message);
            Assert.Equal(_correlationId, instance.CorrelationId);
        }

        [Fact]
        public void CanConstructFromValidationInvalid()
        {
            // Arrange
            var errors = new List<ErrorDetails>
            {
                new ErrorDetails("field1", "Error message 1"),
                new ErrorDetails("field2", "Error message 2")
            };
            var validation = ValidationResult.Invalid(errors);

            // Act
            var instance = BaseReturn<T>.FromValidation(validation, _correlationId);

            // Assert
            Assert.NotNull(instance);
            Assert.False(instance.Success);
            Assert.Equal(400, instance.ErrorCode);
            Assert.Equal(_correlationId, instance.CorrelationId);
            Assert.Contains("Error message 1", instance.Message);
            Assert.Contains("Error message 2", instance.Message);
        }

        [Fact]
        public void SuccessPropertyIsInitializedCorrectly()
        {
            // Assert
            Assert.True(_testClass.Success);
        }

        [Fact]
        public void DataPropertyIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_data, _testClass.Data);
        }

        [Fact]
        public void MessagePropertyIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_message, _testClass.Message);
        }

        [Fact]
        public void CorrelationIdPropertyIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_correlationId, _testClass.CorrelationId);
        }

        [Fact]
        public void ErrorCodePropertyIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(0, _testClass.ErrorCode);
        }

        [Fact]
        public void ImplicitConversionToBoolWorks()
        {
            // Arrange
            var successInstance = BaseReturn<T>.FromSuccess(_data);
            var failureInstance = BaseReturn<T>.FromFailure(_error);

            // Assert
            Assert.True(successInstance);
            Assert.False(failureInstance);
        }

        [Fact]
        public void ImplicitConversionFromDataWorks()
        {
            // Act
            BaseReturn<T> instance = _data;

            // Assert
            Assert.NotNull(instance);
            Assert.True(instance.Success);
            Assert.Equal(_data, instance.Data);
        }

        [Fact]
        public void ToResultMethodWorks()
        {
            // Act
            var result = _testClass.Result;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(_data, result.Value);
        }

      
    }
}
