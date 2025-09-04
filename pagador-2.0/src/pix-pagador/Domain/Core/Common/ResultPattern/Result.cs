namespace Domain.Core.Common.ResultPattern;

public readonly struct Result<T>
{
    public readonly bool IsSuccess;
    public readonly T Value;
    public readonly string Error;
    public readonly int ErrorCode;


    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
        ErrorCode = 0;
    }

    private Result(string error, int errorCode = -1)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the provided error message.
    /// </summary>
    public static Result<T> Failure(string error, int errorCode = -1) => new(error, errorCode);

    /// <summary>
    /// Pattern matching for handling both success and failure cases.
    /// Evita if/else statements e melhora a legibilidade.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, int, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error, ErrorCode);
    }

    /// <summary>
    /// Maps the value to a new type if the result is successful.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(Value))
            : Result<TNew>.Failure(Error, ErrorCode);
    }

    /// <summary>
    /// Binds the result to a new operation if successful.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess ? binder(Value) : Result<TNew>.Failure(Error, ErrorCode);
    }

    /// <summary>
    /// Implicit conversion from T to Result<T> for convenience.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Returns the value if successful, otherwise throws the specified exception.
    /// Use apenas quando necessário para compatibilidade com código legacy.
    /// </summary>
    public T GetValueOrThrow<TException>(Func<string, int, TException> exceptionFactory)
        where TException : Exception
    {
        return IsSuccess ? Value : throw exceptionFactory(Error, ErrorCode);
    }
}