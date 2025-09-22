using Domain.Core.Interfaces.Domain;
using Domain.Core.SharedKernel.Transactions;

//using Domain.Core.Interfaces.Inbound;
using Domain.UseCases.CreateUser;
//using Domain.UseCases.DeleteUser;
//using Domain.UseCases.GetUserById;
//using Domain.UseCases.ListUsers;
//using Domain.UseCases.UpdateUserInfo;

namespace Configurations;

public static class DomainConfiguration
{
    public static IServiceCollection ConfigureDomain( this IServiceCollection services, IConfiguration configuration)
    {
        //Regsitrar Validadores
        services.AddScoped<CreateUserRequestValidator>(); // 
        
        //Domain Services and Shared Kernel
        services.AddTransient<ITransactionFactory, TransactionFactory>();


        // Registrar Use Cases
        services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
        //services.AddScoped<IUserUseCase, GetUserByIdUseCase>();
        //services.AddScoped<IUserUseCase, ListUsersUseCase>();
        //services.AddScoped<IUserUseCase, UpdateUserInfoUseCase>();
        //services.AddScoped<IUserUseCase, DeleteUserUseCase>();

        // Registrar Processadores
        services.AddScoped<CreateUserProcessor>();
        //services.AddScoped<GetUserByIdProcessor>();
        //services.AddScoped<ListUsersProcessor>();
        //services.AddScoped<UpdateUserInfoProcessor>();
        //services.AddScoped<DeleteUserProcessor>();

        return services;
    }
}