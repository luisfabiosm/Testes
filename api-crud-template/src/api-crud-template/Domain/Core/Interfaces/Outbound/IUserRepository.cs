using Domain.Core.Models.Entities;
using Domain.Core.SharedKernel.ResultPattern;
using Domain.UseCases.CreateUser;
using System.Transactions;

namespace Domain.Core.Interfaces.Outbound
{
    public interface IUserRepository
    {
        Task<Result<Guid>> CreateAsync(TransactionCreateUser user, CancellationToken cancellationToken = default);
        Task<Result<Guid>> ExecStoredProcedureCreateAsync(TransactionCreateUser transaction, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    }

}
