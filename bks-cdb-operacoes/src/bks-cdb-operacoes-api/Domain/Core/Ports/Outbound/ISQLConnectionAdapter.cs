using System.Data;
using System.Data.Common;

namespace Domain.Core.Ports.Outbound
{
    public interface ISQLConnectionAdapter
    {

        void SetCorrelationId(string correlationId);

        ConnectionState GetConnectionState() ;

        Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);


        Task CloseConnectionAsync();


        Task<T> ExecuteWithRetryAsync<T>(Func<IDbConnection, Task<T>> operation, CancellationToken cancellationToken = default);


        Task ExecuteWithRetryAsync(Func<IDbConnection, Task> operation, CancellationToken cancellationToken = default);


        // Dapper extension methods
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default);

        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default);

        Task<int> ExecuteAsync(string sql, object param = null, CancellationToken cancellationToken = default);


    }
}
