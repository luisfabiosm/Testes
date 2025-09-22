using Domain.Core.Interfaces.Domain;
using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.ResultPattern;
using System.Diagnostics;

namespace Domain.UseCases.CreateUser
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly CreateUserProcessor _processor;
        private readonly ILogger<CreateUserUseCase> _logger;

        public CreateUserUseCase(IServiceProvider serviceProvider)
        {   
            _processor = serviceProvider.GetRequiredService<CreateUserProcessor>(); 
            _logger = serviceProvider.GetRequiredService<ILogger<CreateUserUseCase>>() ;
        }

        public async Task<Result<CreateUserResponse>> ExecuteAsync(TransactionCreateUser transaction, CancellationToken cancellationToken = default)
        {
            using var activity = Activity.Current?.Source.StartActivity($"{nameof(CreateUserUseCase)}.{nameof(ExecuteAsync)}");

            try
            {
                _logger.LogInformation("Iniciando criação de usuário para email: {Email}", transaction.NewUser.Email);

                activity?.SetTag("user.email", transaction.NewUser.Email);
                activity?.SetTag("use_case", nameof(CreateUserUseCase));

                // 1. Executar o processador
                var result = await _processor.ProcessAsync(transaction, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Usuário criado com sucesso. ID: {UserId}", result.Value.Id);
                    activity?.SetTag("user.id", result.Value.Id.ToString());
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    _logger.LogWarning("Falha na criação do usuário: {Error}", result.Error);
                    activity?.SetStatus(ActivityStatusCode.Error, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar usuário");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Result.Failure<CreateUserResponse>($"Erro interno: {ex.Message}");
            }
        }
    }
}
