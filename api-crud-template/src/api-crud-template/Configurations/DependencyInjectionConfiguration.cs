using Domain.Core.Settings;

namespace Configurations;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection ConfigureDependencyInjection( this IServiceCollection services, IConfiguration configuration)
    {
        // Configurações principais
        services.ConfigureSettings(configuration);

        // Configurar camadas
        services.ConfigureInbound(configuration);
        services.ConfigureOutbound(configuration);
        services.ConfigureDomain(configuration);

        return services;
    }

    private static IServiceCollection ConfigureSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind das configurações
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("AppSettings:Jwt"));
        services.Configure<DatabaseSettings>(configuration.GetSection("AppSettings:DB"));
        services.Configure<OtlpSettings>(configuration.GetSection("AppSettings:Otlp"));

        return services;
    }
}