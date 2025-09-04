using Domain.Core.Base;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using Polly;
using Polly.Retry;


namespace Adapters.Outbound.DBAdapter
{
    public class DBAdapterConnectionOld : BaseService, IDBAdapterConnection, IAsyncDisposable
    {
        #region Variáveis

        private readonly ILogger<DBAdapterConnectionOld> _logger;
        private readonly IOptions<DBSettings> _settings;
        private readonly AsyncRetryPolicy<IDbConnection> _retryPolicy;
        private SqlConnection _connection;
        private readonly SemaphoreSlim _semaphore;
        private const int MaxRetries = 3;



        #endregion

        public DBAdapterConnectionOld(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _settings = serviceProvider.GetRequiredService<IOptions<DBSettings>>();
            _logger = serviceProvider.GetRequiredService<ILogger<DBAdapterConnectionOld>>();
            _retryPolicy = CreateRetryPolicy();
            _semaphore = new SemaphoreSlim(1, 1);
        }


        public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    if (_connection == null || _connection.State != ConnectionState.Open)
                    {
                        await EnsureConnectionClosedAsync();
                        var _connectionString = _settings.Value.GetConnectionString();
                        if (string.IsNullOrEmpty(_connectionString))
                            throw new InvalidOperationException("Connectionstring não configurada");

                        _connection = new SqlConnection(_connectionString);
                        try
                        {
                            await _connection.OpenAsync(cancellationToken);
                            _logger.LogDebug("Database connection opened successfully");
                        }
                        catch (Exception ex)
                        {
                            LogError("GetConnectionAsync: OpenAsync ", ex);
                            await EnsureConnectionClosedAsync();
                            throw;
                        }
                    }
                    return _connection;
                });
            }
            catch (Exception ex)
            {
                LogError("GetConnectionAsync", ex);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<IDbConnection, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var connection = await GetConnectionAsync(cancellationToken);

                    if (connection.State != ConnectionState.Open)
                    {
                        LogInformation("ExecuteWithRetryAsync", "Conexão encontrada fechada antes da operação. Tentativa de reabertura.");
                        await EnsureConnectionClosedAsync();
                        connection = await GetConnectionAsync(cancellationToken);
                    }
 
                    return await operation(connection);
                }
                catch (SqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
                {
                    LogError("ExecuteWithRetryAsync", $"Ocorreu um erro temporário (Tentativas {MaxRetries})");
                    await Task.Delay(GetDelayMilliseconds(attempt), cancellationToken);
                    await EnsureConnectionClosedAsync();
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("closed") && attempt < MaxRetries)
                {
                    LogError("ExecuteWithRetryAsync", "Coneão fechada inesperadamente");
                    await Task.Delay(GetDelayMilliseconds(attempt), cancellationToken);
                    await EnsureConnectionClosedAsync();
                }
               
            }
            throw new InvalidOperationException($"Operação falhou após {MaxRetries} tentativas");
        }


        public async Task ExecuteWithRetryAsync(Func<IDbConnection, Task> operation, CancellationToken cancellationToken = default)
        {
            await ExecuteWithRetryAsync<object>(async (connection) =>
            {
                await operation(connection);
                return null;
            }, cancellationToken);

        }

        private async Task EnsureConnectionClosedAsync()
        {
            if (_connection == null) return;

            try
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error ao fechar conexão", ex.Message);

            }
            finally
            {
                _connection = null;
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (_connection == null) return;

            await _semaphore.WaitAsync();
            try
            {
                // Capture the connection locally to prevent null reference
                var connection = _connection;

                // Ensure connection is not null after semaphore acquisition
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        await connection.CloseAsync();
                    }

                    await connection.DisposeAsync();
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore if connection is already disposed
            }
            finally
            {
                _connection = null;
                _semaphore.Release();
            }
        }

        private bool IsTransientError(SqlException ex)
        {
            int[] transientErrorNumbers = { -2, 10060, 10061, 1205, 50000 }; // Added 50000 for connection closed
            return transientErrorNumbers.Contains(ex.Number);
        }
        private int GetDelayMilliseconds(int attempt)
        {
            return (int)Math.Pow(2, attempt) * 500; // Exponential backoff: 2s, 4s, 8s...
        }

        private AsyncRetryPolicy<IDbConnection> CreateRetryPolicy()
        {
            return Policy<IDbConnection>
                .Handle<SqlException>(ex => IsTransientError(ex))
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(
                    MaxRetries,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    async (exception, duration, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            $"Falha na tentativa de conexão {retryCount}. Nova tentativa em 500ms. Erro: {exception.Exception}",
                            retryCount,
                            duration.TotalMilliseconds,
                            exception.Exception.Message);

                        await EnsureConnectionClosedAsync();
                    }
                );
        }

        public string GetServer() => _settings.Value?.Cluster
        ?? throw new InvalidOperationException("Server cluster not configured");


        //private AsyncRetryPolicy<IDbConnection> CreateRetryPolicy()
        //{
        //    return Policy<IDbConnection>
        //.Handle<SqlException>(ex => new[] { -2, 10060, 10061, 1205 }.Contains(ex.Number))
        //.Or<InvalidOperationException>()
        //.WaitAndRetryAsync(
        //    3,
        //    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        //    (exception, duration, retryCount, context) =>
        //    {
        //        _logger.LogWarning(
        //            "Connection attempt {RetryCount} failed. Retrying in {Delay}ms. Error: {Error}",
        //            retryCount,
        //            duration.TotalMilliseconds,
        //            exception.Exception.Message);

        //        LogError($"Tentativa de conexão falhou. Tentar novamente em {2}ms", exception.Exception);

        //        return Task.CompletedTask;
        //    }
        //);
        //}

        public async ValueTask DisposeAsync()
        {
            await EnsureConnectionClosedAsync();
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
