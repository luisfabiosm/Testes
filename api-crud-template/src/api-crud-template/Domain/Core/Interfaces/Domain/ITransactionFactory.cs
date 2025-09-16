using Domain.Core.Models.Request;
using Domain.UseCases.AddUser;

namespace Domain.Core.Interfaces.Domain
{
    public interface ITransactionFactory
    {
        TransactionAddUser CreateAddUserTransaction(HttpContext context, AddNewUserRequest request, string correlationId);

    }
}
