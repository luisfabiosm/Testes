using Domain.Core.Common.Base;
using Domain.Core.Enum;
using Domain.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Exceptions
{

    public class InternalExceptionTest
    {
        private InternalException _testClass;
        private string _message;
        private int _errorCode;
        private object _details;
        private Exception _innerException;

        public InternalExceptionTest()
        {
            _message = "Erro interno de teste";
            _errorCode = 500;
            _details = new { Detail = "Detalhes do erro" };
            _innerException = new InvalidOperationException("Inner exception");
            _testClass = new InternalException(_message);
        }

        [Fact]
        public void CanConstructWithMessage()
        {
            // Act
            var instance = new InternalException(_message);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_message, instance.Message);
        }

        [Fact]
        public void ConstructorWithMessageInitializesErrorCode()
        {
            // Act
            var instance = new InternalException(_message);

            // Assert
            Assert.Equal(-1, instance.ErrorCode);
        }

        [Fact]
        public void ConstructorWithMessageInitializesErros()
        {
            // Act
            var instance = new InternalException(_message);

            // Assert
            Assert.NotNull(instance.erros);
            Assert.Single(instance.erros);

            var error = instance.erros.First() as BaseError;
            Assert.NotNull(error);
            Assert.Equal(_message, error.message);
            Assert.Equal(EnumErrorType.System, error.type);
        }

        [Fact]
        public void CanConstructWithMessageErrorCodeAndDetails()
        {
            // Act
            var instance = new InternalException(_message, _errorCode, _details);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_message, instance.Message);
            Assert.Equal(_errorCode, instance.ErrorCode);
            Assert.NotNull(instance.erros);
            Assert.Single(instance.erros);
        }

        [Fact]
        public void ConstructorWithDetailsAddsCorrectError()
        {
            // Act
            var instance = new InternalException(_message, _errorCode, _details);

            // Assert
            var error = instance.erros.First() as BaseError;
            Assert.NotNull(error);
            Assert.Equal(_errorCode, error.code);
            Assert.Equal(_message, error.message);
            Assert.Equal(EnumErrorType.System, error.type);
        }

        [Fact]
        public void CanConstructWithMessageErrorCodeAndInnerException()
        {
            // Act
            var instance = new InternalException(_message, _errorCode, _innerException);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_message, instance.Message);
            Assert.Equal(_errorCode, instance.ErrorCode);
            Assert.Equal(_innerException, instance.InnerException);
            Assert.NotNull(instance.erros);
            Assert.Single(instance.erros);
        }

        [Fact]
        public void ConstructorWithInnerExceptionAddsCorrectError()
        {
            // Act
            var instance = new InternalException(_message, _errorCode, _innerException);

            // Assert
            var error = instance.erros.First() as BaseError;
            Assert.NotNull(error);
            Assert.Equal(_message, error.message);
            Assert.Equal(EnumErrorType.System, error.type);
        }

        [Fact]
        public void InheritsFromException()
        {
            // Assert
            Assert.IsAssignableFrom<Exception>(_testClass);
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
            Assert.Equal(-1, _testClass.ErrorCode);
        }

        [Fact]
        public void ErrosListIsInitializedCorrectly()
        {
            // Assert
            Assert.NotNull(_testClass.erros);
            Assert.Single(_testClass.erros);
        }


        [Fact]
        public void CanHandleEmptyMessage()
        {
            // Act
            var instance = new InternalException(string.Empty);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(string.Empty, instance.Message);
            Assert.NotNull(instance.erros);
        }

        [Fact]
        public void CanHandleZeroErrorCode()
        {
            // Act
            var instance = new InternalException(_message, 0, _details);

            // Assert
            Assert.Equal(0, instance.ErrorCode);
        }

        [Fact]
        public void CanHandleNegativeErrorCode()
        {
            // Act
            var instance = new InternalException(_message, -500, _details);

            // Assert
            Assert.Equal(-500, instance.ErrorCode);
        }

        [Fact]
        public void CanHandleNullDetails()
        {
            // Act
            var instance = new InternalException(_message, _errorCode, (object)null);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorCode, instance.ErrorCode);
            Assert.NotNull(instance.erros);
        }

        [Fact]
        public void CanHandleNullInnerException()
        {
            // Act
            var instance = new InternalException(_message, _errorCode, (Exception)null);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorCode, instance.ErrorCode);
            Assert.Null(instance.InnerException);
            Assert.NotNull(instance.erros);
        }

        [Fact]
        public void AllErrorsHaveSystemType()
        {
            // Arrange
            var instance1 = new InternalException(_message);
            var instance2 = new InternalException(_message, _errorCode, _details);
            var instance3 = new InternalException(_message, _errorCode, _innerException);

            // Assert
            foreach (var error in instance1.erros.Cast<BaseError>())
                Assert.Equal(EnumErrorType.System, error.type);

            foreach (var error in instance2.erros.Cast<BaseError>())
                Assert.Equal(EnumErrorType.System, error.type);

            foreach (var error in instance3.erros.Cast<BaseError>())
                Assert.Equal(EnumErrorType.System, error.type);
        }
    }


}
