using Adapters.Outbound.Database.SQL;
using Adapters.Outbound.Logging;


namespace Configurations;

public static class OutboundConfiguration
{
    public static IServiceCollection ConfigureOutbound(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        #region Logging

        services.AddLoggingAdapter(configuration);

        #endregion region


        #region Database SQL 

        services.AddSQLAdapter(configuration);

        #endregion



        return services;
    }



   
}