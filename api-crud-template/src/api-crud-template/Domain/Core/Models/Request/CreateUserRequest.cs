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


        public User ToUser()
        {
            return User.Novo(CPF, Nome.ToString(), Email, Login, Password);
        }   

    }
}
