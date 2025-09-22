using Domain.Core.Enums;
using Domain.Core.Models.Entities;

namespace Domain.Core.Models.Response
{
    public sealed record CreateUserResponse
    {
        public Guid Id { get; private set; }
        public string  CPF { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; }

        public string Login { get; private set; } = string.Empty;
        public EnumStatus Status { get; private set; }
        public DateTime? DataUltimaMovimentacao { get; private set; }

        public CreateUserResponse()
        {
            
        }
        public static CreateUserResponse Create (User user)
        {
            return new CreateUserResponse
            {
                Id = user.Id,
                CPF = user.CPF,
                Nome = user.Nome,
                Email = user.Email,
                Login = user.Login,
                Status = user.Status,
                DataUltimaMovimentacao = user.DataUltimaMovimentacao,
            };
        }

    }
}
