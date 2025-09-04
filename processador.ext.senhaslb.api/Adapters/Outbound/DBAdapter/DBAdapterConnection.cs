using Microsoft.Extensions.Options;
using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;
using System.Data.SqlClient;
using Domain.Core.Base;
using System.Data;
using Polly.Retry;
using Polly;

namespace Adapters.Outbound.DBAdapter
{
    public class DBAdapterConnection : BaseService, IDBAdapterConnection, IAsyncDisposable
    {
        #region variáveis

        private readonly ILogger<DBAdapterConnection> _iLogger;
        private readonly IOptions<DBSettings> _settings;
        private readonly AsyncRetryPolicy<IDbConnection> _retryPolicy;
        private SqlConnection? _connection;
        private readonly SemaphoreSlim _semaphore;
        private const int MaxRetries = 3;

        #endregion

        public DBAdapterConnection(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _settings = serviceProvider.GetRequiredService<IOptions<DBSettings>>();
            _iLogger = serviceProvider.GetRequiredService<ILogger<DBAdapterConnection>>();
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

                        var _newConnection = new SqlConnection(_connectionString);
                        try
                        {
                            await _newConnection.OpenAsync(cancellationToken);
                            _connection = _newConnection;
                            _iLogger.LogDebug("Database connection opened successfully");
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
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<IDbConnection, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                IDbConnection? _newConnection = null;
                try
                {
                    _newConnection = await GetConnectionAsync(cancellationToken);

                    if (_newConnection.State != ConnectionState.Open)
                    {
                        LogInformation("ExecuteWithRetryAsync", "Conexão encontrada fechada antes da operação. Tentativa de reabertura.");
                        await EnsureConnectionClosedAsync();
                        _newConnection = await GetConnectionAsync(cancellationToken);
                    }
 
                    return await operation(_newConnection);
                }
                catch (SqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
                {
                    LogError("ExecuteWithRetryAsync", $"Ocorreu um erro temporário (Tentativas {MaxRetries})");
                    await Task.Delay(GetDelayMilliseconds(attempt), cancellationToken);
                    await EnsureConnectionClosedAsync();
                }
                catch (InvalidOperationException ex) when ((ex.Message.Contains("closed") ||
                                                      ex.Message.Contains("open")) &&
                                                      attempt < MaxRetries)
                {
                    LogError("ExecuteWithRetryAsync", "Conexão fechada inesperadamente");
                    await Task.Delay(GetDelayMilliseconds(attempt), cancellationToken);
                    await EnsureConnectionClosedAsync();
                }
            }

            throw new InvalidOperationException($"Operação falhou após {MaxRetries} tentativas");
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

        public string GetServer() => _settings.Value?.Cluster
        ?? throw new InvalidOperationException("Server cluster not configured");

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
                        _iLogger.LogWarning(
                            $"Falha na tentativa de conexão {retryCount}. Nova tentativa em 500ms. Erro: {exception.Exception}",
                            retryCount,
                            duration.TotalMilliseconds,
                            exception.Exception.Message);

                        await EnsureConnectionClosedAsync();
                    }
                );
        }

        private async Task EnsureConnectionClosedAsync()
        {
            if (_connection == null) return;

            var connectionToDispose = _connection;
            _connection = null; // Clear reference first to prevent race conditions
            await DisposeConnectionAsync(connectionToDispose);
        }

        private async Task DisposeConnectionAsync(SqlConnection connection)
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
                LogError("Error closing connection", ex.Message);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await EnsureConnectionClosedAsync();
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
