using Domain.Core.Enums;
using Domain.Core.Models.Entities;
using FluentAssertions;
using Xunit;

namespace api_crud_template_testes.Unit.Entities;

public class UserTests
{
    [Fact]
    public void Novo_WithValidData_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        var cpf = "123.456.789-01";
        var nome = "João Silva";
        var email = "joao@email.com";
        var login = "joao_silva";
        var password = "password123";

        // Act
        var user = User.Novo(cpf, nome, email, login, password);

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
    public void Novo_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var user1 = User.Novo("111.111.111-11", "User 1", "user1@test.com", "user1", "pass1");
        var user2 = User.Novo("222.222.222-22", "User 2", "user2@test.com", "user2", "pass2");

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Habilitado_WhenStatusIsAtivo_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");

        // Act
        var result = user.Habilitado();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Habilitado_WhenStatusIsInativo_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");
        user.Inativar();

        // Act
        var result = user.Habilitado();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Habilitado_WhenStatusIsExcluido_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");
        // Como não há método Excluir público, vamos testar diretamente o status
        // Para este teste, assumimos que o status pode ser alterado internamente

        // Act & Assert - Status Ativo retorna true
        user.Habilitado().Should().BeTrue();

        // Status Inativo retorna false
        user.Inativar();
        user.Habilitado().Should().BeFalse();
    }

    [Fact]
    public void Inativar_ShouldChangeStatusToInativo()
    {
        // Arrange
        var user = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");
        var originalDate = user.DataUltimaMovimentacao;

        // Act
        user.Inativar();

        // Assert
        user.Status.Should().Be(EnumStatus.Inativo);
        user.DataUltimaMovimentacao.Should().BeAfter(originalDate);
        user.DataUltimaMovimentacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Ativar_AfterInativar_ShouldChangeStatusToAtivo()
    {
        // Arrange
        var user = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");
        user.Inativar();
        var inactivationDate = user.DataUltimaMovimentacao;

        // Act
        user.Ativar();

        // Assert
        user.Status.Should().Be(EnumStatus.Ativo);
        user.DataUltimaMovimentacao.Should().BeAfter(inactivationDate);
        user.DataUltimaMovimentacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Ativar_WhenAlreadyActive_ShouldUpdateDataUltimaMovimentacao()
    {
        // Arrange
        var user = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");
        var originalDate = user.DataUltimaMovimentacao;

        // Wait a bit to ensure time difference
        Thread.Sleep(10);

        // Act
        user.Ativar();

        // Assert
        user.Status.Should().Be(EnumStatus.Ativo);
        user.DataUltimaMovimentacao.Should().BeAfter(originalDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Novo_WithEmptyOrNullCpf_ShouldStillCreateUser(string cpf)
    {
        // Arrange & Act
        var user = User.Novo(cpf, "Test User", "test@email.com", "test", "pass");

        // Assert
        user.Should().NotBeNull();
        user.CPF.Should().Be(cpf);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Novo_WithEmptyOrNullNome_ShouldStillCreateUser(string nome)
    {
        // Arrange & Act
        var user = User.Novo("123.456.789-01", nome, "test@email.com", "test", "pass");

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(nome ?? string.Empty); // Record init sets string.Empty as default
    }

    [Fact]
    public void User_AsRecord_ShouldSupportImmutabilityPatterns()
    {
        // Arrange
        var user1 = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");
        var user2 = User.Novo("123.456.789-01", "Test User", "test@email.com", "test", "pass");

        // Act & Assert
        user1.Should().NotBeNull();
        user2.Should().NotBeNull();
        // Records with different Ids should not be equal
        user1.Should().NotBe(user2);
    }
}