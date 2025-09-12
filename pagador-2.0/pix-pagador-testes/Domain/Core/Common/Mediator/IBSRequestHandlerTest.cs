using Domain.Core.Common.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Mediator
{
    public class IBSRequestHandlerTest
    {
        [Fact]
        public void CanImplementInterface()
        {
            // Arrange & Act
            var instance = new TestHandler();

            // Assert
            Assert.IsAssignableFrom<IBSRequestHandler<TestRequest, string>>(instance);
        }

        [Fact]
        public void InterfaceIsGeneric()
        {
            // Arrange
            var stringHandler = new TestHandler();
            var intHandler = new TestIntHandler();

            // Assert
            Assert.IsAssignableFrom<IBSRequestHandler<TestRequest, string>>(stringHandler);
            Assert.IsAssignableFrom<IBSRequestHandler<TestIntRequest, int>>(intHandler);
        }

        [Fact]
        public async Task CanCallHandleMethod()
        {
            // Arrange
            var handler = new TestHandler();
            var request = new TestRequest();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal("Handled", result);
        }

        [Fact]
        public void CanMockInterface()
        {
            // Arrange
            var mockHandler = new Mock<IBSRequestHandler<TestRequest, string>>();
            var request = new TestRequest();
            var expectedResponse = "Mocked response";

            mockHandler
                .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = mockHandler.Object.Handle(request, CancellationToken.None).Result;

            // Assert
            Assert.Equal(expectedResponse, result);
            mockHandler.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void HasCorrectGenericConstraint()
        {
            // This test verifies the interface constraint at compile time
            // If this compiles, the constraint is working correctly

            // Arrange
            var handler = new TestHandler();
            var request = new TestRequest();

            // Act & Assert - Should compile without issues
            ProcessHandlerRequest(handler, request);
        }

        private async Task<TResponse> ProcessHandlerRequest<TRequest, TResponse>(
            IBSRequestHandler<TRequest, TResponse> handler,
            TRequest request)
            where TRequest : IBSRequest<TResponse>
        {
            return await handler.Handle(request, CancellationToken.None);
        }

        // Test implementations
        public class TestRequest : IBSRequest<string>
        {
        }

        public class TestIntRequest : IBSRequest<int>
        {
        }

        public class TestHandler : IBSRequestHandler<TestRequest, string>
        {
            public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult("Handled");
            }
        }

        public class TestIntHandler : IBSRequestHandler<TestIntRequest, int>
        {
            public Task<int> Handle(TestIntRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(42);
            }
        }
    }

}
