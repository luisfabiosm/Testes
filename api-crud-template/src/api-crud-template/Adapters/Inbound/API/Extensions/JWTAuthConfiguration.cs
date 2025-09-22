using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Adapters.Inbound.API.Extensions
{
    public static class JWTAuthConfiguration
    {

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {

            //Token de 1 ano: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJhcGktY3J1ZC10ZW1wbGF0ZSIsImF1ZCI6ImFwaS1jcnVkLXRlbXBsYXRlIiwiaWF0IjoxNzU4MzExMjk3LCJleHAiOjE3ODk5MzM2OTd9.ekMhaR-lUtS1sNWsERLVDCE1FlAuLuMW9sIZIS4TeKY
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = configuration["Jwt:Issuer"] ?? "api-crud-template",
                     ValidAudience = configuration["Jwt:Audience"] ?? "api-crud-template",
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "super-secret-jwt-api-crud-template-2025-Development"))
                 };
             });

            services.AddAuthorization();

            return services;
        }
    }
}
