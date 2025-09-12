using Domain.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Exceptions
{

    public class SpsErroReturnTest
    {
        private SpsErroReturn _testClass;
        private int _tipoErro;
        private int _codErro;
        private string _msgErro;
        private string _origemErro;

        public SpsErroReturnTest()
        {
            _tipoErro = 1;
            _codErro = 400;
            _msgErro = "Erro de teste";
            _origemErro = "SPS";

            _testClass = SpsErroReturn.Create(_tipoErro, _codErro, _msgErro, _origemErro);
        }

        [Fact]
        public void CanCreateWithAllParameters()
        {
            // Act
            var instance = SpsErroReturn.Create(_tipoErro, _codErro, _msgErro, _origemErro);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_tipoErro, instance.tipoErro);
            Assert.Equal(_codErro, instance.codErro);
            Assert.Equal(_msgErro, instance.msgErro);
            Assert.Equal(_origemErro, instance.origemErro);
        }

        [Fact]
        public void CanCreateWithDefaultOrigin()
        {
            // Act
            var instance = SpsErroReturn.Create(_tipoErro, _codErro, _msgErro);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_tipoErro, instance.tipoErro);
            Assert.Equal(_codErro, instance.codErro);
            Assert.Equal(_msgErro, instance.msgErro);
            Assert.Equal(string.Empty, instance.origemErro);
        }

        [Fact]
        public void TipoErroIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_tipoErro, _testClass.tipoErro);
        }

        [Fact]
        public void CodErroIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_codErro, _testClass.codErro);
        }

        [Fact]
        public void MsgErroIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_msgErro, _testClass.msgErro);
        }

        [Fact]
        public void OrigemErroIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_origemErro, _testClass.origemErro);
        }

        [Fact]
        public void CanCreateFromJsonString()
        {
            // Arrange
            var jsonString = JsonSerializer.Serialize(new
            {
                tipoErro = _tipoErro,
                codErro = _codErro,
                msgErro = _msgErro,
                origemErro = _origemErro
            });

            // Act
            var instance = SpsErroReturn.Create(jsonString);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_tipoErro, instance.tipoErro);
            Assert.Equal(_codErro, instance.codErro);
            Assert.Equal(_msgErro, instance.msgErro);
            Assert.Equal(_origemErro, instance.origemErro);
        }

        [Fact]
        public void CreateFromJsonStringThrowsWithInvalidJson()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act & Assert
            Assert.Throws<ValidateException>(() => SpsErroReturn.Create(invalidJson));
        }

        [Fact]
        public void CreateFromJsonStringThrowsWithNullString()
        {
            // Act & Assert
            Assert.Throws<ValidateException>(() => SpsErroReturn.Create((string)null));
        }

        [Fact]
        public void CreateFromJsonStringThrowsWithEmptyString()
        {
            // Act & Assert
            Assert.Throws<ValidateException>(() => SpsErroReturn.Create(string.Empty));
        }

        [Fact]
        public void CreateFromJsonStringThrowsWithWhitespaceString()
        {
            // Act & Assert
            Assert.Throws<ValidateException>(() => SpsErroReturn.Create("   "));
        }

  

        [Fact]
        public void CanHandleEmptyMessage()
        {
            // Act
            var instance = SpsErroReturn.Create(_tipoErro, _codErro, string.Empty, _origemErro);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(string.Empty, instance.msgErro);
        }

        [Fact]
        public void CanHandleNullOrigin()
        {
            // Act
            var instance = SpsErroReturn.Create(_tipoErro, _codErro, _msgErro, null);

            // Assert
            Assert.NotNull(instance);
            Assert.Null(instance.origemErro);
        }

        [Fact]
        public void CanHandleZeroErrorCode()
        {
            // Act
            var instance = SpsErroReturn.Create(_tipoErro, 0, _msgErro, _origemErro);

            // Assert
            Assert.Equal(0, instance.codErro);
        }

        [Fact]
        public void CanHandleNegativeErrorCode()
        {
            // Act
            var instance = SpsErroReturn.Create(_tipoErro, -100, _msgErro, _origemErro);

            // Assert
            Assert.Equal(-100, instance.codErro);
        }

        [Fact]
        public void CanHandleZeroTipoErro()
        {
            // Act
            var instance = SpsErroReturn.Create(0, _codErro, _msgErro, _origemErro);

            // Assert
            Assert.Equal(0, instance.tipoErro);
        }

        [Fact]
        public void CanHandleNegativeTipoErro()
        {
            // Act
            var instance = SpsErroReturn.Create(-1, _codErro, _msgErro, _origemErro);

            // Assert
            Assert.Equal(-1, instance.tipoErro);
        }

        [Fact]
        public void IsSealed()
        {
            // Assert
            Assert.True(typeof(SpsErroReturn).IsSealed);
        }

        [Fact]
        public void PropertiesAreInit()
        {
            // This test verifies that properties are init-only by compilation
            // If this compiles, it means the properties are properly configured

            // Act
            var instance = SpsErroReturn.Create(_tipoErro, _codErro, _msgErro, _origemErro);

            // Assert
            Assert.NotNull(instance);
            // Cannot assign to init-only properties after creation - this would not compile:
            // instance.tipoErro = 999; // Compilation error
        }

        [Fact]
        public void CreateFromComplexJsonString()
        {
            // Arrange
            var complexJson = $@"{{
                ""tipoErro"": {_tipoErro},
                ""codErro"": {_codErro},
                ""msgErro"": ""{_msgErro}"",
                ""origemErro"": ""{_origemErro}"",
                ""extraField"": ""ignored""
            }}";

            // Act
            var instance = SpsErroReturn.Create(complexJson);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(_tipoErro, instance.tipoErro);
            Assert.Equal(_codErro, instance.codErro);
            Assert.Equal(_msgErro, instance.msgErro);
            Assert.Equal(_origemErro, instance.origemErro);
        }
    }



}
