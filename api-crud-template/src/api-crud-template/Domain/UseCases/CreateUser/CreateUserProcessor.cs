using Domain.Core.Interfaces.Outbound;
using Domain.Core.Models.Entities;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.ResultPattern;
using System.Diagnostics;

namespace Domain.UseCases.CreateUser
{
    public class CreateUserProcessor
    {
        private readonly IUserRepository _userRepository;
        private readonly CreateUserRequestValidator _validator;
        private readonly ILogger<CreateUserProcessor> _logger;

        public CreateUserProcessor(IServiceProvider serviceProvider)
        {
            _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            _validator = serviceProvider.GetRequiredService<CreateUserRequestValidator>();
            _logger = serviceProvider.GetRequiredService< ILogger <CreateUserProcessor>> ();
        }

        public async Task<Result<CreateUserResponse>> ProcessAsync(TransactionCreateUser transaction, CancellationToken cancellationToken = default)
        {
            using var activity = Activity.Current?.Source.StartActivity($"{nameof(CreateUserProcessor)}.{nameof(ProcessAsync)}");

            try
            {
                // 1. Validar request
                var validationResult = await _validator.ValidateAsync(transaction, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(e => e.Message));
                    _logger.LogWarning("Dados inválidos para criação de usuário: {Errors}", errors);
                    return Result.Failure<CreateUserResponse>($"Dados inválidos: {errors}");
                }

                // 2. Verificar se email já existe
                var existingUserResult = await _userRepository.ExistsByEmailAsync(transaction.NewUser.Email, cancellationToken);
                if (existingUserResult.IsFailure)
                {
                    return Result.Failure<CreateUserResponse>($"Erro ao verificar email existente: {existingUserResult.Error}");
                }

                if (existingUserResult.Value)
                {
                    _logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", transaction.NewUser.Email);
                    return Result.Failure<CreateUserResponse>("Email já está em uso");
                }

                // 3. Salvar no repositório
                var createResult = await _userRepository.CreateAsync(transaction, cancellationToken);
                if (createResult.IsFailure)
                {
                    return Result.Failure<CreateUserResponse>($"Erro ao salvar usuário: {createResult.Error}");
                }

                // 4. Criar response
                var response = CreateUserResponse.Create(transaction.NewUser);   
          

                activity?.SetTag("user.created", true);
                _logger.LogInformation("Usuário processado com sucesso no processador. ID: {UserId}", response.Id);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no processador de criação de usuário");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Result.Failure<CreateUserResponse>($"Erro interno no processador: {ex.Message}");
            }
        }
    }
}
