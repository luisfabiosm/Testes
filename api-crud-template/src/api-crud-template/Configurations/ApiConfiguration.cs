using Adapters.Inbound.API.Endpoints;
using Adapters.Inbound.API.Extensions;
using Adapters.Inbound.API.Middlewares;
using Domain.Core.SharedKernel;

namespace Configurations
{
    public static class ApiConfiguration
    {
        public static void UseAPIExtensions(this WebApplication app)
        {
            Global.ENVIRONMENT = app.Environment.EnvironmentName;
            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName != "Production")
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseApiMiddlewares();
            app.MapUserEndpoints();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHealthChecks("/health");

            // Endpoint de informações da API
            app.MapGet("/", () => new
            {
                Application = "CRUD Template API",
                Version = "1.0.0",
                Environment = app.Environment.EnvironmentName,
                Timestamp = DateTime.UtcNow
            });

            app.Run();
        }
    }

    
}
