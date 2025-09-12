using Domain.Core.Common.Base;
using Domain.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Base
{
    public class BaseErrorTest
    {
        private BaseError _testClass;
        private int _code;
        private string _mensagem;
        private EnumErrorType _type;
        private string _source;
        public BaseErrorTest()
        {
            _code = 0;
            _mensagem = "TestValue2071051154";
            _type = EnumErrorType.System;
            _source = "TestValue-2052877093";
            _testClass = new BaseError(_code, _mensagem, _type, _source);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new BaseError(_code, _mensagem, _type, _source);
            // Assert
            Assert.NotNull(instance);
        }
        [Fact]
        public void CodeIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_code, _testClass.code);
        }
        [Fact]
        public void MensagemIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_mensagem, _testClass.message);
        }
        [Fact]
        public void TypeIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_type, _testClass.type);
        }
        [Fact]
        public void SourceIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_source, _testClass.source);
        }
    }
}
