using Domain.Core.Common.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Mediator
{
    public class IBSRequestTest
    {
        [Fact]
        public void CanImplementInterface()
        {
            // Arrange & Act
            var instance = new TestRequest();

            // Assert
            Assert.IsAssignableFrom<IBSRequest<string>>(instance);
        }

        [Fact]
        public void InterfaceIsGeneric()
        {
            // Arrange
            var stringRequest = new TestRequest();
            var intRequest = new TestIntRequest();

            // Assert
            Assert.IsAssignableFrom<IBSRequest<string>>(stringRequest);
            Assert.IsAssignableFrom<IBSRequest<int>>(intRequest);
        }

        [Fact]
        public void CanBeUsedAsConstraint()
        {
            // Arrange
            var request = new TestRequest();

            // Act & Assert - Should compile without issues
            ProcessRequest(request);
        }

        private T ProcessRequest<T>(IBSRequest<T> request)
        {
            return default(T);
        }

        // Test implementations
        private class TestRequest : IBSRequest<string>
        {
        }

        private class TestIntRequest : IBSRequest<int>
        {
        }
    }

}
