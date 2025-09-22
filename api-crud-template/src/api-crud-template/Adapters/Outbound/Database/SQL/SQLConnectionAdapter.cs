using Dapper;
using Domain.Core.Interfaces.Outbound;
using Domain.Core.Settings;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Data;
using DbConnection = Microsoft.Data.SqlClient.SqlConnection;
using DbException = Microsoft.Data.SqlClient.SqlException;

namespace Adapters.Outbound.Database.SQL
{


    public class SQLConnectionAdapter : ISQLConnectionAdapter, IAsyncDisposable
    {
        private readonly IOptions<DatabaseSettings> _settings;
        private readonly AsyncRetryPolicy<IDbConnection> _retryPolicy;
        private DbConnection _connection;
        private readonly SemaphoreSlim _semaphore;
        private const int MaxRetries = 3;
        private string _CorrelationId;


        public SQLConnectionAdapter(IServiceProvider serviceProvider) 
        {
            _settings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>();
            _retryPolicy = CreateRetryPolicy();
            _semaphore = new SemaphoreSlim(1, 1);
        }


        public void SetCorrelationId(string correlationId)
        {
            _CorrelationId = correlationId;
        }

        public ConnectionState GetConnectionState() => _connection == null ? ConnectionState.Closed : _connection.State;


        public string GetServer() => _settings.Value?.Cluster ?? throw new InvalidOperationException("Server não configurado");

        public async Task CloseConnectionAsync()
        {
            if (_connection == null)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                var connection = _connection;

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
                        {
                            var ex = new InvalidOperationException("Connectionstring não configurada");
                            throw ex;
                        }

                        var _newConnection = new DbConnection(_connectionString);
                        try
                        {
                            await _newConnection.OpenAsync(cancellationToken);
                            _connection = _newConnection;
                        }
                        catch (Exception ex)
                        {
                            await EnsureConnectionClosedAsync();
                            throw;
                        }
                    }

                    return _connection;
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ExecuteWithRetryAsync(Func<IDbConnection, Task> operation, CancellationToken cancellationToken = default)
        {
            await ExecuteWithRetryAsync<object>(async (connection) =>
            {
                await operation(connection);
                return null;
            }, cancellationToken);
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<IDbConnection, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {

                IDbConnection connection = null;
                {
                    try
                    {
                        connection = await GetConnectionAsync(cancellationToken);
                        if (connection.State != ConnectionState.Open)
                        {
                            await EnsureConnectionClosedAsync();
                            connection = await GetConnectionAsync(cancellationToken);
                        }

                        return await operation(connection);
                    }
                    catch (DbException ex) when (IsTransientError(ex) && attempt < MaxRetries)
                    {
                        await Task.Delay(GetDelayMilliseconds(attempt), cancellationToken);
                        await EnsureConnectionClosedAsync();
                    }
                    catch (InvalidOperationException ex) when ((ex.Message.Contains("closed") || ex.Message.Contains("open")) && attempt < MaxRetries)
                    {
                        await Task.Delay(GetDelayMilliseconds(attempt), cancellationToken);
                        await EnsureConnectionClosedAsync();
                    }
                    throw new InvalidOperationException($"Operação falhou após {MaxRetries} tentativas");
                }
            }
            throw new InvalidOperationException($"A operação falhou após {MaxRetries} tentativas");
        }


        #region PRIVATE 

        private bool IsTransientError(DbException ex)
        {
            return false;
        }

        private AsyncRetryPolicy<IDbConnection> CreateRetryPolicy()
        {
            return Policy<IDbConnection>
                .Handle<DbException>(ex => IsTransientError(ex))
                .Or<InvalidOperationException>()
                .WaitAndRetryAsync(
                    MaxRetries,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    async (exception, duration, retryCount, context) =>
                    {
                        await EnsureConnectionClosedAsync();
                    }
                );
        }

        private async Task EnsureConnectionClosedAsync()
        {
            if (_connection == null) return;

            var connectionToDispose = _connection;
            _connection = null;
            await DisposeConnectionAsync(connectionToDispose);
        }

        private async Task DisposeConnectionAsync(DbConnection connection)
        {

            if (connection == null) return;

            try
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
                await connection.DisposeAsync();
            }
            catch (Exception ex)
            {
            }
        }

        private int GetDelayMilliseconds(int attempt)
        {
            return (int)Math.Pow(2, attempt) * 500; // Exponential backoff: 2s, 4s, 8s...
        }


        #endregion

        #region Dapper Extension Methods for Convenience

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async connection =>
            {
                return await connection.QueryAsync<T>(sql, param);
            }, cancellationToken);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async connection =>
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
            }, cancellationToken);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async connection =>
            {
                return await connection.ExecuteAsync(sql, param);
            }, cancellationToken);
        }

        #endregion


        public async ValueTask DisposeAsync()
        {

            await EnsureConnectionClosedAsync();
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }

    }

}
