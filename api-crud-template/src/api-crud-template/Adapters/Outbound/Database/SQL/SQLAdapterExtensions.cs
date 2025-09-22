using Domain.Core.Interfaces.Outbound;
using Domain.Core.Settings;

namespace Adapters.Outbound.Database.SQL
{
    public static class SQLAdapterExtensions
    {
        public static IServiceCollection AddSQLAdapter(this IServiceCollection services, IConfiguration configuration)
        {

            #region SQL SERVER or Postgresql Session Management


            services.Configure<DatabaseSettings>(options =>
            {
                var _settings = configuration.GetSection("AppSettings:DB");

                options.Cluster = Environment.GetEnvironmentVariable("CLUSTER_SERVER") ?? _settings.GetValue<string>("Cluster");
                options.Username = Environment.GetEnvironmentVariable("USER") ?? _settings.GetValue<string>("Username");
                options.Password = Environment.GetEnvironmentVariable("CRIPT_PASSWORD") ?? _settings.GetValue<string>("Password");
                options.Database = Environment.GetEnvironmentVariable("DB") ?? _settings.GetValue<string>("Database");
                options.CommandTimeout = _settings.GetValue<int>("CommandTimeout");
                options.ConnectTimeout = _settings.GetValue<int>("ConnectTimeout");
            });


            services.AddScoped<ISQLConnectionAdapter, SQLConnectionAdapter>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;

            #endregion
        }
    }
}
