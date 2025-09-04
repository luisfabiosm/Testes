using System.Data;

namespace Domain.Core.Ports.Outbound
{
    public interface IDBAdapterConnection
    {
        Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
        Task CloseConnectionAsync();
        Task<T> ExecuteWithRetryAsync<T>(Func<IDbConnection, Task<T>> operation, CancellationToken cancellationToken = default);
    }
}
