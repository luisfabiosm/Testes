using Domain.Core.Models.Entities;

namespace Domain.Core.Models.Request
{
    public struct CreateUserRequest
    {
        public string CPF { get; set; }
        public string Nome { get; set; }
        public DateTime Nascimento { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public CreateUserRequest(string cpf, string nome, DateTime nascimento, string email, string login, string password)
        {
            CPF = cpf.Replace(".", "").Replace("-", "");    
            Nome = nome.Trim(); 
            Nascimento = nascimento;
            Email = email.Trim().ToLower();
            Login = login;
            Password = password;
        }

        public User ToUser()
        {
            return User.Novo(CPF, Nome.ToString(), Email, Login, Password);
        }   

    }
}
