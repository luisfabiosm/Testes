using Domain.Core.Enums;
using System.Reflection.Metadata;

namespace Domain.Core.Models.Entities
{
    public sealed record User
    {
        public int CPF { get; private set; } 
        public string Nome { get; private set; } = string.Empty;
        public string Login { get; private set; }
        public EnumStatus Status { get; private set; }
        public DateTime Nascimento { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public DateTime? DataUltimaMovimentacao { get; private set; }

        public static User Novo(int numero, string nome, string email, string login, string password, DateTime nascimento)
        {
            return new User
            {
                CPF = numero,
                Nome = nome,
                Status = EnumStatus.Ativo,
                Email= email,
                Login = login,
                Password = password,
                Nascimento = nascimento,
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
