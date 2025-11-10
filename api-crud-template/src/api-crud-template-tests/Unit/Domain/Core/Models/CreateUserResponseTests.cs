using api_crud_template_tests.Fixtures;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Unit.Domain.Core.Models
{
    public class CreateUserResponseTests
    {
        private CreateUserResponse _testClass;

        public CreateUserResponseTests()
        {
            _testClass = TestUserFixtures.CreateValidUserResponse();
        }

        [Fact]
        public void CanInitialize()
        {
            // Act
            var instance = TestUserFixtures.CreateValidUserResponse();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanCreateWithValidData_ShouldCreateUserResponsetWithCorrectProperties()
        {
            // Arrange
            var user = TestUserFixtures.CreateValidUser;

            // Act
            var response = TestUserFixtures.CreateValidUserResponse(user);

            // Assert
            response.Should().NotBeNull();
            response.CPF.Should().Be(user.CPF);
            response.Nome.Should().Be(user.Nome);
            response.Email.Should().Be(user.Email);
            response.Login.Should().Be(user.Login);
    
        }



    }
}
