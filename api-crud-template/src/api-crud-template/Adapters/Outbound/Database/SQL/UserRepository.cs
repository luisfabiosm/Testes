using Dapper;
using Domain.Core.Enums;
using Domain.Core.Interfaces.Outbound;
using Domain.Core.Settings;
using Domain.Core.SharedKernel;
using Domain.Core.SharedKernel.ResultPattern;
using Domain.UseCases.CreateUser;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Adapters.Outbound.Database.SQL;

public class UserRepository : IUserRepository
{
    protected ISQLConnectionAdapter _dbConnection;
    protected IDbConnection _session;
    private readonly ILogger<UserRepository> _logger; 
    protected readonly IOptions<DatabaseSettings> _dbsettings;

    internal string MockEnviromentName = "Mock";

    public UserRepository(IServiceProvider serviceProvider)
    {
        _dbConnection = serviceProvider.GetRequiredService<ISQLConnectionAdapter>();
        _dbsettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>();
        _logger = serviceProvider.GetRequiredService<ILogger<UserRepository>>(); ;
    }

    public async Task<Result<Guid>> CreateAsync(TransactionCreateUser transaction, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity($"{nameof(UserRepository)}.{nameof(CreateAsync)}");
        activity?.SetTag("user.email", transaction.NewUser.Email);
        try
        {

            if (Global.ENVIRONMENT == MockEnviromentName)
            {
                _logger.LogInformation("Ambiente Mock", transaction.NewUser.Email);
                return Result.Success(transaction.NewUser.Id); 
            }
            var _result = await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {
            
                var query = @"
                INSERT INTO Users (Id, Nome, Email, CPF, Login, Password, Status, DataUltimaMovimentacao)
                VALUES (@Id, @Nome, @Email, @CPF, @Login, @Password, @Status, @DataUltimaMovimentacao);
                SELECT @@ROWCOUNT;";



                var _params = new DynamicParameters();
                _params.Add("@Id", transaction.NewUser.Id, DbType.Guid);
                _params.Add("@Nome", transaction.NewUser.Nome, DbType.String);
                _params.Add("@Email", transaction.NewUser.Email, DbType.String);
                _params.Add("@CPF", transaction.NewUser.CPF, DbType.String);
                _params.Add("@Login", transaction.NewUser.Login, DbType.String);
                _params.Add("@Password", transaction.NewUser.Password, DbType.String);
                _params.Add("@Status", transaction.NewUser.Status, DbType.Int32);
                _params.Add("@DataUltimaMovimentacao", DateTime.Now, DbType.DateTime);

                int rowsAffected = await _connection.ExecuteScalarAsync<int>(query, _params);
                return rowsAffected;



            });

            if (_result == 0)
                {
                    _logger.LogWarning("Nenhuma linha afetada na criação do usuário: {Email}", transaction.NewUser.Email);
                    return Result.Failure<Guid>("Falha ao criar usuário no banco de dados");
                }

                _logger.LogInformation("Usuário criado com sucesso no banco. ID: {UserId}, Email: {Email}",
                    transaction.NewUser.Id, transaction.NewUser.Email);

                activity?.SetTag("database.rows_affected", _result);


            return Result.Success(transaction.NewUser.Id);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário no banco. Email: {Email}", transaction.NewUser.Email);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Result.Failure<Guid>($"Erro ao criar usuário: {ex.Message}");
        }
    }

  
    public async Task<Result<Guid>> ExecStoredProcedureCreateAsync(TransactionCreateUser transaction, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity($"{nameof(UserRepository)}.{nameof(CreateAsync)}");
        activity?.SetTag("user.email", transaction.NewUser.Email);
        try
        {

            if (Global.ENVIRONMENT == MockEnviromentName)
            {
                _logger.LogInformation("Ambiente Mock", transaction.NewUser.Email);
                return Result.Success(transaction.NewUser.Id);
            }


            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {
                var _parameters = new DynamicParameters();

                //INPUT
                _parameters.Add("@pNome", transaction.NewUser.Nome);
                _parameters.Add("@pCPF", transaction.NewUser.CPF);
         

                //OUTPUT
                _parameters.Add("@pId", 0, DbType.Int32, ParameterDirection.InputOutput);


                await _connection.ExecuteAsync("spx_CreateNewUser", _parameters,
                        commandTimeout: _dbsettings.Value.CommandTimeout,
                        commandType: CommandType.StoredProcedure);
            });

            return Result.Success(transaction.NewUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário no banco. Email: {Email}", transaction.NewUser.Email);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Result.Failure<Guid>($"Erro ao criar usuário: {ex.Message}");
        }
    }


    public async Task<Result<bool>> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = false;

            if (Global.ENVIRONMENT == MockEnviromentName)
            {
                _logger.LogInformation("Ambiente Mock", false);
                return Result.Success(false);
            }

            await _dbConnection.ExecuteWithRetryAsync(async (_connection) =>
            {
                const string sql = @"
                SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
                FROM Users 
                WHERE Email = @Email AND Status != @DeletedStatus";

                exists = await _connection.QuerySingleAsync<bool>(sql, new
                {
                    Email = email,
                    DeletedStatus = (int)EnumStatus.Excluido
                });
            });

            return Result.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do usuário por email. Email: {Email}", email);
            return Result.Failure<bool>($"{ex.Message}");
        }
    }

   

 
}




