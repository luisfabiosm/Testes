namespace Domain.Core.SharedKernel.ResultPattern
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException();

            if (!isSuccess && string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
        public static Result<T> Failure<T>(string error) => new(default, false, error);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException("Cannot access value when result is failure");

                return _value!;
            }
        }

        protected internal Result(T? value, bool isSuccess, string error) : base(isSuccess, error)
        {
            _value = value;
        }
    }

}
