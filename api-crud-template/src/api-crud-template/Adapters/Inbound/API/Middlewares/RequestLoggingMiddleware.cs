using System.Diagnostics;

namespace Adapters.Inbound.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly string[] SensitiveHeaders =
    {
    "authorization",
    "x-api-key",
    "cookie",
    "x-auth-token"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Gerar correlation ID se não existir
        if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
        {
            context.Request.Headers.TryAdd("X-Correlation-ID", Guid.NewGuid().ToString());
        }

        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

        // Adicionar correlation ID ao response
        if (!string.IsNullOrEmpty(correlationId))
        {
            context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);
        }

        var stopwatch = Stopwatch.StartNew();

        // Log da requisição
        LogRequest(context, correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log da resposta
            LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds);
        }
    }

    private void LogRequest(HttpContext context, string? correlationId)
    {
        var request = context.Request;

        // Filtrar headers sensíveis
        var filteredHeaders = request.Headers
            .Where(h => !SensitiveHeaders.Contains(h.Key.ToLower()))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        _logger.LogInformation("Requisição HTTP recebida: {Method} {Path} {QueryString} | " +
                              "CorrelationId: {CorrelationId} | " +
                              "UserAgent: {UserAgent} | " +
                              "RemoteIP: {RemoteIP} | " +
                              "Headers: {@Headers}",
            request.Method,
            request.Path,
            request.QueryString,
            correlationId,
            request.Headers["User-Agent"].FirstOrDefault(),
            context.Connection.RemoteIpAddress?.ToString(),
            filteredHeaders);

        // Adicionar informações ao Activity atual
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("http.method", request.Method);
            activity.SetTag("http.url", $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
            activity.SetTag("http.user_agent", request.Headers["User-Agent"].FirstOrDefault());
            activity.SetTag("correlation.id", correlationId);

            if (context.Connection.RemoteIpAddress != null)
            {
                activity.SetTag("http.client_ip", context.Connection.RemoteIpAddress.ToString());
            }
        }
    }

    private void LogResponse(HttpContext context, string? correlationId, long elapsedMs)
    {
        var response = context.Response;

        var logLevel = response.StatusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, "Resposta HTTP enviada: {Method} {Path} | " +
                              "StatusCode: {StatusCode} | " +
                              "ElapsedMs: {ElapsedMs} | " +
                              "CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            response.StatusCode,
            elapsedMs,
            correlationId);

        // Adicionar informações ao Activity atual
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("http.status_code", response.StatusCode);
            activity.SetTag("http.response_time_ms", elapsedMs);

            // Definir status da atividade baseado no código de resposta
            if (response.StatusCode >= 400)
            {
                activity.SetStatus(response.StatusCode >= 500
                    ? ActivityStatusCode.Error
                    : ActivityStatusCode.Ok,
                    $"HTTP {response.StatusCode}");
            }
            else
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }
        }

        // Log de performance para requisições lentas
        if (elapsedMs > 5000) // 5 segundos
        {
            _logger.LogWarning("Requisição lenta detectada: {Method} {Path} | " +
                              "ElapsedMs: {ElapsedMs} | " +
                              "CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                correlationId);
        }
    }
}
