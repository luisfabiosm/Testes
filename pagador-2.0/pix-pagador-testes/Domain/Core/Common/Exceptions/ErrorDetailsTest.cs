using Domain.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Exceptions
{
    public class ErrorDetailsTest
    {
        [Fact]
        public void CanConstruct()
        {
            // Arrange
            var campo = "testField";
            var mensagens = "Test error message";

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(campo, instance.campo);
            Assert.Equal(mensagens, instance.mensagens);
        }

        [Fact]
        public void CampoIsInitializedCorrectly()
        {
            // Arrange
            var campo = "testField";
            var mensagens = "Test error message";

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal(campo, instance.campo);
        }

        [Fact]
        public void MensagensIsInitializedCorrectly()
        {
            // Arrange
            var campo = "testField";
            var mensagens = "Test error message";

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal(mensagens, instance.mensagens);
        }

        [Fact]
        public void CanHandleNullCampo()
        {
            // Arrange
            string campo = null;
            var mensagens = "Test error message";

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Null(instance.campo);
            Assert.Equal(mensagens, instance.mensagens);
        }

        [Fact]
        public void CanHandleNullMensagens()
        {
            // Arrange
            var campo = "testField";
            string mensagens = null;

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal(campo, instance.campo);
            Assert.Null(instance.mensagens);
        }

        [Fact]
        public void CanHandleEmptyStrings()
        {
            // Arrange
            var campo = "";
            var mensagens = "";

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal(string.Empty, instance.campo);
            Assert.Equal(string.Empty, instance.mensagens);
        }

        [Fact]
        public void CanHandleWhitespaceStrings()
        {
            // Arrange
            var campo = "   ";
            var mensagens = "\t\n";

            // Act
            var instance = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal("   ", instance.campo);
            Assert.Equal("\t\n", instance.mensagens);
        }

        [Fact]
        public void RecordEqualityWorks()
        {
            // Arrange
            var campo = "testField";
            var mensagens = "Test error message";
            var instance1 = new ErrorDetails(campo, mensagens);
            var instance2 = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal(instance1, instance2);
        }

        [Fact]
        public void RecordInequalityWorks()
        {
            // Arrange
            var instance1 = new ErrorDetails("field1", "message1");
            var instance2 = new ErrorDetails("field2", "message2");

            // Assert
            Assert.NotEqual(instance1, instance2);
        }

        [Fact]
        public void GetHashCodeWorks()
        {
            // Arrange
            var campo = "testField";
            var mensagens = "Test error message";
            var instance1 = new ErrorDetails(campo, mensagens);
            var instance2 = new ErrorDetails(campo, mensagens);

            // Assert
            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
        }

        [Fact]
        public void ToStringWorks()
        {
            // Arrange
            var campo = "testField";
            var mensagens = "Test error message";
            var instance = new ErrorDetails(campo, mensagens);

            // Act
            var result = instance.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(campo, result);
            Assert.Contains(mensagens, result);
        }
    }

}
