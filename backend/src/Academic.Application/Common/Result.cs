namespace Academic.Application.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public Error? Error { get; protected set; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; protected set; }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string code, string message) => new()
    {
        IsSuccess = false,
        Error = new Error(code, message)
    };

    public static Result ValidationFailure(IReadOnlyDictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        Error = new Error("validation_error", "Validation error."),
        ValidationErrors = validationErrors
    };
}

public sealed class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public new static Result<T> Failure(string code, string message) => new()
    {
        IsSuccess = false,
        Error = new Error(code, message)
    };

    public new static Result<T> ValidationFailure(IReadOnlyDictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        Error = new Error("validation_error", "Validation error."),
        ValidationErrors = validationErrors
    };
}
