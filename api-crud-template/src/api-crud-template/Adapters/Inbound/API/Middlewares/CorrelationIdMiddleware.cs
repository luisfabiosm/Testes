using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Adapters.Inbound.API.Middlewares;


public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Obter ou gerar Correlation ID
        var correlationId = GetCorrelationId(context);

        // 2. Adicionar ao contexto HTTP
        context.Items["CorrelationId"] = correlationId;

        // 3. Adicionar ao contexto de logging do Serilog
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            // 4. Adicionar ao header de resposta
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
                return Task.CompletedTask;
            });

            // 5. Adicionar ao Activity para OpenTelemetry
            Activity.Current?.SetTag("correlation.id", correlationId);

            // 6. Executar próximo middleware
            await _next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        // Tentar pegar do header da requisição
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            return correlationId.FirstOrDefault() ?? GenerateCorrelationId();
        }

        // Gerar novo correlation ID
        return GenerateCorrelationId();
    }

    private static string GenerateCorrelationId() => Guid.NewGuid().ToString("D");
}