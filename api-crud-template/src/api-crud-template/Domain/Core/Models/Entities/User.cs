using Domain.Core.Enums;
using System.Reflection.Metadata;

namespace Domain.Core.Models.Entities
{
    public sealed record User
    {
        public Guid Id { get; private set; }
        public string CPF { get; private set; } = string.Empty;     
        public string Nome { get; private set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public EnumStatus Status { get; private set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public DateTime DataUltimaMovimentacao { get; private set; }

        public User()
        {
            
        }

        public static User Novo(string cpf, string nome, string email, string login, string password)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                CPF = cpf,
                Nome = nome,
                Status = EnumStatus.Ativo,
                Email= email,
                Login = login,
                Password = password,
                DataUltimaMovimentacao = DateTime.UtcNow

            };
        }

        public bool Habilitado() => (Status == EnumStatus.Ativo) ? true: false;

        public void Inativar()
        {
            Status = EnumStatus.Inativo;
            DataUltimaMovimentacao = DateTime.UtcNow;
        }

        public void Ativar()
        {
            Status = EnumStatus.Ativo;
            DataUltimaMovimentacao = DateTime.UtcNow;
        }

    }

}
