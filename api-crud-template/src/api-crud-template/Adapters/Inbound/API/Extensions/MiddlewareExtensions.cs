using Adapters.Inbound.API.Middlewares;

namespace Adapters.Inbound.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            return app;
        }
    }
}
