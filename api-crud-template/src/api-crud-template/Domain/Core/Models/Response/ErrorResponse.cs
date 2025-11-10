using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Domain.Core.Models.Response;


public record ErrorResponse
{
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string Instance { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string RequestId { get; init; } = Activity.Current?.Id ?? Guid.NewGuid().ToString();
    public Dictionary<string, string[]>? Errors { get; init; }

    public ErrorResponse()
    {
        
    }

    public static ErrorResponse BadRequest(string detail, Dictionary<string, string[]>? errors = null)
    {
        return new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = detail,
            Errors = errors
        };
    }

    public static ErrorResponse InternalServerError(string detail)
    {
        return new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = 500,
            Detail = detail
        };
    }

    public static ErrorResponse Unauthorized(string detail = "Token de autenticação inválido ou ausente")
    {
        return new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = 401,
            Detail = detail
        };
    }

    
}
