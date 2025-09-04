using Domain.Core.Ports.Outbound;
using Domain.Core.Settings;

namespace Adapters.Outbound.Database.SQL
{
    public static class SQLAdapterExtensions
    {
        public static IServiceCollection AddSQLAdapter(this IServiceCollection services, IConfiguration configuration)
        {

            #region SQL SERVER or Postgresql Session Management


            services.Configure<DBSettings>(options =>
            {
                var _settings = configuration.GetSection("AppSettings:DB");

                options.ServerUrl = Environment.GetEnvironmentVariable("SPA_CLUSTER_SERVER") ?? _settings.GetValue<string>("ServerUrl");
                options.Username = Environment.GetEnvironmentVariable("SPA_USER") ?? _settings.GetValue<string>("Username");
                options.Password = Environment.GetEnvironmentVariable("SPA_CRIPT_PASSWORD") ?? _settings.GetValue<string>("Password");
                options.Database = Environment.GetEnvironmentVariable("SPA_DB") ?? _settings.GetValue<string>("Database");
                options.CommandTimeout = _settings.GetValue<int>("CommandTimeout");
                options.ConnectTimeout = _settings.GetValue<int>("ConnectTimeout");
            });


            services.AddScoped<ISQLConnectionAdapter, SQLConnectionAdapter>();
            services.AddScoped<ISPARepository, SPARepository>();

            return services;

            #endregion
        }
    }
}
