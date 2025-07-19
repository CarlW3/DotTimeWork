namespace DotTimeWork.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public string? ErrorMessage { get; }
        public Exception? Exception { get; }

        private Result(bool isSuccess, T? value, string? errorMessage, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
        public static Result<T> Failure(Exception exception) => new(false, default, exception.Message, exception);

        public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
        {
            return IsSuccess && Value != null 
                ? Result<TOut>.Success(mapper(Value))
                : Result<TOut>.Failure(ErrorMessage ?? "Operation failed");
        }

        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess && Value != null)
            {
                action(Value);
            }
            return this;
        }

        public Result<T> OnFailure(Action<string> action)
        {
            if (IsFailure && ErrorMessage != null)
            {
                action(ErrorMessage);
            }
            return this;
        }
    }

    /// <summary>
    /// Represents the result of an operation without a return value
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? ErrorMessage { get; }
        public Exception? Exception { get; }

        private Result(bool isSuccess, string? errorMessage, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string errorMessage) => new(false, errorMessage);
        public static Result Failure(Exception exception) => new(false, exception.Message, exception);

        public Result OnSuccess(Action action)
        {
            if (IsSuccess)
            {
                action();
            }
            return this;
        }

        public Result OnFailure(Action<string> action)
        {
            if (IsFailure && ErrorMessage != null)
            {
                action(ErrorMessage);
            }
            return this;
        }
    }
}
