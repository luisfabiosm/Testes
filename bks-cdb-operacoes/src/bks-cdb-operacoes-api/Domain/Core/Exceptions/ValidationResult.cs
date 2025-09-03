using Domain.Core.Common.ResultPattern;

namespace Domain.Core.Exceptions;

public readonly struct ValidationResult
{
    public readonly bool IsValid;
    public readonly List<ValidationErrorDetails> Errors;

    private ValidationResult(bool isValid, List<ValidationErrorDetails> errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<ValidationErrorDetails>();
    }

    public static ValidationResult Valid() => new(true);

    public static ValidationResult Invalid(List<ValidationErrorDetails> errors) => new(false, errors);

    public static ValidationResult Invalid(string message, string field = null)
    {
        var errors = new List<ValidationErrorDetails> { new(field ?? "Unknown", message) };
        return new(false, errors);
    }


    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var allErrors = new List<ValidationErrorDetails>();
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


    public Result<T> ToResult<T>(T value)
    {
        return IsValid
            ? Result<T>.Success(value)
            : Result<T>.Failure(string.Join("; ", Errors.Select(e => e.mensagem)));
    }
}