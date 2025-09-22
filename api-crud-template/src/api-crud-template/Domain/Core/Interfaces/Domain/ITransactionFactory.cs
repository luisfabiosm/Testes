using Domain.Core.Models.Request;
using Domain.UseCases.CreateUser;

namespace Domain.Core.Interfaces.Domain
{
    public interface ITransactionFactory
    {
        TransactionCreateUser CreateUserTransaction(HttpContext context, CreateUserRequest request, string correlationId);

    }
}
