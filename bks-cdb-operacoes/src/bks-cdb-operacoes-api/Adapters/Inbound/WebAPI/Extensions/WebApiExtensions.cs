using Adapters.Inbound.WebApi.Middleware;
using Adapters.Inbound.WebAPI.Endpoints;
using Microsoft.OpenApi.Models;

namespace Adapters.Inbound.WebApi.Extensions
{
    public static class WebApiExtensions
    {
        public static IServiceCollection addWebApiEndpoints(this IServiceCollection services, IConfiguration configuration)
        {


            services.AddEndpointsApiExplorer();
            services.AddHealthChecks();
            services.AddJwtAuthentication(configuration);

            return services;
        }


        public static IServiceCollection ConfigureSwagger(this IServiceCollection services, string apiName, string version = "v1")
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BKS CDB Operações",
                    Version = "v1",
                    Description = "API de operações bancárias CDB.",
                    Contact = new OpenApiContact
                    {
                        Name = "W3 Dev Team",
                        Email = "fabio.magalhaes@w3as.com.br"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Cabeçalho de autorização JWT usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.OperationFilter<SwaggerMinimalApiOperationFilter>();
            });

            return services;
        }

        public static void UseAPIExtensions(this WebApplication app)
        {

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName != "Production")
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseHandleResponseResultMiddleware();
            app.AddCDBEndpoints();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHealthChecks("/health");
            app.AddMonitorEndpoints();
            app.Run();
        }
    }
}
