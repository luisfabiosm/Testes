using api_crud_template_tests.Fixtures;
using Domain.Core.Enums;
using Domain.Core.Models.Entities;
using Domain.Core.Models.Request;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Unit.Domain.Core.Models
{
    public class CreateUserRequestTests
    {
        private CreateUserRequest _testClass;

        public CreateUserRequestTests()
        {
            _testClass = TestUserFixtures.CreateValidUserRequest;
        }

        [Fact]
        public void CanInitialize()
        {
            // Act
            var instance = TestUserFixtures.CreateValidUserRequest;

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanCreateWithValidData_ShouldCreateUserRequestWithCorrectProperties()
        {
            // Arrange
            var cpf = "12345678901";
            var nome = "João Silva";
            var nascimento = new DateTime(1990, 1, 1);
            var email = "joao@email.com";
            var login = "joao_silva";
            var password = "password123";

            // Act
            var request = TestUserFixtures.CreateNewUserRequest(cpf, nome, nascimento, email, login, password);

            // Assert
            request.Should().NotBeNull();
            request.CPF.Should().Be(cpf);
            request.Nome.Should().Be(nome);
            request.Email.Should().Be(email);
            request.Login.Should().Be(login);
            request.Password.Should().Be(password);
            request.Nascimento.Should().Be(nascimento);
        }

        [Fact]
        public void CanSetAndGetCPF()
        {
            // Arrange
            var testValue = TestUserFixtures.cpf;

            // Act
            _testClass.CPF = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.CPF);
        }

        [Fact]
        public void CanSetAndGetNome()
        {
            // Arrange
            var testValue = TestUserFixtures.nome;

            // Act
            _testClass.Nome = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Nome);
        }

        [Fact]
        public void CanSetAndGetLogin()
        {
            // Arrange
            var testValue = TestUserFixtures.login;

            // Act
            _testClass.Login = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Login);
        }

        [Fact]
        public void CanSetAndGetPassword()
        {
            // Arrange
            var testValue = TestUserFixtures.password;

            // Act
            _testClass.Password = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Password);
        }

        [Fact]
        public void CanSetAndGetEmail()
        {
            // Arrange
            var testValue = TestUserFixtures.email;

            // Act
            _testClass.Email = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Email);
        }

        [Fact]
        public void CanSetAndGetNascimento()
        {
            // Arrange
            var testValue = TestUserFixtures.nascimento;

            // Act
            _testClass.Nascimento = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Nascimento);
        }


    }
}
