using Domain.Core.Models.Settings;
using Domain.Core.Ports.Outbound;

namespace Adapters.Outbound.DBAdapter.Configuration
{
    public static class DBAdapterConfiguration
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            #region SQL SERVER Session Management

            services.Configure<DBSettings>(options =>
            {
                var _settings = configuration.GetSection("AppSettings:DB");
                
                options.Cluster = Environment.GetEnvironmentVariable("SPA_CLUSTER_SERVER")?? _settings.GetValue<string>("Cluster")!;          
                options.Username = Environment.GetEnvironmentVariable("SPA_USER")?? _settings.GetValue<string>("Username")!;
                options.Password = Environment.GetEnvironmentVariable("SPA_CRIPT_PASSWORD")?? _settings.GetValue<string>("Password")!;
                options.Database = Environment.GetEnvironmentVariable("SPA_DB")?? _settings.GetValue<string>("Database")!;

                options.CommandTimeout = _settings.GetValue<int>("CommandTimeout");
                options.ConnectTimeout = _settings.GetValue<int>("ConnectTimeout");
                options.IsTraceExecActive = _settings.GetValue<bool>("IsTraceExecActive");

                Console.WriteLine($"SPA_CLUSTER_SERVER: {options.Cluster}");
            });

            services.AddScoped<IDBAdapterConnection, DBAdapterConnection>();
            services.AddScoped<ISPARepository, SPARepository>();
    
            return services;

            #endregion
        }
    }
}
