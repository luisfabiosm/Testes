using Domain.Core.Models.Response;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Adapters.Inbound.API.Middlewares;

// ===== EXCEPTION HANDLING MIDDLEWARE =====

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Erro não tratado na aplicação");

        // Definir atividade de trace com erro
        Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        ErrorResponse errorResponse;

        switch (exception)
        {
            case ArgumentException:
            case NullReferenceException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.BadRequest(exception.Message);
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = ErrorResponse.Unauthorized(exception.Message);
                break;


            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse = new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.7",
                    Title = "Request Timeout",
                    Status = 408,
                    Detail = "A operação excedeu o tempo limite"
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Em desenvolvimento, mostra detalhes do erro
                var detail = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? exception.Message
                    : "Ocorreu um erro interno no servidor";

                errorResponse = ErrorResponse.InternalServerError(detail);
                break;
        }

        // Adicionar informações do contexto
        errorResponse = errorResponse with
        {
            Instance = context.Request.Path,
            RequestId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
        });

        await response.WriteAsync(jsonResponse);
    }
}
