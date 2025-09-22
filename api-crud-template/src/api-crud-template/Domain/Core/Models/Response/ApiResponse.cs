using System.Diagnostics;

namespace Domain.Core.Models.Response
{
    public record ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string RequestId { get; init; } = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        public static ApiResponse<T> OnSuccess(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> OnError(string message, T? data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}
