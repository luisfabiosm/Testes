using Domain.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Exceptions
{
    public class ValidateExceptionTest
    {
        private ValidateException _testClass;
        private string _message;
        private int _errorCode;
        private List<ErrorDetails> _errorDetails;

        public ValidateExceptionTest()
        {
            _message = "Erro de validação de teste";
            _errorCode = 400;
            _errorDetails = new List<ErrorDetails>
            {
                new ErrorDetails("campo1", "Erro no campo 1"),
                new ErrorDetails("campo2", "Erro no campo 2")
            };

            _testClass = new ValidateException(_message);
        }

        [Fact]
        public void CanConstructDefault()
        {
            // Act
            var instance = new ValidateException();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanConstructWithMessage()
        {
            // Act
            var instance = new ValidateException(_message);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_message, instance.Message);
        }

        [Fact]
        public void ErrorCodeIsInitializedToDefault()
        {
            // Assert
            Assert.Equal(-1, _testClass.ErrorCode);
        }

        [Fact]
        public void MessageIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_message, _testClass.Message);
        }

        [Fact]
        public void RequestErrorsIsInitializedToNull()
        {
            // Assert
            Assert.Null(_testClass.RequestErrors);
        }

        [Fact]
        public void CanCreateWithAllParameters()
        {
            // Act
            var instance = ValidateException.Create(_message, _errorCode, _errorDetails, "API");

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
            Assert.NotNull(instance.Message);
            Assert.Contains(_errorDetails[0].mensagens, instance.Message);
            Assert.Contains(_errorDetails[1].mensagens, instance.Message);
        }

        [Fact]
        public void CanCreateWithDefaultOrigin()
        {
            // Act
            var instance = ValidateException.Create(_message, _errorCode, _errorDetails);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
            Assert.NotNull(instance.Message);
        }

        [Fact]
        public void CanCreateWithErrorDetailsList()
        {
            // Act
            var instance = ValidateException.Create(_errorDetails);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
            Assert.NotNull(instance.Message);
            Assert.Contains(_errorDetails[0].mensagens, instance.Message);
            Assert.Contains(_errorDetails[1].mensagens, instance.Message);
        }

        [Fact]
        public void InheritsFromException()
        {
            // Assert
            Assert.IsAssignableFrom<Exception>(_testClass);
        }


        [Fact]
        public void CanHandleEmptyMessage()
        {
            // Act
            var instance = new ValidateException(string.Empty);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(string.Empty, instance.Message);
        }

        [Fact]
        public void CreateWithEmptyErrorDetailsList()
        {
            // Arrange
            var emptyList = new List<ErrorDetails>();

            // Act
            var instance = ValidateException.Create(emptyList);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(emptyList, instance.RequestErrors);
            Assert.NotNull(instance.Message);
        }


        [Fact]
        public void CreateSerializesErrorDetailsToMessage()
        {
            // Act
            var instance = ValidateException.Create(_errorDetails);

            // Assert
            Assert.NotNull(instance.Message);

            // Verificar que a mensagem contém as informações dos erros
            // Testamos se a serialização funciona, não a deserialização
            Assert.Contains(_errorDetails[0].mensagens, instance.Message);
            Assert.Contains(_errorDetails[1].mensagens, instance.Message);
            Assert.Contains(_errorDetails[0].campo, instance.Message);
            Assert.Contains(_errorDetails[1].campo, instance.Message);

            // Verificar que é JSON válido
            Assert.True(IsValidJson(instance.Message), "A mensagem deve ser um JSON válido");
        }

     
        [Fact]
        public void CreateWithSingleErrorDetail()
        {
            // Arrange
            var singleError = new List<ErrorDetails>
            {
                new ErrorDetails("singleField", "Single error message")
            };

            // Act
            var instance = ValidateException.Create(singleError);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(singleError, instance.RequestErrors);
            Assert.Contains("Single error message", instance.Message);
        }

        [Fact]
        public void CreateWithZeroErrorCode()
        {
            // Act
            var instance = ValidateException.Create(_message, 0, _errorDetails);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
        }

        [Fact]
        public void CreateWithNegativeErrorCode()
        {
            // Act
            var instance = ValidateException.Create(_message, -500, _errorDetails);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
        }

        [Fact]
        public void CreateWithNullOrigin()
        {
            // Act
            var instance = ValidateException.Create(_message, _errorCode, _errorDetails, null);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
        }

        [Fact]
        public void CreateWithEmptyOrigin()
        {
            // Act
            var instance = ValidateException.Create(_message, _errorCode, _errorDetails, string.Empty);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_errorDetails, instance.RequestErrors);
        }

        [Fact]
        public void RequestErrorsAreSetCorrectly()
        {
            // Act
            var instance = ValidateException.Create(_errorDetails);

            // Assert
            Assert.Same(_errorDetails, instance.RequestErrors);
        }

        [Fact]
        public void MessageContainsAllErrorMessages()
        {
            // Act
            var instance = ValidateException.Create(_errorDetails);

            // Assert
            foreach (var error in _errorDetails)
            {
                Assert.Contains(error.mensagens, instance.Message);
            }
        }

        [Fact]
        public void CreateWithComplexErrorDetails()
        {
            // Arrange
            var complexErrors = new List<ErrorDetails>
            {
                new ErrorDetails("field.nested", "Nested field error"),
                new ErrorDetails("array[0].property", "Array property error"),
                new ErrorDetails("", "Empty field error"),
                new ErrorDetails("special@chars#field", "Special characters field")
            };

            // Act
            var instance = ValidateException.Create(complexErrors);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(complexErrors, instance.RequestErrors);
            Assert.NotNull(instance.Message);
        }


        private static bool IsValidJson(string jsonString)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }




}
