using Domain.Core.Common.ResultPattern;

namespace Domain.Core.Exceptions;

public readonly struct ValidationResult
{
    public readonly bool IsValid;
    public readonly List<ErrorDetails> Errors;

    private ValidationResult(bool isValid, List<ErrorDetails> errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<ErrorDetails>();
    }

    public static ValidationResult Valid() => new(true);

    public static ValidationResult Invalid(List<ErrorDetails> errors) => new(false, errors);

    public static ValidationResult Invalid(string message, string field = null)
    {
        var errors = new List<ErrorDetails> { new(field ?? "Unknown", message) };
        return new(false, errors);
    }

    /// <summary>
    /// Combina múltiplos resultados de validação.
    /// </summary>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var allErrors = new List<ErrorDetails>();
        var isValid = true;

        foreach (var result in results)
        {
            if (!result.IsValid)
            {
                isValid = false;
                allErrors.AddRange(result.Errors);
            }
        }

        return isValid ? Valid() : Invalid(allErrors);
    }

    /// <summary>
    /// Converte para Result<T> pattern.
    /// </summary>
    public Result<T> ToResult<T>(T value)
    {
        return IsValid
            ? Result<T>.Success(value)
            : Result<T>.Failure(string.Join("; ", Errors.Select(e => e.mensagens)));
    }
}