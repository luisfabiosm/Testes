using api_crud_template_tests.Fixtures;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Unit.Domain.Core.Models
{
    public class ErrorResponseTests
    {
        private ErrorResponse _testClass;
        public ErrorResponseTests()
        {
            _testClass  = new ErrorResponse();
        }

        [Fact]
        public void CanInitialize()
        {
            // Act
            var instance = new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "Detalhe do erro",
                Instance = "/api/resource/123",
                Errors = new Dictionary<string, string[]>
                {
                    { "Field1", new[] { "Error1", "Error2" } },
                    { "Field2", new[] { "Error3" } }
                }
            };

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanCreateBadRequest_ShouldCreateErrorResponseCorrectProperties()
        {
            // Arrange
            var detail = "Detalhe do erro";
            var errors = new Dictionary<string, string[]>
                {
                    { "Field1", new[] { "Error1", "Error2" } },
                    { "Field2", new[] { "Error3" } }
                };

            // Act
            var response = ErrorResponse.BadRequest(detail,errors);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be(400);
            response.Detail.Should().Be(detail);
            response.Title.Should().Be("Bad Request");

        }

        [Fact]
        public void CanCreateInternalServerError_ShouldCreateErrorResponseCorrectProperties()
        {
            // Arrange
            var detail = "Detalhe do erro";
    
            // Act
            var response = ErrorResponse.InternalServerError(detail);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be(500);
            response.Detail.Should().Be(detail);
            response.Title.Should().Be("Internal Server Error");

        }

        [Fact]
        public void CanCreateUnauthorizedError_ShouldCreateErrorResponseCorrectProperties()
        {
            // Arrange
            var detail = "Token de autenticação inválido ou ausente";

            // Act
            var response = ErrorResponse.Unauthorized(detail);

            // Assert
            response.Should().NotBeNull();
            response.Status.Should().Be(401);
            response.Detail.Should().Be(detail);
            response.Title.Should().Be("Unauthorized");

        }

    }
}
