using Adapters.Outbound.Database.SQL;
using Domain.Core.Constant;
using Domain.Core.Ports.Outbound;
using System.Diagnostics;

namespace Adapters.Inbound.WebAPI.Endpoints
{
    public static partial class MonitorEndpoint
    {

        public static void AddMonitorEndpoints(this WebApplication app)
        {
            var monitoringGroup = app.MapGroup("bks/cdb/operacoes/monitoring")
                               .WithTags("Monitoramento")
                               .AllowAnonymous();


            monitoringGroup.MapGet("/health/detailed", async (
            IServiceProvider serviceProvider, ISQLConnectionAdapter _dbConnection) =>
                    {
                        var checks = new Dictionary<string, object>
                        {
                            ["timestamp"] = DateTime.UtcNow,
                            ["version"] = "2.0.0",
                            ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                            ["machineName"] = Environment.MachineName,
                            ["processId"] = Environment.ProcessId,
                            ["SQLConnectionState"] = _dbConnection.GetConnectionState() ,//OperationConstants.CONNECTION_STATE,
                            ["SQLTotalConnectionsOn"] = OperationConstants.CONNECTIONS_ACTIVE,
                            ["SQLTotalConnectionsClosed"] = OperationConstants.CONNECTIONS_CLOSED

                        };

                        // Adicionar checks específicos se necessário
                        // var dbCheck = await CheckDatabaseConnection(serviceProvider);
                        // checks["database"] = dbCheck;

                        return Results.Ok(checks);
                    })
        .WithName("DetailedHealthCheck")
        .WithSummary("Health check detalhado da aplicação")
        .CacheOutput("NoCache");



        monitoringGroup.MapGet("/metrics", () =>
        {
            var metrics = new
            {
                memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024, // MB
                gcCollections = new
                {
                    gen0 = GC.CollectionCount(0),
                    gen1 = GC.CollectionCount(1),
                    gen2 = GC.CollectionCount(2)
                },
                threadCount = ThreadPool.ThreadCount,
                uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
            };

            return Results.Ok(metrics);
        })
        .WithName("GetMetrics")
        .WithSummary("Métricas básicas da aplicação")
        .CacheOutput("PixCache");
        }
    }
}
