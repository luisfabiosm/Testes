
// Conditional using directives
using Adapters.Outbound.Database.SQL;

using Adapters.Outbound.Logging;
using Adapters.Outbound.Metrics;

namespace Configurations
{
    public static class OutboundConfiguration
    {
        public static IServiceCollection ConfigureOutboundAdapters(this IServiceCollection services, IConfiguration configuration)
        {

            #region Logging

            services.AddLoggingAdapter(configuration);

            #endregion region


            #region Metrics

            services.AddMetricsAdapter(configuration);

            #endregion



            #region Database SQL or PostgreSQL

            services.AddSQLAdapter(configuration);

            #endregion



            return services;
        }
    }
}
