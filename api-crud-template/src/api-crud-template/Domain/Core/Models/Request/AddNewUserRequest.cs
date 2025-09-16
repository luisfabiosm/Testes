using Domain.Core.Models.Entities;

namespace Domain.Core.Models.Request
{
    public struct AddNewUserRequest
    {
        public int CPF { get; set; }
        public int Nome { get; set; }
        public DateTime Nascimento { get; set; }
        public string Email { get; set; }
        public string LoginId { get; set; }
        public string Password { get; set; }


        public User ToUser()
        {
            return User.Novo(CPF, Nome.ToString(), Email, LoginId, Password, Nascimento);
        }   

    }
}
