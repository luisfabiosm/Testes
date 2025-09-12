using Domain.Core.Common.Base;
using System;
using Xunit;
using T = System.String;

namespace pix_pagador_testes.Domain.Core.Common.Base
{
    public class BaseResponseTest
    {
        private BaseResponse<T> _testClass;
        private T _data;
        private string _message;
        private bool _success;
        private int _errorCode;

        public BaseResponseTest()
        {
            _data = "TestValue1692786951";
            _message = "Success message";
            _success = true;
            _errorCode = 0;

            _testClass = BaseResponse<T>.CreateSuccess(_data, _message);
        }

        [Fact]
        public void CanConstructDefault()
        {
            // Act
            var instance = new BaseResponse<T>();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanCreateSuccess()
        {
            // Act
            var instance = BaseResponse<T>.CreateSuccess(_data, _message);

            // Assert
            Assert.NotNull(instance);
            Assert.True(instance.Success);
            Assert.Equal(_data, instance.Data);
            Assert.Equal(_message, instance.Message);
            Assert.Equal(0, instance.ErrorCode);
        }

        [Fact]
        public void CanCreateSuccessWithDefaultMessage()
        {
            // Act
            var instance = BaseResponse<T>.CreateSuccess(_data);

            // Assert
            Assert.NotNull(instance);
            Assert.True(instance.Success);
            Assert.Equal(_data, instance.Data);
            Assert.Equal("Sucesso", instance.Message);
            Assert.Equal(0, instance.ErrorCode);
        }

        [Fact]
        public void CanCreateError()
        {
            // Arrange
            var errorMessage = "Error occurred";
            var errorCode = 500;

            // Act
            var instance = BaseResponse<T>.CreateError(errorMessage, errorCode);

            // Assert
            Assert.NotNull(instance);
            Assert.False(instance.Success);
            Assert.Equal(errorMessage, instance.Message);
            Assert.Equal(errorCode, instance.ErrorCode);
            Assert.Equal(default(T), instance.Data);
        }

        [Fact]
        public void CanCreateErrorWithDefaultErrorCode()
        {
            // Arrange
            var errorMessage = "Error occurred";

            // Act
            var instance = BaseResponse<T>.CreateError(errorMessage);

            // Assert
            Assert.NotNull(instance);
            Assert.False(instance.Success);
            Assert.Equal(errorMessage, instance.Message);
            Assert.Equal(-1, instance.ErrorCode);
            Assert.Equal(default(T), instance.Data);
        }

        [Fact]
        public void SuccessIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_success, _testClass.Success);
        }

        [Fact]
        public void MessageIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_message, _testClass.Message);
        }

        [Fact]
        public void ErrorCodeIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_errorCode, _testClass.ErrorCode);
        }

        [Fact]
        public void DataIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_data, _testClass.Data);
        }

        [Fact]
        public void PropertiesCanBeSet()
        {
            // Arrange
            var newData = "NewTestValue";
            var newMessage = "New message";
            var newSuccess = false;
            var newErrorCode = 404;

            // Act
            _testClass.Success = newSuccess;
            _testClass.Message = newMessage;
            _testClass.ErrorCode = newErrorCode;
            _testClass.Data = newData;

            // Assert
            Assert.Equal(newSuccess, _testClass.Success);
            Assert.Equal(newMessage, _testClass.Message);
            Assert.Equal(newErrorCode, _testClass.ErrorCode);
            Assert.Equal(newData, _testClass.Data);
        }
    }
}
