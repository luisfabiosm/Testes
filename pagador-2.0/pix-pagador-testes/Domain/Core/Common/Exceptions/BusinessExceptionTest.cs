using Domain.Core.Enum;
using Domain.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Exceptions
{

    public class BusinessExceptionTest
    {
        private BusinessException _testClass;
        private string _mensagem;
        private int _codigo;
        private string _origem;

        public BusinessExceptionTest()
        {
            _mensagem = "Erro de negócio de teste";
            _codigo = 400;
            _origem = "API";
            _testClass = new BusinessException(_mensagem);
        }

        [Fact]
        public void CanConstructWithMessage()
        {
            // Act
            var instance = new BusinessException(_mensagem);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_mensagem, instance.Message);
        }

        [Fact]
        public void ErrorCodeIsInitializedToDefault()
        {
            // Assert
            Assert.Equal(400, _testClass.ErrorCode);
        }

        [Fact]
        public void BusinessErrorIsInitializedToNull()
        {
            // Assert
            Assert.Null(_testClass.BusinessError);
        }

        [Fact]
        public void MessageIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_mensagem, _testClass.Message);
        }

        [Fact]
        public void CanCreateWithMessageAndCode()
        {
            // Act
            var instance = BusinessException.Create(_mensagem, _codigo, _origem);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_mensagem, instance.Message);
            Assert.Equal(400, instance.ErrorCode); // Default value
            Assert.NotNull(instance.BusinessError);
            Assert.Equal(_codigo, instance.BusinessError.codErro);
            Assert.Equal(_mensagem, instance.BusinessError.msgErro);
            Assert.Equal(_origem, instance.BusinessError.origemErro);
            Assert.Equal((int)EnumTipoErro.NEGOCIO, instance.BusinessError.tipoErro);
        }

        [Fact]
        public void CanCreateWithDefaultOrigin()
        {
            // Act
            var instance = BusinessException.Create(_mensagem, _codigo);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_mensagem, instance.Message);
            Assert.NotNull(instance.BusinessError);
            Assert.Equal(_codigo, instance.BusinessError.codErro);
            Assert.Equal(_mensagem, instance.BusinessError.msgErro);
            Assert.Equal("API", instance.BusinessError.origemErro);
            Assert.Equal((int)EnumTipoErro.NEGOCIO, instance.BusinessError.tipoErro);
        }

        [Fact]
        public void CanCreateWithSpsErroReturn()
        {
            // Arrange
            var spsError = SpsErroReturn.Create(1, 500, "Erro SPS", "SPS");

            // Act
            var instance = BusinessException.Create(spsError);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(spsError.msgErro, instance.Message);
            Assert.Equal(400, instance.ErrorCode); // Default value
            Assert.Equal(spsError, instance.BusinessError);
            Assert.Equal(spsError.codErro, instance.BusinessError.codErro);
            Assert.Equal(spsError.msgErro, instance.BusinessError.msgErro);
            Assert.Equal(spsError.origemErro, instance.BusinessError.origemErro);
            Assert.Equal(spsError.tipoErro, instance.BusinessError.tipoErro);
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
            var instance = new BusinessException(string.Empty);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(string.Empty, instance.Message);
        }

        [Fact]
        public void CreateWithNegativeErrorCode()
        {
            // Arrange
            var negativeCode = -100;

            // Act
            var instance = BusinessException.Create(_mensagem, negativeCode, _origem);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(negativeCode, instance.BusinessError.codErro);
        }

        [Fact]
        public void CreateWithZeroErrorCode()
        {
            // Arrange
            var zeroCode = 0;

            // Act
            var instance = BusinessException.Create(_mensagem, zeroCode, _origem);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(zeroCode, instance.BusinessError.codErro);
        }

        [Fact]
        public void CreateWithNullOrigin()
        {
            // Act
            var instance = BusinessException.Create(_mensagem, _codigo, null);

            // Assert
            Assert.NotNull(instance);
            Assert.Null(instance.BusinessError.origemErro);
        }

        [Fact]
        public void CreateWithEmptyOrigin()
        {
            // Act
            var instance = BusinessException.Create(_mensagem, _codigo, string.Empty);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(string.Empty, instance.BusinessError.origemErro);
        }

        [Fact]
        public void CreateWithNullSpsErroReturn()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => BusinessException.Create((SpsErroReturn)null));
        }

        [Fact]
        public void BusinessErrorContainsCorrectTipoErro()
        {
            // Act
            var instance = BusinessException.Create(_mensagem, _codigo, _origem);

            // Assert
            Assert.Equal((int)EnumTipoErro.NEGOCIO, instance.BusinessError.tipoErro);
        }
    }


}
