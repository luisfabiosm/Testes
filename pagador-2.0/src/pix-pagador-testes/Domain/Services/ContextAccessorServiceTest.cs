using Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Services
{
    public class ContextAccessorServiceTest
    {
        private ContextAccessorService _testClass;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<ClaimsPrincipal> _mockUser;
        private Mock<HttpRequest> _mockRequest;
        private Mock<IHeaderDictionary> _mockHeaders;

        public ContextAccessorServiceTest()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _mockRequest = new Mock<HttpRequest>();
            _mockHeaders = new Mock<IHeaderDictionary>();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);
            _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
            _mockRequest.Setup(x => x.Headers).Returns(_mockHeaders.Object);

            _testClass = new ContextAccessorService();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ContextAccessorService();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void GetCanalReturnsCorrectValue()
        {
            // Arrange
            var expectedCanal = (short)100;
            var claim = new Claim("Canal", expectedCanal.ToString());

            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act
            var result = _testClass.GetCanal(_mockHttpContext.Object);

            // Assert
            Assert.Equal(expectedCanal, result);
        }

        [Fact]
        public void GetCanalThrowsWhenClaimNotFound()
        {
            // Arrange
            _mockUser.Setup(x => x.FindFirst("Canal")).Returns((Claim)null);

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(
                () => _testClass.GetCanal(_mockHttpContext.Object));

            Assert.Contains("Claim obrigatória 'Canal' não encontrada", exception.Message);
        }

        [Fact]
        public void GetCanalThrowsWhenClaimIsEmpty()
        {
            // Arrange
            var claim = new Claim("Canal", "");
            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(
                () => _testClass.GetCanal(_mockHttpContext.Object));

            Assert.Contains("Claim obrigatória 'Canal' não encontrada", exception.Message);
        }

        [Fact]
        public void GetCanalThrowsWhenClaimIsWhitespace()
        {
            // Arrange
            var claim = new Claim("Canal", "   ");
            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(
                () => _testClass.GetCanal(_mockHttpContext.Object));

            Assert.Contains("Claim obrigatória 'Canal' não encontrada", exception.Message);
        }

        [Fact]
        public void GetCanalThrowsWhenClaimIsNotNumeric()
        {
            // Arrange
            var claim = new Claim("Canal", "not-a-number");
            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act & Assert
            var exception = Assert.Throws<FormatException>(
                () => _testClass.GetCanal(_mockHttpContext.Object));

            Assert.Contains("Claim 'Canal' inválida", exception.Message);
        }

        [Fact]
        public void GetCanalThrowsWhenClaimIsOutOfShortRange()
        {
            // Arrange
            var claim = new Claim("Canal", "99999"); // Exceeds short.MaxValue
            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act & Assert
            var exception = Assert.Throws<FormatException>(
                () => _testClass.GetCanal(_mockHttpContext.Object));

            Assert.Contains("Claim 'Canal' inválida", exception.Message);
        }

        [Fact]
        public void GetChaveIdempotenciaReturnsCorrectValue()
        {
            // Arrange
            var expectedChave = "idempotencia-key-123";
            var headers = new HeaderDictionary
            {
                { "Chave-Idempotencia", expectedChave }
            };

            _mockHeaders.Setup(x => x.TryGetValue("Chave-Idempotencia", out It.Ref<StringValues>.IsAny))
                .Returns((string key, out StringValues value) =>
                {
                    value = new StringValues(expectedChave);
                    return true;
                });

            // Act
            var result = _testClass.GetChaveIdempotencia(_mockHttpContext.Object);

            // Assert
            Assert.Equal(expectedChave, result);
        }

        [Fact]
        public void GetChaveIdempotenciaThrowsWhenHeaderNotFound()
        {
            // Arrange
            _mockHeaders.Setup(x => x.TryGetValue("Chave-Idempotencia", out It.Ref<StringValues>.IsAny))
                .Returns(false);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _testClass.GetChaveIdempotencia(_mockHttpContext.Object));

            Assert.Contains("Cabeçalho obrigatório 'Chave-Idempotencia' não encontrado ou vazio", exception.Message);
        }

        [Fact]
        public void GetChaveIdempotenciaThrowsWhenHeaderIsEmpty()
        {
            // Arrange
            _mockHeaders.Setup(x => x.TryGetValue("Chave-Idempotencia", out It.Ref<StringValues>.IsAny))
                .Returns((string key, out StringValues value) =>
                {
                    value = new StringValues("");
                    return true;
                });

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _testClass.GetChaveIdempotencia(_mockHttpContext.Object));

            Assert.Contains("Cabeçalho obrigatório 'Chave-Idempotencia' não encontrado ou vazio", exception.Message);
        }

        [Fact]
        public void GetChaveIdempotenciaThrowsWhenHeaderIsWhitespace()
        {
            // Arrange
            _mockHeaders.Setup(x => x.TryGetValue("Chave-Idempotencia", out It.Ref<StringValues>.IsAny))
                .Returns((string key, out StringValues value) =>
                {
                    value = new StringValues("   ");
                    return true;
                });

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => _testClass.GetChaveIdempotencia(_mockHttpContext.Object));

            Assert.Contains("Cabeçalho obrigatório 'Chave-Idempotencia' não encontrado ou vazio", exception.Message);
        }

        [Fact]
        public void GetCanalHandlesMinimumShortValue()
        {
            // Arrange
            var expectedCanal = short.MinValue;
            var claim = new Claim("Canal", expectedCanal.ToString());

            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act
            var result = _testClass.GetCanal(_mockHttpContext.Object);

            // Assert
            Assert.Equal(expectedCanal, result);
        }

        [Fact]
        public void GetCanalHandlesMaximumShortValue()
        {
            // Arrange
            var expectedCanal = short.MaxValue;
            var claim = new Claim("Canal", expectedCanal.ToString());

            _mockUser.Setup(x => x.FindFirst("Canal")).Returns(claim);

            // Act
            var result = _testClass.GetCanal(_mockHttpContext.Object);

            // Assert
            Assert.Equal(expectedCanal, result);
        }

        [Fact]
        public void GetChaveIdempotenciaHandlesSpecialCharacters()
        {
            // Arrange
            var expectedChave = "key-with-special-chars_123!@#";
            _mockHeaders.Setup(x => x.TryGetValue("Chave-Idempotencia", out It.Ref<StringValues>.IsAny))
                .Returns((string key, out StringValues value) =>
                {
                    value = new StringValues(expectedChave);
                    return true;
                });

            // Act
            var result = _testClass.GetChaveIdempotencia(_mockHttpContext.Object);

            // Assert
            Assert.Equal(expectedChave, result);
        }
    }

}
