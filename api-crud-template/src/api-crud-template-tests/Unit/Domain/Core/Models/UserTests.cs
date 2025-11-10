using api_crud_template_tests.Fixtures;
using Domain.Core.Enums;
using Domain.Core.Models.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Unit.Domain.Core.Models
{
    public class UserTests
    {
        private readonly User _testClass;


        public UserTests()
        {
            _testClass = TestUserFixtures.CreateDefaultUser;
        }


        [Fact]
        public void CanInitialize()
        {
            // Act
            var instance = TestUserFixtures.CreateDefaultUser;

            // Assert
            Assert.NotNull(instance);
        }


        [Fact]
        public void CanCreateWithValidData_ShouldCreateUserWithCorrectProperties()
        {
            // Arrange
            var cpf = "12345678901";
            var nome = "João Silva";
            var email = "joao@email.com";
            var login = "joao_silva";
            var password = "password123";

            // Act
            var user = TestUserFixtures.CreateNewUser(cpf,nome,email, login, password);

            // Assert
            user.Should().NotBeNull();
            user.Id.Should().NotBe(Guid.Empty);
            user.CPF.Should().Be(cpf);
            user.Nome.Should().Be(nome);
            user.Email.Should().Be(email);
            user.Login.Should().Be(login);
            user.Password.Should().Be(password);
            user.Status.Should().Be(EnumStatus.Ativo);
            user.DataUltimaMovimentacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
        public void Ativar_AfterInativar_ShouldChangeStatusToAtivo()
        {
            // Arrange
            var user = TestUserFixtures.CreateValidUser;

            user.Inativar();
            var activationDate = user.DataUltimaMovimentacao;

            // Act
            user.Ativar();

            // Assert
            user.Status.Should().Be(EnumStatus.Ativo);
            user.DataUltimaMovimentacao.Should().BeAfter(activationDate);
            user.DataUltimaMovimentacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Inativar_AfterAtivar_ShouldChangeStatusToInativo()
        {
            // Arrange
            var user = TestUserFixtures.CreateValidUser;

            user.Inativar();
            var inactivationDate = user.DataUltimaMovimentacao;

            // Act
            user.Inativar();

            // Assert
            user.Status.Should().Be(EnumStatus.Inativo);
            user.DataUltimaMovimentacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

    }
}
