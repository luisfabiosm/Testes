using Domain.Core.Common.Base;
using Domain.Core.Exceptions;
using Domain.Core.Ports.Outbound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;
namespace pix_pagador_testes.Domain.Core.Common.Base
{
    public class BaseServiceTest
    {
        private TestableBaseService _testClass;
        private Mock<ILoggingAdapter> _mockLoggingAdapter;
        private Mock<IServiceProvider> _mockServiceProvider;

        public BaseServiceTest()
        {
            _mockLoggingAdapter = new Mock<ILoggingAdapter>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
            _testClass = new TestableBaseService(_mockServiceProvider.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TestableBaseService(_mockServiceProvider.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullServiceProvider()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestableBaseService(null));
        }

        [Fact]
        public void HandleErrorWithBusinessException()
        {
            // Arrange
            var businessException = BusinessException.Create("Business error", 400);
            var methodName = "TestMethod";

            // Act
            var result = _testClass.TestHandleError(businessException, methodName);

            // Assert
            Assert.IsType<BusinessException>(result);
            Assert.Equal(businessException.Message, result.Message);
            _mockLoggingAdapter.Verify(
                la => la.LogError($"Erro em: {methodName} - {businessException.Message}", businessException),
                Times.Once);
        }

        [Fact]
        public void HandleErrorWithInternalException()
        {
            // Arrange
            var internalException = new InternalException("Internal error");
            var methodName = "TestMethod";

            // Act
            var result = _testClass.TestHandleError(internalException, methodName);

            // Assert
            Assert.IsType<InternalException>(result);
            Assert.Equal(internalException.Message, result.Message);
            _mockLoggingAdapter.Verify(
                la => la.LogError($"Erro em: {methodName} - {internalException.Message}", internalException),
                Times.Once);
        }

        [Fact]
        public void HandleErrorWithUnknownException()
        {
            // Arrange
            var unknownException = new ArgumentException("Unknown error");
            var methodName = "TestMethod";

            // Act
            var result = _testClass.TestHandleError(unknownException, methodName);

            // Assert
            Assert.IsType<InternalException>(result);
            Assert.Equal(unknownException.Message, result.Message);
            _mockLoggingAdapter.Verify(
                la => la.LogError($"Erro em: {methodName} - {unknownException.Message}", unknownException),
                Times.Once);
        }

        [Fact]
        public void HandleExceptionWithBusinessException()
        {
            // Arrange
            var businessException = BusinessException.Create("Business error", 400);
            var methodName = "TestMethod";

            // Act
            var (operation, ex) = _testClass.TestHandleException(methodName, businessException);

            // Assert
            Assert.Equal(methodName, operation);
            Assert.IsType<BusinessException>(ex);
            Assert.Equal(businessException.Message, ex.Message);
            _mockLoggingAdapter.Verify(
                la => la.LogError($"Erro em: {methodName} - {businessException.Message}", businessException),
                Times.Once);
        }

        [Fact]
        public void HandleExceptionWithUnknownException()
        {
            // Arrange
            var unknownException = new ArgumentException("Unknown error");
            var methodName = "TestMethod";

            // Act
            var (operation, ex) = _testClass.TestHandleException(methodName, unknownException);

            // Assert
            Assert.Equal(methodName, operation);
            Assert.IsType<InternalException>(ex);
            Assert.Equal(unknownException.Message, ex.Message);
            _mockLoggingAdapter.Verify(
                la => la.LogError($"Erro em: {methodName} - {unknownException.Message}", unknownException),
                Times.Once);
        }

        [Fact]
        public void LogErrorCallsLoggingAdapter()
        {
            // Arrange
            var methodName = "TestMethod";
            var exception = new Exception("Test exception");

            // Act
            _testClass.TestLogError(methodName, exception);

            // Assert
            _mockLoggingAdapter.Verify(
                la => la.LogError($"Erro em: {methodName} - {exception.Message}", exception),
                Times.Once);
        }

        // Testable implementation to access protected methods
        private class TestableBaseService : BaseService
        {
            public TestableBaseService(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            public Exception TestHandleError(Exception exception, string methodName)
            {
                return HandleError(exception, methodName);
            }

            public (string operation, Exception ex) TestHandleException(string methodName, Exception exception)
            {
                return HandleException(methodName, exception);
            }

            public void TestLogError(string methodName, Exception exception)
            {
                LogError(methodName, exception);
            }
        }
    }
}
