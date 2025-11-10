using Domain.Core.Models.Entities;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Fixtures
{
    public static class TestUserFixtures
    {
        public static string cpf = "12345678901";
        public static string nome = "João Silva";
        public static string email = "joao.silva@email.com";
        public static string login = "joao_silva";
        public static string password = "MinhaSenh@123!";
        public static DateTime nascimento = new DateTime(1990, 1, 1);

        public static CreateUserRequest CreateValidUserRequest => new()
        {
            CPF = "12345678901",
            Nome = "João Silva",
            Nascimento = new DateTime(1990, 1, 1),
            Email = "joao.silva@email.com",
            Login = "joao_silva",
            Password = "MinhaSenh@123!"
            
        };

        public static CreateUserRequest CreateNewUserRequest(string cpf, string nome, DateTime nascimento,  string email, string login, string password)
        {
            return new CreateUserRequest(cpf, nome, nascimento, email, login, password);
        }

        public static User CreateDefaultUser => new User();

        public static User CreateNewUser(string cpf, string nome, string email, string login, string password)
        {
            return User.Novo(cpf, nome, email, login, password);
        }

        public static User CreateValidUser => User.Novo(
             cpf,
             nome,
             email,
             login,
             password
         );


        public static CreateUserResponse CreateValidUserResponse()
        {
            return CreateUserResponse.Create(CreateValidUser);
        }

        public static CreateUserResponse CreateValidUserResponse(User user)
        {
            return CreateUserResponse.Create(user);
        }

    }


}
