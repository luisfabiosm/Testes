using Adapters.Inbound.API.Extensions;
using Domain.Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Configurations;

public static class InboundConfiguration
{
    public static IServiceCollection ConfigureInbound( this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar Minimal APIs
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });


        services.AddEndpointsApiExplorer();
        services.AddHealthChecks();
        services.AddJwtAuthentication(configuration);
        services.ConfigureSwagger();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

         // Health Checks
        //services.AddHealthChecks().AddSqlServer(configuration.GetConnectionString("DefaultConnection")!);

        return services;
    }
}