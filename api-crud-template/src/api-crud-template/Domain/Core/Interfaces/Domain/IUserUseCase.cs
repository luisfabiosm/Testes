
using Domain.Core.Models.Entities;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.ResultPattern;
using Domain.UseCases.CreateUser;

namespace Domain.Core.Interfaces.Domain
{
    public interface IUserUseCase<TRequest, TResponse>
    {
        Task<Result<TResponse>> ExecuteAsync(TRequest transaction, CancellationToken cancellationToken = default);
    }

    public interface ICreateUserUseCase : IUserUseCase<TransactionCreateUser, CreateUserResponse> { }

    //public interface IGetUserByIdUseCase : IUserUseCase<GetUserByIdRequest, GetUserByIdResponse?> { }
    //public interface IListUsersUseCase : IUserUseCase<ListUsersRequest, IEnumerable<User>> { }
    //public interface IUpdateUserInfoUseCase : IUserUseCase<UpdateUserInfoRequest, UpdateUserInfoResponse> { }
    //public interface IDeleteUserUseCase : IUserUseCase<DeleteUserRequest, Unit> { } // Unit = void result

}
