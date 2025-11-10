using Domain.Core.Models.Entities;
using Domain.Core.Models.Request;
using Domain.UseCases.CreateUser;

namespace api_crud_template_testes.Fixtures;

public static class TestFixtures
{
    public static class Users
    {
        public static string cpf = "";
        public static string nome = "";
        public static string email = "";
        public static string login = "";
        public static string password = "";

        public static CreateUserRequest ValidCreateUserRequest => new()
        {
            CPF = "123.456.789-01",
            Nome = "João Silva",
            Nascimento = new DateTime(1990, 1, 1),
            Email = "joao.silva@email.com",
            Login = "joao_silva",
            Password = "MinhaSenh@123!"
        };

        public static CreateUserRequest ValidCreateUserRequestLongName => new()
        {
            CPF = "123.456.789-01",
            Nome = new string('A', 101),
            Nascimento = new DateTime(1990, 1, 1),
            Email = "joao.silva@email.com",
            Login = "joao_silva",
            Password = "MinhaSenh@123!"
        };

        public static CreateUserRequest InvalidCreateUserRequest => new()
        {
            CPF = "", // Invalid CPF
            Nome = "J", // Too short
            Nascimento = new DateTime(1990, 1, 1),
            Email = "invalid-email", // Invalid format
            Login = "",
            Password = ""
        };

        public static User ValidUser => User.Novo(
            "123.456.789-01",
            "João Silva",
            "joao.silva@email.com",
            "joao_silva",
            "MinhaSenh@123!"
        );


        public static User UserParameters => User.Novo(cpf, nome, email, login, password );


        public static User InvalidUserLongName => User.Novo(
            "123.456.789-01",
           new string('A', 101),
            "joao.silva@email.com",
            "joao_silva",
            "MinhaSenh@123!"
        );

        public static User InvalidUser => new()
        {
            Password = ""
        };

        public static User InvalidUserCPFInvalido => User.Novo(
          "12345678901",
          "João Silva",
          "joao.silva@email.com",
          "joao_silva",
          "MinhaSenh@123!"
      );


        public static TransactionCreateUser ValidTransaction => new()
        {
            Code = 1,
            CorrelationId = Guid.NewGuid().ToString(),
            canal = 1,
            chaveIdempotencia = "test-key",
            NewUser = ValidUser
        };
        public static TransactionCreateUser ValidTransactionUserParameters => new()
        {
            Code = 1,
            CorrelationId = Guid.NewGuid().ToString(),
            canal = 1,
            chaveIdempotencia = "test-key",
            NewUser = UserParameters
        };

        public static TransactionCreateUser InvalidTransactionEmptyUser => new()
        {
            Code = 1,
            CorrelationId = Guid.NewGuid().ToString(),
            canal = 1,
            chaveIdempotencia = "test-key",
            NewUser = InvalidUser
        };
        public static TransactionCreateUser InvalidTransactionUserLomgName => new()
        {
            Code = 1,
            CorrelationId = Guid.NewGuid().ToString(),
            canal = 1,
            chaveIdempotencia = "test-key",
            NewUser = InvalidUserLongName
        };

        public static TransactionCreateUser InvalidTransactionMultipleErros => new()
        {
            Code = 1,
            CorrelationId = Guid.NewGuid().ToString(),
            canal = 1,
            chaveIdempotencia = "test-key",
            NewUser = InvalidUser
        };

        public static TransactionCreateUser InvalidTransactionCPFInvalido => new()
        {
            Code = 1,
            CorrelationId = Guid.NewGuid().ToString(),
            canal = 1,
            chaveIdempotencia = "test-key",
            NewUser = InvalidUserCPFInvalido
        };
    }

    public static class CorrelationIds
    {
        public static string Valid => Guid.NewGuid().ToString();
        public static string TestId => "test-correlation-id";
    }
}