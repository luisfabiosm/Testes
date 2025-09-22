namespace Domain.Core.SharedKernel.ResultPattern
{
    public static class ResultExtensions
    {
        public static async Task<Result<TOut>> Map<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, TOut> mapper)
        {
            var result = await resultTask;
            return result.IsSuccess
                ? Result.Success(mapper(result.Value))
                : Result.Failure<TOut>(result.Error);
        }

        public static async Task<Result> Bind<T>(
            this Task<Result<T>> resultTask,
            Func<T, Task<Result>> binder)
        {
            var result = await resultTask;
            return result.IsSuccess
                ? await binder(result.Value)
                : Result.Failure(result.Error);
        }
    }
}
